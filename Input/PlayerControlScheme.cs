using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Project.Gameplay;

namespace Project.Input
{
    //Схема управления игроком, используя InputSystem
    [CreateAssetMenu(fileName = "PlayerControlScheme", menuName = "Input/Gameplay Control Schemes/PlayerControlScheme")]
    public class PlayerControlScheme : ControlScheme
    {
        #region СОБЫТИЯ
        /// <summary>
        /// Событие передвижения игрока
        /// </summary>
        public event Action<int> MoveStarted;
        /// <summary>
        /// Событие передвижения игрока
        /// </summary>
        public event Action MoveEnded;
        /// <summary>
        /// Событие начала прыжка игрока
        /// </summary>
        public event Action JumpStarted;
        /// <summary>
        /// Событие окончания прыжка игрока
        /// </summary>
        public event Action JumpEnded;
        /// <summary>
        /// Событие взаимодействия игрока с объектом
        /// </summary>
        public event Action Interacting;
        #endregion


        #region ЗАЩИЩЕННЫЕ МЕТОДЫ ДЛЯ ПЕРЕОПРЕДЕЛЕНИЯ
        //Подписаться на действия
        protected override void Subscribe()
        {
            this.inputActions.GameplayPlayer.Move.performed += OnMoveStarted;
            this.inputActions.GameplayPlayer.Move.canceled += OnMoveEnded;
            this.inputActions.GameplayPlayer.Jump.started += OnJumpStarted;
            this.inputActions.GameplayPlayer.Jump.canceled += OnJumpEnded;
            this.inputActions.GameplayPlayer.Interact.started += OnInteract;
            this.inputActions.GameplayPlayer.Rotate.started += OnRotate;
            this.inputActions.GameplayPlayer.RotateByStick.performed += OnRotateByStick;
            this.inputActions.GameplayPlayer.RotateByStick.canceled += OnEndRotateByStick;
            this.inputActions.GameplayPlayer.ZoomCamera.performed += OnZoomCamera;
        }
        //Отписаться от действий
        protected override void Unsubscribe()
        {
            this.inputActions.GameplayPlayer.Move.performed -= OnMoveStarted;
            this.inputActions.GameplayPlayer.Move.canceled -= OnMoveEnded;
            this.inputActions.GameplayPlayer.Jump.started -= OnJumpStarted;
            this.inputActions.GameplayPlayer.Jump.canceled -= OnJumpEnded;
            this.inputActions.GameplayPlayer.Interact.started -= OnInteract;
            this.inputActions.GameplayPlayer.Rotate.started -= OnRotate;
            this.inputActions.GameplayPlayer.RotateByStick.performed -= OnRotateByStick;
            this.inputActions.GameplayPlayer.RotateByStick.canceled -= OnEndRotateByStick;
            this.inputActions.GameplayPlayer.ZoomCamera.performed -= OnZoomCamera;
        }
        #endregion


        #region ОБРАБОТКА СОБЫТИЙ
        //Передвижения игрока 
        private void OnMoveStarted(InputAction.CallbackContext context)
        {
            //Результат
            int result = 1;
            //Значение ввода
            float value = context.ReadValue<float>();

            //Округляем результат, если ввод со стика геймпада
            if(value > 0.5f) {
                result = 1;
            }else if(value < -0.5f) {
                result = -1;
            }else{
                MoveEnd();
                return;
            }
            
            //Вызываем событие
            MoveStarted?.Invoke(result);
        }

        //Передвижения игрока окончено 
        private void OnMoveEnded(InputAction.CallbackContext context)
        {
            MoveEnded?.Invoke();
        }

        //Прыжок
        public void OnJumpStarted(InputAction.CallbackContext context)
        {
            JumpStart();
        }
        public void OnJumpEnded(InputAction.CallbackContext context)
        {
            JumpEnd();
        }

        //Использовать
        public void OnInteract(InputAction.CallbackContext context)
        {
            Interact();
        }
        #endregion
    }
}