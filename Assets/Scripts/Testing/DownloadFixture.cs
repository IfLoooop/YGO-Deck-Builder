#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YGODeckBuilder.Extensions;
using YGODeckBuilder.Utility;
using Debug = UnityEngine.Debug;

namespace YGODeckBuilder.Testing
{
    /// <summary>
    /// Provides all needed dependencies for <see cref="DownloadTest"/>.
    /// </summary>
    internal abstract class DownloadFixture : MonoBehaviour
    {
#if UNITY_EDITOR
        #region Properties
        /// <summary>
        /// For cancelling the web requests.
        /// </summary>
        protected CancellationTokenSource? CancellationTokenSource { get; private set; }
        #endregion
        
        #region Methods
        /// <summary>
        /// Measures the time it takes to complete the given <c>Action</c>.
        /// </summary>
        /// <param name="_DownloadTest">The <see cref="DownloadTest"/> object where this method was started.</param>
        /// <param name="_Action">The action to measure the time of.</param>
        /// <param name="_CallerMember">
        /// Leave empty. <br/>
        /// <i>Will be the member name where this method is called from.</i>
        /// </param>
        protected async void Benchmark(DownloadTest _DownloadTest, Func<Task> _Action, [CallerMemberName] string _CallerMember = "")
        {
            _DownloadTest.DownloadInProgress = true;
            
            Debug.Log($"Test [{_CallerMember}] started.".Bold());

            this.CancellationTokenSource = new CancellationTokenSource();
            
            var _stopwatch = new Stopwatch();

            _stopwatch.Start();
            
            await _Action();
            
            _stopwatch.Stop();
            
            Debug.Log(_stopwatch.Elapsed.Color(RichTextColor.Yellow));
            Debug.Log($"Test [{_CallerMember}] stopped.".Bold());
            
            _DownloadTest.DownloadInProgress = false;
        }
        #endregion
#endif
    }
}