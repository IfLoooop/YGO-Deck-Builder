#nullable enable

using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YGODeckBuilder.Data
{
    /// <summary>
    /// A <see cref="SerializedKeyValuePair{K,V}"/> with a <see cref="bool"/> <see cref="SerializedKeyValuePair{K,V}.key"/> and a readonly <see cref="SerializedKeyValuePair{K,V}.value"/> in the inspector.
    /// </summary>
    /// <typeparam name="V"><see cref="Type"/> of <see cref="SerializedKeyValuePair{K,V}.value"/>.</typeparam>
    [Serializable]
    internal struct ReadonlySelectableEntry<V>
    {
        #region Inspector Fields
        [Tooltip("Selectable entry with a Readonly value in the inspector.")]
        [HideLabel]
        [SerializeField] private SerializedKeyValuePair<bool, V> entry;
        #endregion
        
        #region Constants
        /// <summary>
        /// Refactor resistant name for <see cref="Entry"/>. <br/>
        /// <i>For custom <see cref="PropertyDrawer"/>.</i>
        /// </summary>
        internal const string ENTRY = nameof(entry);
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="entry"/>.
        /// </summary>
        internal SerializedKeyValuePair<bool, V> Entry { get => this.entry; set => this.entry = value; }
        #endregion
        
        #region Constructors
        /// <summary>
        /// <see cref="ReadonlySelectableEntry{V}"/>.
        /// </summary>
        /// <param name="_Key"><see cref="SerializedKeyValuePair{K,V}.key"/>.</param>
        /// <param name="_Value"><see cref="SerializedKeyValuePair{K,V}.value"/>.</param>
        internal ReadonlySelectableEntry(bool _Key, V _Value)
        {
            this.entry = new SerializedKeyValuePair<bool, V>(_Key, _Value);
        }
        #endregion
    }
}