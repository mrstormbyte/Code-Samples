using System;

namespace Project.Gameplay
{
    /// <summary>
    /// Живые персонажи
    /// </summary>
    public interface ICharacter
    {
        #region СВОЙСТВА
        /// <summary>
        /// Жив ли персонаж
        /// </summary>
        public bool isAlive {get;}
        #endregion


        #region СОБЫТИЯ
        /// <summary>
        /// Событие смерти персонажа
        /// </summary>
        public event Action Died;
        #endregion
    }
}
