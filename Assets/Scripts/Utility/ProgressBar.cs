#nullable enable

using System;
using System.Threading;
using YGODeckBuilder.Testing;

namespace YGODeckBuilder.Utility
{
    /// <summary>
    /// Provides functionality for managing and updating progress bars. <br/>
    /// <i>Uses the <see cref="DebugWindow"/> in editor.</i>
    /// </summary>
    internal sealed class ProgressBar
    {
        #region Fields
        /// <summary>
        /// Number that the <see cref="currentProgress"/> should reach to fill the progress bar.
        /// </summary>
        private long totalProgress;
        /// <summary>
        /// Counter for the current progress state.
        /// </summary>
        private long currentProgress;
        #endregion
        
        #region Properties
#if UNITY_EDITOR
        /// <summary>
        /// Unchangeable identifier (Not necessarily unique). <br/>
        /// <i>Only needed for the <see cref="DebugWindow"/> to remove old progress bars from the list.</i>
        /// </summary>
        internal string Id { get; }
#endif
        /// <summary>
        /// Descriptive title to display in the progress window.
        /// </summary>
        internal string Title { get; private set; }
        /// <summary>
        /// <see cref="totalProgress"/>.
        /// </summary>
        internal long TotalProgress => Interlocked.Read(ref this.totalProgress);
        /// <summary>
        /// <see cref="currentProgress"/>.
        /// </summary>
        internal long CurrentProgress => Interlocked.Read(ref this.currentProgress);
        /// <summary>
        /// Timestamp when this <see cref="ProgressBar"/> is created.
        /// </summary>
        internal DateTime StartTime { get; }
        /// <summary>
        /// Timestamp when the <see cref="CurrentProgress"/> was last updated.
        /// </summary>
        internal DateTime LastUpdate { get; set; }
        /// <summary>
        /// Progress of <see cref="currentProgress"/>/<see cref="totalProgress"/> in percent.
        /// </summary>
        internal float Progress => (float)this.CurrentProgress / this.TotalProgress;
        #endregion

        #region Constructors
        /// <summary>
        /// <see cref="ProgressBar"/>.
        /// </summary>
        /// <param name="_Title"><see cref="Title"/>.</param>
        /// <param name="_TotalProgress"><see cref="totalProgress"/>.</param>
        private ProgressBar(string _Title, long _TotalProgress)
        {
#if UNITY_EDITOR
            this.Id = _Title;
#endif
            this.Title = _Title;
            this.totalProgress = _TotalProgress;
            this.StartTime = DateTime.Now;
            this.LastUpdate = this.StartTime;
            
#if UNITY_EDITOR
            DebugWindow.Open(this);
#endif
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Creates a new <see cref="ProgressBar"/> and opens the <see cref="DebugWindow"/> in editor.
        /// </summary>
        /// <param name="_Title"><see cref="Title"/>.</param>
        /// <param name="_TotalProgress"><see cref="totalProgress"/>.</param>
        /// <returns>The created <see cref="ProgressBar"/>.</returns>
        internal static ProgressBar Create(string _Title, long _TotalProgress)
        {
            return new ProgressBar(_Title, _TotalProgress);
        }
        
        /// <summary>
        /// Increments <see cref="CurrentProgress"/> by one.
        /// </summary>
        internal void AddProgress()
        {
            Interlocked.Increment(ref this.currentProgress);
            this.LastUpdate = DateTime.Now;

            // Dynamically updates the total progress if the value that was set is too small.
            if (this.CurrentProgress > this.TotalProgress)
            {
                Interlocked.Exchange(ref this.totalProgress, this.CurrentProgress + 1);
            }
            
#if UNITY_EDITOR
            DebugWindow.UpdateWindow();
#endif   
        }
        
        /// <summary>
        /// Increments <see cref="totalProgress"/> by the given <c>_Amount</c>.
        /// </summary>
        /// <param name="_Amount">The amount to increment <see cref="totalProgress"/> by.</param>
        internal void AddTotal(int _Amount = 1)
        {
            Interlocked.Exchange(ref this.totalProgress, this.TotalProgress + _Amount);
            
#if UNITY_EDITOR
            DebugWindow.UpdateWindow();
#endif   
        }
        
        /// <summary>
        /// Stops the progress bar.
        /// </summary>
        internal void Stop()
        {
            this.Title = $"{this.Title} Stopped";
            this.totalProgress = this.currentProgress;
            
#if UNITY_EDITOR
            DebugWindow.UpdateWindow();
#endif
        }
        #endregion
    }
}