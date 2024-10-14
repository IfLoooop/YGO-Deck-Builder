// ReSharper disable RedundantUsingDirective
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using YGODeckBuilder.Extensions;
using YGODeckBuilder.Settings;
using YGODeckBuilder.Web;
using YGODeckBuilder.Web.Yugipedia;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace YGODeckBuilder.Testing
{
    /// <summary>
    /// Provides an inspector button for quick testing.
    /// </summary>
    internal sealed class Test : MonoBehaviour
    {
        #region Methods
        private void Awake()
        {
            if (!Application.isEditor)
            {
                Destroy(this);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector button for quick testing.
        /// </summary>
        [Button("Test")]
        private async void TestButton()
        {
            Debug.Log("Test started".Bold());
            
            await this.TestMethod();
            
            Debug.Log("Test finished".Bold());
        }

        /// <summary>
        /// Contains the logic for <see cref="TestButton"/>.
        /// </summary>
        private async Task TestMethod()
        {
            Debug.Log(await YugipediaAPI.GetPageCountAsync("Category:OCG_cards", new CancellationToken()));
        }
#endif
        #endregion
    }
}