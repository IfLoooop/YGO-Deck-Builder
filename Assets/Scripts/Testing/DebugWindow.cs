#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YGODeckBuilder.Extensions;
using YGODeckBuilder.Utility;

namespace YGODeckBuilder.Testing
{
#if UNITY_EDITOR
    /// <summary>
    /// <see cref="EditorWindow"/> to display debug information.
    /// </summary>
    internal sealed class DebugWindow : EditorWindow
    {
        #region Constants
        /// <summary>
        /// Display name for this <see cref="DebugWindow"/>.
        /// </summary>
        private const string WINDOW_NAME = "Debug Window";
        /// <summary>
        /// Space between elements and the border.
        /// </summary>
        private const float PADDING = 2.5f;
        /// <summary>
        /// Element height.
        /// </summary>
        private const float HEIGHT = 20f;
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="GUIStyle"/> for titles.
        /// </summary>
        private static GUIStyle titleStyle = null!;
        /// <summary>
        /// <see cref="GUIStyle"/> for labels.
        /// </summary>
        private static GUIStyle labelStyle = null!;
        
        /// <summary>
        /// Singleton of <see cref="DebugWindow"/>.
        /// </summary>
        private static DebugWindow? window;
        /// <summary>
        /// Stores all repaint requests.
        /// </summary>
        private static readonly ConcurrentQueue<Action> repaintQueue = new();
        /// <summary>
        /// Contains every <see cref="ProgressBar"/> that is currently running.
        /// </summary>
        private static readonly List<ProgressBar> progressBars = new();
        #endregion
        
        #region Methods
        /// <summary>
        /// Creates and opens this <see cref="DebugWindow"/>.
        /// </summary>
        [MenuItem("Window/Debug Window")]
        private static void Open()
        {
            window ??= GetWindow<DebugWindow>(WINDOW_NAME);
        }
        
        private void OnGUI()
        {
            SetStyles();

            var _currentRow = 0;
            EditorGUI.LabelField(new Rect(PADDING, HEIGHT * _currentRow++, base.position.width - PADDING * 2, HEIGHT), "<b>Download Progress</b>", titleStyle);
            foreach (var _progressBar in progressBars)
            {
                if (_progressBar.TotalProgress > 0 && _progressBar.CurrentProgress > 0)
                {
                    EditorGUI.LabelField(new Rect(PADDING, HEIGHT * _currentRow++, base.position.width - PADDING * 2, HEIGHT), $"<i>{_progressBar.Title}</i>", labelStyle);
                    EditorGUI.LabelField(new Rect(PADDING, HEIGHT * _currentRow++, base.position.width - PADDING * 2, HEIGHT), $"{_progressBar.CurrentProgress.ToString(new string('0', _progressBar.TotalProgress.ToString().Length))}/{_progressBar.TotalProgress}", labelStyle);
                    EditorGUI.ProgressBar(new Rect(PADDING, HEIGHT * _currentRow++, base.position.width - PADDING * 2, HEIGHT), _progressBar.Progress, $"{_progressBar.Progress * 100f:0.0}%");
                    EditorGUI.LabelField(new Rect(PADDING, HEIGHT * _currentRow++, base.position.width - PADDING * 2, HEIGHT), $@"Estimated time: {_progressBar.StartTime.EstimateTime(_progressBar.CurrentProgress, _progressBar.TotalProgress, _progressBar.LastUpdate):hh\:mm\:ss}", labelStyle);
                
                    // Adds an empty line between each iteration.
                    _currentRow++;   
                }
            }
        }

        /// <summary>
        /// Sets all <see cref="GUIStyle"/>.
        /// </summary>
        private static void SetStyles()
        {
            var _guiStyleState = new GUIStyleState { textColor = Color.white };
            var _rectOffset = new RectOffset((int)PADDING, 0, 0, 0);
            
            labelStyle = new GUIStyle
            {
                richText = true, 
                normal = _guiStyleState,
                alignment = TextAnchor.MiddleLeft,
                padding = _rectOffset
            };
            titleStyle = new GUIStyle
            {
                richText = true, 
                normal = _guiStyleState,
                alignment = TextAnchor.MiddleCenter,
                padding = _rectOffset
            };
        }

        private void OnEnable()
        {
            EditorApplication.update += ProcessQueue;
            EditorApplication.update += SetLastUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= ProcessQueue;
            EditorApplication.update -= SetLastUpdate;
        }

        /// <summary>
        /// Dequeues <see cref="repaintQueue"/> and invokes all actions.
        /// </summary>
        private static void ProcessQueue()
        {
            while (repaintQueue.TryDequeue(out var _action))
            {
                _action();
            }
        }
        
        /// <summary>
        /// Updates the progress bar every second. <br/>
        /// <i>This lets the estimated remaining time increase when the last <see cref="UpdateWindow"/> call took longer than one second.</i>
        /// </summary>
        private static void SetLastUpdate()
        {
            foreach (var _progressBar in progressBars)
            {
                if (DateTime.Now - _progressBar.LastUpdate >= TimeSpan.FromSeconds(1))
                {
                    _progressBar.LastUpdate = DateTime.Now;
                    window?.Repaint();
                }   
            }
        }
        
        /// <summary>
        /// Adds the given <c>_ProgressBar</c> to <see cref="progressBars"/> and opens the <see cref="DebugWindow"/>.
        /// </summary>
        /// <param name="_ProgressBar">The <see cref="ProgressBar"/> to add to <see cref="progressBars"/>.</param>
        internal static void Open(ProgressBar _ProgressBar)
        {
            if (progressBars.FindIndex(_AddedProgressBar => _AddedProgressBar.Id == _ProgressBar.Id) is var _index and > -1)
            {
                progressBars.RemoveAt(_index);
            }
            
            progressBars.Add(_ProgressBar);
            window ??= GetWindow<DebugWindow>(WINDOW_NAME);
        }
        
        /// <summary>
        /// Force repaints the <see cref="DebugWindow"/>.
        /// </summary>
        internal static void UpdateWindow()
        {
            repaintQueue.Enqueue(() => window?.Repaint());
        }
        #endregion
    }
#endif
}