#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YGODeckBuilder.Data
{
    /// <summary>
    /// A <see cref="List{T}"/> of <see cref="ReadonlySelectableEntry{V}"/>.
    /// </summary>
    /// <typeparam name="V"><see cref="Type"/> of <see cref="SerializedKeyValuePair{K,V}.value"/>.</typeparam>
    [Serializable]
    internal sealed class ReadonlySelectableEntries<V>
    {
        #region Inspector Fields
        [Tooltip("Selectable entries with Readonly values in the inspector.")]
        [PropertyOrder(2)][ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [SerializeField] private List<ReadonlySelectableEntry<V>> entries;
        #endregion
        
        #region Constructors
        /// <summary>
        /// <see cref="ReadonlySelectableEntries{V}"/>.
        /// </summary>
        public ReadonlySelectableEntries()
        {
            this.entries = new List<ReadonlySelectableEntry<V>>();
        }
        
        /// <summary>
        /// <see cref="ReadonlySelectableEntries{V}"/>.
        /// </summary>
        /// <param name="_Entries"><see cref="entries"/>.</param>
        internal ReadonlySelectableEntries(IEnumerable<ReadonlySelectableEntry<V>> _Entries)
        {
            this.entries = new List<ReadonlySelectableEntry<V>>(_Entries);
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets every <see cref="SerializedKeyValuePair{K,V}.key"/> in <see cref="entries"/> to <c>true</c>.
        /// </summary>
        [ButtonGroup, PropertyOrder(1)]
        private void SelectAll()
        {
            this.SetKeys(true);
        }

        /// <summary>
        /// Sets every <see cref="SerializedKeyValuePair{K,V}.key"/> in <see cref="entries"/> to <c>false</c>.
        /// </summary>
        [ButtonGroup, PropertyOrder(1)]
        private void DeselectAll()
        {
            this.SetKeys(false);
        }
        
        /// <summary>
        /// Sets every <see cref="SerializedKeyValuePair{K,V}.key"/> in <see cref="entries"/> to the value of the given <c>_Key</c>.
        /// </summary>
        /// <param name="_Key">The value to set every <see cref="SerializedKeyValuePair{K,V}.key"/>  in <see cref="entries"/> to.</param>
        private void SetKeys(bool _Key)
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.entries.Count; i++)
            {
                this.entries[i] = new ReadonlySelectableEntry<V>(_Key, this.entries[i].Entry.Value);
            }
        }

        /// <summary>
        /// Clears <see cref="entries"/> and populates it with the given <c>_Values</c>. <br/>
        /// <i>If any of the given <c>_Values</c> already exist in <see cref="entries"/>, the <see cref="SerializedKeyValuePair{K,V}.key"/> will be set to the existing value.</i>
        /// </summary>
        /// <param name="_Values">The new values for <see cref="entries"/>.</param>
        internal void Populate(IEnumerable<V> _Values)
        {
            var _selected = this.GetSelected().ToArray();
            
            this.entries.Clear();
            
            foreach (var _value in _Values)
            {
                this.entries.Add(_selected.Contains(_value)
                    ? new ReadonlySelectableEntry<V>(true, _value)
                    : new ReadonlySelectableEntry<V>(false, _value));
            }
        }
        
        /// <summary>
        /// Returns the <see cref="SerializedKeyValuePair{K,V}.value"/> of every <see cref="ReadonlySelectableEntry{V}"/> in <see cref="entries"/>, where the <see cref="SerializedKeyValuePair{K,V}.key"/> is <c>true</c>.
        /// </summary>
        /// <param name="_PrintWarning">
        /// Prints a warning to the consolse if no <see cref="entries"/> are selected <br/>
        /// <b>Editor only.</b>
        /// .</param>
        /// <returns>The <see cref="SerializedKeyValuePair{K,V}.value"/> of every <see cref="ReadonlySelectableEntry{V}"/> in <see cref="entries"/>, where the <see cref="SerializedKeyValuePair{K,V}.key"/> is <c>true</c>.</returns>
        internal V[] GetSelected(bool _PrintWarning = false)
        {
            var _selected = this.entries.Where(_ReadonlySelectableEntry => _ReadonlySelectableEntry.Entry.Key).Select(_ReadonlySelectableEntry => _ReadonlySelectableEntry.Entry.Value).ToArray();

#if UNITY_EDITOR
            if (_PrintWarning && _selected.Length == 0)
            {
                Debug.LogWarning("No entries selected.");
            }
#endif
            return _selected;
        }
        #endregion
    }
}