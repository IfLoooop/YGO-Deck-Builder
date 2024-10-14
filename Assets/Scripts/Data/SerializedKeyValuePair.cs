#nullable enable

using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YGODeckBuilder.Data
{
    /// <summary>
    /// A key value pair struct that can be serialized in the inspector.
    /// </summary>
    /// <typeparam name="K"><see cref="Type"/> for <see cref="key"/>.</typeparam>
    /// <typeparam name="V"><see cref="Type"/> for <see cref="value"/>.</typeparam>
    [Serializable]
    internal struct SerializedKeyValuePair<K,V>
    {
        #region Inspector Fields
        [Tooltip("Key")]
        [HorizontalGroup, HideLabel]
        [SerializeField] private K key;
        [Tooltip("Value")]
        [HorizontalGroup, HideLabel]
        [SerializeField] private V value;
        #endregion
        
        #region Constants
        /// <summary>
        /// Refactor resistant name for <see cref="key"/>. <br/>
        /// <i>For custom <see cref="PropertyDrawer"/>.</i>
        /// </summary>
        internal const string KEY = nameof(key);
        /// <summary>
        /// Refactor resistant name for <see cref="value"/>. <br/>
        /// <i>For custom <see cref="PropertyDrawer"/>.</i>
        /// </summary>
        internal const string VALUE = nameof(value);
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="key"/>.
        /// </summary>
        internal K Key { get => this.key; set => this.key = value; }
        /// <summary>
        /// <see cref="value"/>.
        /// </summary>
        internal V Value { get => this.value; set => this.value = value; }
        #endregion
        
        #region Constructors
        /// <summary>
        /// <see cref="SerializedKeyValuePair{K,V}"/>.
        /// </summary>
        /// <param name="_Key"><see cref="key"/>.</param>
        /// <param name="_Value"><see cref="value"/>.</param>
        internal SerializedKeyValuePair(K _Key, V _Value)
        {
            this.key = _Key;
            this.value = _Value;
        }
        #endregion
    }
}