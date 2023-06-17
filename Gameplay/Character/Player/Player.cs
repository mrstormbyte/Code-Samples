using System;
using Project.Input;
using UnityEngine;

namespace Project.Gameplay
{
    /// <summary>
    /// Свойства игрока и взаимодействия с интерактивными объектами
    /// </summary>
    [SelectionBase]
    public class Player : MonoBehaviour, ICharacter
    {
        #region ПОЛЯ
        [Header("Схема управления"), SerializeField]
        private PlayerControlScheme _playerControlScheme = null;
        
        //Интерактивный объект касаемый на данный момент 
        private BaseInteractable _touchedInteractable;
        #endregion


        #region СВОЙСТВА
        /// <summary>
        /// Жив ли игрок
        /// </summary>
        public bool isAlive { get; private set; } = true;

        /// <summary>
        /// Касается ли игрок интерактивного объекта
        /// </summary>
        public bool isTouchingInteractable => _touchedInteractable != null;
        #endregion


        #region СОБЫТИЯ
        /// <summary>
        /// Событие смерти игрока
        /// </summary>
        public event Action Died;
        
        /// <summary>
        /// Событие прикосновения к интерактивному объекту
        /// </summary>
        public event Action InteractableTouchingStarted;
        /// <summary>
        /// Событие окончания прикосновения к интерактивному объекту
        /// </summary>
        public event Action InteractableTouchingEnded;
        #endregion


        #region MONOBEHAVIOUR
        private void OnEnable()
        {
            //Управление игроком            
            _playerControlScheme.Interacting += OnInteract;
            //Прикосновение к интерактивным объектам
            BaseInteractable.PlayerTouchingStarted += OnInteractableTouchingStarted;
            BaseInteractable.PlayerTouchingEnded += OnInteractableTouchingEnded;
        }
        private void OnDisable()
        {
            //Управление игроком
            _playerControlScheme.Interacting -= OnInteract;
            //Прикосновение к интерактивным объектам
            BaseInteractable.PlayerTouchingStarted -= OnInteractableTouchingStarted;
            BaseInteractable.PlayerTouchingEnded -= OnInteractableTouchingEnded;
        }
        #endregion


        #region ПРИВАТНЫЕ МЕТОДЫ И ФУНКЦИИ
        //Взаимодействие с интерактивным объектом
        private void OnInteract()
        {
            if(_touchedInteractable == null)
                return;

            _touchedInteractable.Interact();
        }
        //Касаемся интерактивного объекта 
        private void OnInteractableTouchingStarted(BaseInteractable interactable)
        {
            _touchedInteractable = interactable;
            InteractableTouchingStarted?.Invoke();
        }
        //Перестаём касаться интерактивного объекта
        private void OnInteractableTouchingEnded(BaseInteractable interactable)
        {
            _touchedInteractable = null;
            InteractableTouchingEnded?.Invoke();
        }
        #endregion
    }
}
