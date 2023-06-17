using System;

namespace Project.Gameplay
{
    /// <summary>
    /// Интерфейс для ходящих персонажен
    /// </summary>
    public interface IWalkable
    {
        #region СВОЙСТВА
        /// <summary>
        /// На земле ли персонаж
        /// </summary>
        public bool isGrounded {get;}
        /// <summary>
        /// Направление персонажа
        /// </summary>
        public Direction direction {get;}
        /// <summary>
        /// Передвигается ли персонаж
        /// </summary>
        public bool isMoving {get;}

        /// <summary>
        /// Вертикальная скорость персонажа
        /// </summary>
        public float verticalVelocity {get;}
        #endregion


        #region СОБЫТИЯ
        /// <summary>
        /// Персонаж оторвался от земли
        /// </summary>
        public event Action TookOff;
        /// <summary>
        /// Персонаж приземлился (скорость при приземлении)
        /// </summary>
        public event Action<float> Landed;
        /// <summary>
        /// Изменилось горизонтальное направление
        /// </summary>
        public event Action HorizontalDirectionChanged;
        /// <summary>
        /// Изменилось вертикальное направление
        /// </summary>
        public event Action VerticalDirectionChanged;
        #endregion
    }
}