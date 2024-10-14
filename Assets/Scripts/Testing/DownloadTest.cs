#nullable enable

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using YGODeckBuilder.Data;
using YGODeckBuilder.Extensions;
using YGODeckBuilder.Utility;
using YGODeckBuilder.Web;
using YGODeckBuilder.Web.Yugipedia;

namespace YGODeckBuilder.Testing
{
    /// <summary>
    /// Contains various download tests.
    /// </summary>
    internal sealed class DownloadTest : DownloadFixture
    {
        #region Inspector Fields
        [Tooltip("Each URL contains all cards for a specific medium.")]
        [SerializeField] private ReadonlySelectableEntries<string> cards = new();
        #endregion
        
        #region Properties
        /// <summary>
        /// Will be <c>true</c> while a <see cref="DownloadFixture.Benchmark"/> is running.
        /// </summary>
        internal bool DownloadInProgress { get; set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            if (!Application.isEditor)
            {
                Destroy(this);
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            this.SetCardURLs();
        }
        
        /// <summary>
        /// Downloads all card titles from the URLs selected in <see cref="cards"/>.
        /// </summary>
        /// <param name="_Pages">Number of max additional pages to request the data from, if not all cards could be returned with one request.</param>
        [DisableIf(nameof(DownloadInProgress))]
        [Button("Download Cards", ButtonStyle.FoldoutButton), Tooltip("Downloads all card titles from the URLs selected in \"cards\".")]
        private void DownloadCardsAsync(int _Pages = int.MaxValue - 1)
        {
            base.Benchmark(this, async () =>
            {
                var _cardTitles = new List<string>(); // TODO: Store somewhere.
                
                foreach (var _url in this.cards.GetSelected(true))
                {
                    var _endpoint = _url.Replace(WebClient.BASE_ADDRESS, string.Empty);
                    
                    if (await YugipediaAPI.GetAllPagesAsync(_endpoint, base.CancellationTokenSource!.Token, _Pages) is not {} _categoryMembers)
                    {
                        continue;
                    }
                    
                    foreach (var _categoryMember in _categoryMembers.Where(_CategoryMember => !_cardTitles.Contains(_CategoryMember.Title)))
                    {
                        _cardTitles.Add(_categoryMember.Title);
                    }
                }
            });
        }

        /// <summary>
        /// Downloads the card data for the given <c>Endpoint</c>.
        /// </summary>
        /// <param name="_Endpoint">The endpoint of the card to download.</param>
        [DisableIf(nameof(DownloadInProgress))]
        [Button("Download Card Data", ButtonStyle.FoldoutButton), Tooltip("Downloads the card data for the given \"Endpoint\".")]
        private void DownloadCardDataAsync(string _Endpoint)
        {
            base.Benchmark(this, async () =>
            {
                if (string.IsNullOrWhiteSpace(_Endpoint))
                {
                    Debug.LogError($"_Endpoint is empty: {_Endpoint}");
                    return;
                }
                
                var _endpoint = _Endpoint.Replace(WebClient.BASE_ADDRESS, string.Empty);
                
                var _sourceText = await YugipediaAPI.GetCardData(_endpoint, base.CancellationTokenSource!.Token); // TODO: Store somewhere.
            });
        }
        
        /// <summary>
        /// Downloads the set data for the given <c>Endpoint</c>.
        /// </summary>
        /// <param name="_Endpoint">The endpoint of the set to download.</param>
        [DisableIf(nameof(DownloadInProgress))]
        [Button("Download Set Data", ButtonStyle.FoldoutButton), Tooltip("Downloads the set data for the given \"Endpoint\".")]
        private void DownloadSetDataAsync(string _Endpoint)
        {
            base.Benchmark(this, async () =>
            {
                if (string.IsNullOrWhiteSpace(_Endpoint))
                {
                    Debug.LogError($"_Endpoint is empty: {_Endpoint}");
                    return;
                }
                
                var _endpoint = _Endpoint.Replace(WebClient.BASE_ADDRESS, string.Empty);
                
                var _htmlText = await YugipediaAPI.GetSetData(_endpoint, base.CancellationTokenSource!.Token); // TODO: Store somewhere.
            });
        }
        
        /// <summary>
        /// Aborts all running and queued downloads.
        /// </summary>
        [Button, Tooltip("Aborts all running and queued downloads.")]
        private void Abort()
        {
            Debug.Log("Aborted.".Color(RichTextColor.Orange));
            
            WebClient.Abort(base.CancellationTokenSource!);
        }

        /// <summary>
        /// Populates <see cref="cards"/> with all URLs.
        /// </summary>
        private void SetCardURLs()
        {
            this.cards.Populate(new[]
            {
                "https://yugipedia.com/wiki/Category:OCG_cards",
                "https://yugipedia.com/wiki/Category:TCG_cards"
            });
        }
#endif
        #endregion
    }
}