using System;
using System.Collections;
using Project.Input;
using UnityEngine;

namespace Project.Gameplay
{
    public class PlayerMovement : MonoBehaviour, IWalkable
    {
        #region ПОЛЯ
        [Header("Схема управления"), SerializeField]
        private PlayerControlScheme _playerControlScheme = null;
        [Header("Направление игрока"), SerializeField]
        private Direction _direction = Direction.Right;
        [Header("Кривая силы прыжка"), SerializeField]
        private AnimationCurve _jumpForceCurve;

        //СКОРОСТЬ
        //Текущая скорость перемещения
        private float _currentSpeed = 0;
        //Максимальная скорость перемещения
        private const float MAX_SPEED = 0.08f; 

        //УСКОРЕНИЕ
        //Текущее ускорение
        private float _currentAcceleration = 0;
        //Максимальное ускорение
        private float _currentMaxAcceleration = 0.16f; 
        //Максимальное ускорение на земле
        private const float GROUND_MAX_ACCELERATION = 0.16f;
        //Максимальное ускорение в воздухе при изменении направления
        private const float AIR_MAX_ACCELERATION = 0.4f;
        //Множитель ускорения
        private const float ACCELERATION_MULTIPLIER = 2f; 

        //ПРЫЖОК
        //Выполняется ли прыжок
        private bool _isJumping = false;
        //Время прыжка
        private float _jumpTime = 0;
        //Скорость увеличения времени прыжка
        private const float JUMP_TIME_INCREASE_SPEED = 4f;

        //ПАДЕНИЕ
        //Предыдущая вертикальная скорость
        private float _previousVerticalVelocity = 0;
        //Множитель скорости контроллируемого падения
        private Vector2 _fallMultiplier = new Vector2(0, -500);

        //Приземлился ли игрок 
        private bool _isGrounded = false;
        //Направление движения по Х 
        private int _horizontalAxis = 0;
        //Упёрся ли игрок в стену
        private bool _isStuckedInWall = false;

        //Компоненты
        private Transform _transform = null;
        private Rigidbody2D _rigidBody = null;
        private IWalkable _movement = null;
        #endregion


        #region СВОЙСТВА
        /// <summary>
        /// Приземлился ли игрок
        /// </summary>
        public bool isGrounded
        {
            get => _isGrounded;
            private set
            {
                if(_isGrounded == value)
                    return;

                _isGrounded = value;

                if(_isGrounded){
                    OnLanding();
                }else{
                    OnTakingOff();
                }
            }
        }
        /// <summary>
        /// Направление игрока
        /// </summary>
        public Direction direction
        {
            get => _direction;
            private set
            {
                //То же значение или такого значения в перечислении нет
                if(_direction == value || !Enum.IsDefined(typeof(Direction), (int)value))
                    return;

                _direction = value;
                _transform.SetlocalScaleX((int)_direction);
                HorizontalDirectionChanged?.Invoke();
            }
        }
        /// <summary>
        /// Передвигается ли игрок
        /// </summary>
        public bool isMoving => _horizontalAxis != 0;
        /// <summary>
        /// Вертикальная скорость игрока. <0 - падает, >0 - взлетает, 
        /// </summary>
        public float verticalVelocity => _rigidBody.velocity.y;

        //Абсолютное значение текущей скорости
        private float _currentSpeedAbs => Mathf.Abs(_currentSpeed);
        //Максимальная скорость при приземлении после долгого падения
        private float _maxSpeedAfterLongFall => MAX_SPEED / 4;
        #endregion


        #region СОБЫТИЯ
        /// <summary>
        /// Оторвался от земли
        /// </summary>
        public event Action TookOff;
        /// <summary>
        /// Игрок приземлился (скорость при приземлении)
        /// </summary>
        public event Action<float> Landed;
        
        /// <summary>
        /// Движение начинается
        /// </summary>
        public event Action RunStarted;
        /// <summary>
        /// Движение заканчивается
        /// </summary>
        public event Action RunEnded;
        /// <summary>
        /// Прыжок
        /// </summary>
        public event Action Jump;
        /// <summary>
        /// Изменилось горизонтальное направление
        /// </summary>
        public event Action HorizontalDirectionChanged;
        /// <summary>
        /// Изменилось вертикальное направление
        /// </summary>
        public event Action VerticalDirectionChanged;
        #endregion


        #region MONOBEHAVIOUR
#if UNITY_EDITOR
        private void OnValidate()
        {
            //Поворачиваем игрока в правильном направлении
            this.direction = _direction;
        }
#endif
        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _rigidBody = GetComponent<Rigidbody2D>();
            _movement = GetComponent<IWalkable>();
        }
        private void OnEnable()
        {
            _playerControlScheme.MoveStarted += OnMoveStarted;
            _playerControlScheme.MoveEnded += OnMoveEnded;
            _playerControlScheme.JumpStarted += OnJumpStarted;
            _playerControlScheme.JumpEnded += OnJumpEnded;
        }
        private void OnDisable()
        {
            _playerControlScheme.MoveStarted -= OnMoveStarted;
            _playerControlScheme.MoveEnded -= OnMoveEnded;
            _playerControlScheme.JumpStarted -= OnJumpStarted;
            _playerControlScheme.JumpEnded -= OnJumpEnded;
        }
        private void OnDestroy()
        {
            RunStarted = null;
            RunEnded = null;
            Jump = null;
            TookOff = null;
            Landed = null;
            HorizontalDirectionChanged = null;
            VerticalDirectionChanged = null;
        }
        //Физика
        private void FixedUpdate()
        {
            #region Обработка прыжка
            //Если выполняется прыжок - уменьшаем время прыжка
            if(_isJumping && _jumpTime < 1){
                //Увеличиваем время прыжка
                _jumpTime = Mathf.Clamp(_jumpTime + JUMP_TIME_INCREASE_SPEED * Time.deltaTime, 0, 1);
                //Устанавливаем силу прыжка относительно времени
                _rigidBody.velocity = Vector2.up * _jumpForceCurve.Evaluate(_jumpTime);
            }
            //Перестаём выполнять прыжок, если время прыжка закончилось
            if(_jumpTime == 1 && _isJumping)
                OnJumpEnded();
            #endregion

            #region Обработка падения
            if(!_isGrounded){
                //Увеличиваем гравитацию при контроллируемом падении
                if(this.verticalVelocity < 0)
                    _rigidBody.AddForce(_fallMultiplier * Time.deltaTime, ForceMode2D.Force);

                //Изменяем максимальное ускорение и множитель ускорения, если игрок меняет горизонтальное направление в воздухе,
                //необходимо для более быстрого маневрирования в воздухе
                if(_horizontalAxis * _currentSpeed < 0)
                    _currentMaxAcceleration = AIR_MAX_ACCELERATION;

                //При изменении направления вертикальной скорости, вызываем событие
                if(this.verticalVelocity * _previousVerticalVelocity <= 0)
                    VerticalDirectionChanged?.Invoke();
 
                //Сохраняем предыдущее значение вертикальной скорости
                _previousVerticalVelocity = this.verticalVelocity;
            }
            #endregion
                
            #region Обработка ускорения и скорости передвижения
            if(_horizontalAxis != 0 && !_isStuckedInWall){
                //Вычисляем новое ускорение
                _currentAcceleration += ACCELERATION_MULTIPLIER * Time.deltaTime;
                //Ограничиваем максимальное ускорение
                _currentAcceleration = Mathf.Clamp(_currentAcceleration, 0, _currentMaxAcceleration);

                //Скорость += направление движения * ускорение
                _currentSpeed += _horizontalAxis * _currentAcceleration * Time.deltaTime;
                //Ограничиваем максимальную скорость перемещения
                _currentSpeed = Mathf.Clamp(_currentSpeed, -MAX_SPEED, MAX_SPEED);
            }
                
            //Устанавливаем новую позицию
            _rigidBody.SetPositionX(_rigidBody.position.x + _currentSpeed * Time.timeScale);
            #endregion
        }
        #endregion


        #region ПРИВАТНЫЕ МЕТОДЫ И ФУНКЦИИ
        //Горизонтальное перемещение
        private void OnMoveStarted(int value)
        {
            //Устанавливаем направление движения
            _horizontalAxis = value;
            //Поворачиваем игрока в нужную сторону
            this.direction = (Direction)_horizontalAxis;
            //Начинаем движение
            RunStarted?.Invoke();
        }
        private void OnMoveEnded()
        {
            //Устанавливаем направление движения
            _horizontalAxis = 0;
            //Убираем ускорение
            _currentAcceleration = 0;
            //Сбрасываем текущую скорость, если игрок на земле
            if(_isGrounded)
                _currentSpeed = 0;
            //Заканчиваем движение
            RunEnded?.Invoke();
        }
        //Прыжок
        private void OnJumpStarted()
        {
            if(!_isGrounded)
                return;

            _isJumping = true;
            this.isGrounded = false;

            //Вызываем событие
            StartCoroutine(CallJumpEvent());
        }
        private IEnumerator CallJumpEvent()
        {
            yield return new WaitForFixedUpdate();
            Jump?.Invoke();
        }
        private void OnJumpEnded()
        {
            _isJumping = false;
        }
        //При приземлении
        private void OnLanding()
        {
            //Изменяем максимальное ускорение и множитель ускорения 
            _currentMaxAcceleration = GROUND_MAX_ACCELERATION;
            
            //Сбрасываем текущую скорость, если кнопки передвижения не нажаты (0 * _currentSpeed = 0)
            //или нажаты в противоположную от скорости сторону (<0)
            if(_horizontalAxis * _currentSpeed <= 0){
                _currentSpeed = 0;
            }else{
                //Убавляем скорость при падении свысока
                if(_isLongFall && _currentSpeedAbs > _maxSpeedAfterLongFall){
                    _currentSpeed = _currentSpeed > 0 ? _maxSpeedAfterLongFall : -_maxSpeedAfterLongFall;
                    _currentAcceleration = 0;
                }
            }

            //Сбрасываем время прыжка
            _jumpTime = 0;
            //Сбрасываем предыдущую вертикальную скорость
            _previousVerticalVelocity = 0;
            //Вызываем событие при приземлении
            Landed?.Invoke(_rigidBody.velocity.magnitude);
        }
        //Отрываемся от земли
        private void OnTakingOff()
        {
            TookOff?.Invoke();
        }
        #endregion
    }
}

