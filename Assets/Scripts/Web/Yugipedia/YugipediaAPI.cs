#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YGODeckBuilder.Extensions;
using YGODeckBuilder.Settings;
using YGODeckBuilder.Utility;

namespace YGODeckBuilder.Web.Yugipedia
{
    /// <summary>
    /// For sending request to the yugipedia API. <br/>
    /// https://yugipedia.com/wiki/Yugipedia:API <br/>
    /// https://www.mediawiki.org/wiki/API:Etiquette <br/>
    /// https://www.mediawiki.org/wiki/API:Query <br/>
    /// https://www.mediawiki.org/wiki/API:Parse <br/>
    /// https://www.mediawiki.org/wiki/API:Errors_and_warnings
    /// </summary>
    internal static class YugipediaAPI
    {
        #region Constants
        /// <summary>
        /// Every API call must start with this.
        /// </summary>
        private const string API = "api.php";
        /// <summary>
        /// Executes a query action to retrieve the data for all category members from the API.
        /// </summary>
        private const string QUERY = "?action=query";
        /// <summary>
        /// Query parameter to retrieve information about a category.
        /// </summary>
        private const string CATEGORY_INFO = "&prop=categoryinfo";
        /// <summary>
        /// Query parameter used to retrieve a list of category members.
        /// </summary>
        private const string LIST = "&list=" + CATEGORY_MEMBERS + "&cmlimit=max";
        /// <summary>
        /// Parameter to retrieve all members/pages of a specified category from the API.
        /// </summary>
        private const string CATEGORY_MEMBERS = "categorymembers";
        /// <summary>
        /// Executes a parse action to process and extract structured data.
        /// </summary>
        private const string PARSE = "?action=parse";
        /// <summary>
        /// Returns the source text of a wiki page. <br/>
        /// <b>Can only be used with <see cref="PARSE"/></b>
        /// </summary>
        private const string WIKI_TEXT = "&prop=wikitext";
        /// <summary>
        /// Returns the rendered HTML text of a wiki page. <br/>
        /// <b>Can only be used with <see cref="PARSE"/></b>
        /// </summary>
        private const string TEXT = "&prop=text";
        /// <summary>
        /// Specifies that the format in which the data will be returned should be JSON.
        /// </summary>
        private const string JSON_FORMAT = "&format=json";
        /// <summary>
        /// Specifies to use the format version 2.
        /// </summary>
        private const string FORMAT_VERSION_2 = "&formatversion=2";
        
        /// <summary>
        /// Error code/key when the request has reached API's rate limit.
        /// </summary>
        private const string RATE_LIMITED_ERROR = "ratelimited";
        #endregion
        
        #region Methods
        /// <summary>
        /// Gets the page count under the given <c>_Endpoint</c>.
        /// </summary>
        /// <param name="_Endpoint"><b>Only endpoints that start with <c>Category:</c> are allowed.</b></param>
        /// <param name="_CancellationToken">Cancels all queued requests.</param>
        /// <returns>The number of pages under the given <c>_Endpoint</c>, or <c>-1</c> if an error occured.</returns>
        internal static async Task<int> GetPageCountAsync(string _Endpoint, CancellationToken _CancellationToken)
        {
            if (!CheckIfCategory(_Endpoint))
            {
                return -1;
            }
            
            //api.php?action=query&prop=categoryinfo&titles=Category:OCG_cards&format=json
            var _pages = -1;
            var _endpoint = API + QUERY + CATEGORY_INFO + $"&titles={_Endpoint}" + JSON_FORMAT + FORMAT_VERSION_2;
            
            await WebClient.QueueWebRequestAsync(_endpoint, async _Response =>
            {
                if (await ParseResponseAsync<CategoryInfoResponse.Root>(_Endpoint, _Response) is {} _parseResponse)
                {
                    if (CheckResponse(_parseResponse) is {} _message)
                    {
                        return _message;
                    }
                    
                    if (_parseResponse.Query?.Pages.FirstOrDefault()?.CategoryInfo is {} _categoryInfo)
                    {
                        _pages = _categoryInfo.Pages;
                    }
                    else
                    {
                        Debug.LogError($"The response content was null. {(WebClient.BASE_ADDRESS + _Endpoint).ToHyperlink()}");
                    }
                }

                return null;
                
            }, _CancellationToken);
            
            return _pages;
        }
        
        /// <summary>
        /// Retrieves all pages under the given <c>_Endpoint</c> from the API.
        /// </summary>
        /// <param name="_Endpoint"><b>Only endpoints that start with <c>Category:</c> are allowed.</b></param>
        /// <param name="_Pages">Number of max additional pages to request the data from, if not all category members could be returned with one request.</param>
        /// <param name="_CancellationToken">Cancels all queued requests.</param>
        /// <returns>A <see cref="List{T}"/> containing every retrieved <see cref="QueryResponse.CategoryMember"/>, or <c>null</c> if an error occurred.</returns>
        internal static async Task<List<QueryResponse.CategoryMember>?> GetAllPagesAsync(string _Endpoint, CancellationToken _CancellationToken, int _Pages = int.MaxValue - 1) // -1 for the total progress calculation.
        {
            if (!CheckIfCategory(_Endpoint))
            {
                return null;
            }
            
            var _pageCount = await GetPageCountAsync(_Endpoint, _CancellationToken);
            // +1 for the first request. ("_Pages" = All additional requests after the first one.)
            // 500 is the max number of pages that can be retrieved in one request.
            var _totalProgress = Math.Min(_Pages + 1, Mathf.CeilToInt(_pageCount / 500f));
            var _progressBar = ProgressBar.Create("Get All Pages", _totalProgress);
            
            var _categoryMembers = new List<QueryResponse.CategoryMember>();
            var _endpoint = API + QUERY + LIST + $"&cmtitle={_Endpoint}" + JSON_FORMAT + FORMAT_VERSION_2;
            var _continue = string.Empty;

            do
            {
                var _status = await WebClient.QueueWebRequestAsync(_endpoint + _continue, async _Response =>
                {
                    _continue = string.Empty;
                    
                    if (await ParseResponseAsync<QueryResponse.Root>(_endpoint + _continue, _Response) is {} _queryResponse)
                    {
                        if (CheckResponse(_queryResponse) is {} _message)
                        {
                            return _message;
                        }
                    
                        // ReSharper disable once InconsistentNaming
                        if (_queryResponse.Query.TryGetValue(CATEGORY_MEMBERS, out var _CategoryMembers))
                        {
                            _categoryMembers.AddRange(_CategoryMembers);
                        }
                        else
                        {
                            return $"Could not retrieve the [{CATEGORY_MEMBERS}] from the response.";
                        }

                        if (_Pages-- > 0)
                        {
                            // ReSharper disable once InconsistentNaming
                            if (_queryResponse.Continue is {} _Continue)
                            {
                                _continue = $"&cmcontinue={_Continue.CmContinue}";
                            }   
                        }
                    }

                    return null;
                    
                }, _CancellationToken);
                
                _progressBar.AddProgress();
                
                if (_status == TaskStatus.Canceled)
                {
                    break;
                }

            } while (_continue != string.Empty);

            _progressBar.Stop();
            return _categoryMembers;
        }

        /// <summary>
        /// Retrieves the data for the given <c>_Endpoint</c> from the API.
        /// </summary>
        /// <param name="_Endpoint">Should be the endpoint of a card.</param>
        /// <param name="_CancellationToken">Cancels all queued requests.</param>
        /// <returns>The source text for the given <c>_Endpoint</c>, or <c>null</c> if an error occurred.</returns>
        internal static async Task<string?> GetCardData(string _Endpoint, CancellationToken _CancellationToken)
        {
            string? _content = null;
            var _endpoint = API + PARSE + WIKI_TEXT + $"&page={_Endpoint}" + JSON_FORMAT + FORMAT_VERSION_2;
            
            await WebClient.QueueWebRequestAsync(_endpoint, async _Response =>
            {
                if (await ParseResponseAsync<ParseResponse.Root>(_Endpoint, _Response) is {} _parseResponse)
                {
                    if (CheckResponse(_parseResponse) is {} _message)
                    {
                        return _message;
                    }
                    
                    if (_parseResponse.Json?.WikiText is {} _wikiText)
                    {
                        _content = _wikiText;
                    }
                    else
                    {
                        Debug.LogError($"The response content was null. {(WebClient.BASE_ADDRESS + _Endpoint).ToHyperlink()}");
                    }
                }

                return null;
                
            }, _CancellationToken);

            return _content;
        }
        
        /// <summary>
        /// Retrieves the data for the given <c>_Endpoint</c> from the API.
        /// </summary>
        /// <param name="_Endpoint">Should be the endpoint of a set.</param>
        /// <param name="_CancellationToken">Cancels all queued requests.</param>
        /// <returns>The rendered HTML text for the given <c>_Endpoint</c>, or <c>null</c> if an error occurred.</returns>
        internal static async Task<string?> GetSetData(string _Endpoint, CancellationToken _CancellationToken)
        {
            string? _content = null;
            var _endpoint = API + PARSE + TEXT + $"&page={_Endpoint}" + JSON_FORMAT + FORMAT_VERSION_2;
            
            await WebClient.QueueWebRequestAsync(_endpoint, async _Response =>
            {
                if (await ParseResponseAsync<ParseResponse.Root>(_Endpoint, _Response) is {} _parseResponse)
                {
                    if (CheckResponse(_parseResponse) is {} _message)
                    {
                        return _message;
                    }
                    
                    if (_parseResponse.Json?.Text is {} _text)
                    {
                        _content = _text;
                    }
                    else
                    {
                        Debug.LogError($"The response content was null. {(WebClient.BASE_ADDRESS + _Endpoint).ToHyperlink()}");
                    }
                }

                return null;
                
            }, _CancellationToken);

            return _content;
        }
        
        /// <summary>
        /// Parses the API response to the given <see cref="Type"/> <c>T</c>. <br/>
        /// <i>Also writes the response to <see cref="GlobalSettings.JsonFilePath"/> in editor.</i>
        /// </summary>
        /// <param name="_Endpoint">The endpoint from which the response was recieved.</param>
        /// <param name="_Response">The API response to parse.</param>
        /// <typeparam name="T">Must be of <see cref="Type"/> <see cref="QueryResponse"/>.<see cref="QueryResponse.Root"/> or <see cref="ParseResponse"/>.<see cref="ParseResponse.Root"/> and implement the interface <see cref="IRoot"/>.</typeparam>
        /// <returns>The parsed API response as the given <see cref="Type"/> <c>T</c>, or <c>null</c> if the response couldn't be parsed.</returns>
        /// <exception cref="Exception">Logs any <see cref="Exception"/> that occured during deserialization.</exception>
        private static async Task<T?> ParseResponseAsync<T>(string _Endpoint, string _Response) where T : IRoot
        {
#if UNITY_EDITOR
            await File.WriteAllTextAsync(GlobalSettings.JsonFilePath, _Response);
#endif
            try
            {
                if (await JsonSerializer.DeserializeAsync<T>(_Response.ToMemoryStream(Encoding.UTF8)) is {} _response)
                {
                    return _response;
                }
                
                Debug.LogError($"Could not parse the response of: {(WebClient.BASE_ADDRESS + _Endpoint).ToHyperlink()}");
            }
            catch (Exception _exception)
            {
                Debug.LogException(_exception);
            }

            return default;
        }

        /// <summary>
        /// Checks if the given <c>_Endpoint</c> starts with <c>Category:</c>.
        /// </summary>
        /// <param name="_Endpoint">The endpoint to check.</param>
        /// <returns><c>true</c> if the <c>_Endpoint</c> starts with <c>Category:</c>, otherwise <c>false</c>.</returns>
        private static bool CheckIfCategory(string _Endpoint)
        {
            const string _CATEGORY = "Category:";
            
            if (!_Endpoint.StartsWith(_CATEGORY))
            {
                Debug.LogError($"The endpoint: [{_Endpoint}] doesn't start with [{_CATEGORY.Bold()}], only endpoints that start with [{_CATEGORY.Bold()}] are allowed.");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Checks it the response contains any <see cref="IRoot.Error"/> or <see cref="IRoot.Warnings"/>, and prints them to the console if they exist.
        /// </summary>
        /// <param name="_Response">The response to check.</param>
        /// <typeparam name="T">Must be of <see cref="Type"/> <see cref="QueryResponse"/>.<see cref="QueryResponse.Root"/> or <see cref="ParseResponse"/>.<see cref="ParseResponse.Root"/> and implement the interface <see cref="IRoot"/>.</typeparam>
        /// <returns>
        /// <c>null</c> indicates a successful response, otherwise this will contain the message why the request didn't succeed. <br/>
        /// <i>Should be passed back to the <see cref="WebClient"/> (if not <c>null</c>), so the request is tried again.</i>
        /// </returns>
        private static string? CheckResponse<T>(T _Response) where T : IRoot
        {
            if (_Response.Error.ContainsKey(RATE_LIMITED_ERROR))
            {
                return "API rate limit reached.";
            }
            if (_Response.Error.Count > 0)
            {
                Debug.LogError(string.Join("\n", _Response.Error.Select(_Error => _Error.Key + ": " + _Error.Value)));
            }
            if (_Response.Warnings.Count > 0)
            {
                Debug.LogWarning(string.Join("\n", _Response.Warnings.Select(_Error => _Error.Key + ": " + _Error.Value)));
            }

            return null;
        }
        #endregion
    }
}