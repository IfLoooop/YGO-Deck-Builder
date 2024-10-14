#nullable enable

using UnityEditor;
using UnityEngine;
using YGODeckBuilder.Data;

namespace YGODeckBuilder.Editor.PropertyDrawers
{
    /// <summary>
    /// <see cref="PropertyDrawer"/> for <see cref="ReadonlySelectableEntry{V}"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlySelectableEntry<>))]
    internal sealed class ReadonlySelectableEntryDrawer : PropertyDrawer
    {
        #region Methods
        public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            var _entryProperty = _Property.FindPropertyRelative(ReadonlySelectableEntry<object>.ENTRY);
            var _keyProperty = _entryProperty.FindPropertyRelative(SerializedKeyValuePair<object, object>.KEY);
            var _valueProperty = _entryProperty.FindPropertyRelative(SerializedKeyValuePair<object, object>.VALUE);
            
            var _keyRect = new Rect(_Position.x, _Position.y, _Position.width * 0.5f, _Position.height);
            var _valueRect = new Rect(_Position.x + _Position.width * 0.5f, _Position.y, _Position.width * 0.5f, _Position.height);
            
            EditorGUI.PropertyField(_keyRect, _keyProperty, GUIContent.none);
            
            GUI.enabled = false;
            EditorGUI.PropertyField(_valueRect, _valueProperty, GUIContent.none);
            GUI.enabled = true;
        }
        #endregion
    }
}