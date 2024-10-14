#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YGODeckBuilder.Extensions;

namespace YGODeckBuilder.Web
{
    /// <summary>
    /// For sending <c>GET</c> request through <see cref="UnityWebRequest"/>. <br/>
    /// <i>Only sends request to <see cref="BASE_ADDRESS"/>.</i>
    /// </summary>
    internal static class WebClient
    {
        #region Constants
        /// <summary>
        /// Base address for the API.
        /// </summary>
        internal const string BASE_ADDRESS = "https://yugipedia.com/wiki/";
        /// <summary>
        /// Time in seconds after which the web request will be aborted.
        /// </summary>
        private const int TIMEOUT = 30;
        /// <summary>
        /// The maximum number of attempts one web request will be sent, before being aborted.
        /// </summary>
        private const uint MAX_ATTEMPTS = 3;
        /// <summary>
        /// Time in milliseconds to wait before making a new attempt to send the web request.
        /// </summary>
        private const int RETRY_DELAY = 500;
        #endregion

        #region Fields
        /// <summary>
        /// The <see cref="UnityWebRequest"/> that is currently being used for sending a request. <br/>
        /// <i>There will only ever be one active <see cref="UnityWebRequest"/> sending at a time.</i>
        /// </summary>
        private static UnityWebRequest? unityWebRequest;
        /// <summary>
        /// Teh request headers to use with every web request. <br/>
        /// <b>Key:</b> The header name. <br/>
        /// <b>Value:</b> The header value.
        /// </summary>
        private static readonly ReadOnlyDictionary<string, string> requestHeaders = new(new Dictionary<string, string>
        {
            // Is needed to follow the API Etiquette for: https://www.mediawiki.org/wiki/API:Etiquette // TODO: Check what email to set in build.
            { "User-Agent", $"{Application.productName}/{Application.version} ({Application.companyName}, {Environment.GetEnvironmentVariable("email")}) Unity/{Application.unityVersion}" },
            { "Accept", "application/json" },
            { "Accept-Encoding", "gzip" }
        });
        /// <summary>
        /// Contains all web requests that still need to be sent. <br/>
        /// <b>Request:</b> The request that will be sent. <br/>
        /// <b>RequestCompletionSource:</b> The <see cref="TaskCompletionSource{TResult}"/> that awaits the request in <see cref="QueueWebRequestAsync"/>.
        /// </summary>
        private static readonly Queue<(Func<Task> Request, TaskCompletionSource<object?> RequestCompletionSource)> requestQueue = new();
        /// <summary>
        /// Will be <c>true</c> when <see cref="unityWebRequest"/> is currently sending a request.
        /// </summary>
        private static bool awaitingRequest;
        #endregion
        
        #region Methods
        /// <summary>
        /// Aborts the currently running <see cref="unityWebRequest"/> and clears the <see cref="requestQueue"/>.
        /// </summary>
        /// <param name="_CancellationTokenSource">Should be the <see cref="CancellationTokenSource"/> that holds the <see cref="CancellationToken"/> that was used in <see cref="QueueWebRequestAsync"/>.</param>
        internal static void Abort(CancellationTokenSource _CancellationTokenSource)
        {
            _CancellationTokenSource.Cancel();
            unityWebRequest?.Abort();
            
            while (requestQueue.TryDequeue(out var _request))
            {
                _request.RequestCompletionSource.TrySetCanceled();
            }
        }
        
        /// <summary>
        /// Adds a new web request to the <see cref="requestQueue"/>. <br/>
        /// <i>If no web request is currently being sent, the added request will be sent immediately, otherwise it will be sent after all requests in <see cref="requestQueue"/> have been sent.</i>
        /// </summary>
        /// <param name="_Endpoint">
        /// The endpoint at <see cref="BASE_ADDRESS"/> to send the request to. <br/>
        /// <b>(Must not include the <c>/wiki/</c> part.)</b>
        /// </param>
        /// <param name="_ResponseCallback">
        /// Will be invoked with the response text on success. <br/>
        /// <b><see cref="string"/>:</b> The response text. <br/>
        /// <b><see cref="Task"/>&lt;<see cref="string"/>&gt;:</b> <c>null</c> indicates a successful response, otherwise this should be the message why the request should be send again. <br/>
        /// <i>Will be included in the error log if all requests fail.</i>
        /// </param>
        /// <param name="_CancellationToken">Cancels all queued requests.</param>
        /// <returns>The <see cref="Task.Status"/> of the <see cref="TaskCompletionSource{TResult}"/>.</returns>
        internal static async Task<TaskStatus> QueueWebRequestAsync(string _Endpoint, Func<string, Task<string?>> _ResponseCallback, CancellationToken _CancellationToken)
        {
            if (_CancellationToken.IsCancellationRequested)
            {
                return TaskStatus.Canceled;
            }
            
            var _waitForRequest = new TaskCompletionSource<object?>();
            
            requestQueue.Enqueue((async () =>
            {
                await SendWebRequestAsync(_Endpoint, _ResponseCallback, _CancellationToken);

                _waitForRequest.TrySetResult(null);
                
            }, _waitForRequest));

            SentNextRequestAsync(_CancellationToken);
            
            try
            {
                await _waitForRequest.Task;
            }
            catch (TaskCanceledException) when(_waitForRequest.Task.IsCanceled) { /* Expected behavior. */ }

            return _waitForRequest.Task.Status;
        }
        
        /// <summary>
        /// Sends the next request in <see cref="requestQueue"/>.
        /// </summary>
        /// <param name="_CancellationToken">Won't send the next request if a cancellation has been requested.</param>
        private static async void SentNextRequestAsync(CancellationToken _CancellationToken)
        {
            if (requestQueue.Count > 0 && !awaitingRequest && !_CancellationToken.IsCancellationRequested)
            {
                awaitingRequest = true;
                
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(1000); // The wiki doesn't allow more than one requests per second.
                
                if (requestQueue.TryDequeue(out var _request))
                {
                    await _request.Request();
                }
                
                awaitingRequest = false;
                
                SentNextRequestAsync(_CancellationToken);
            }
        }
        
        /// <summary>
        /// Sends a <c>GET</c> request to the given <c>_Endpoint</c> at <see cref="BASE_ADDRESS"/>.
        /// </summary>
        /// <param name="_Endpoint">The endpoint at <see cref="BASE_ADDRESS"/> to send the request to.</param>
        /// <param name="_ResponseCallback">
        /// Will be invoked with the response text on success. <br/>
        /// <b><see cref="string"/>:</b> The response text. <br/>
        /// <b><see cref="Task"/>&lt;<see cref="string"/>&gt;:</b> <c>null</c> indicates a successful response, otherwise this should be the message why the request should be send again. <br/>
        /// <i>Will be included in the error log if all requests fail.</i>
        /// </param>
        /// <param name="_CancellationToken">Indicates when the download should be canceled.</param>
        /// <exception cref="Exception">Logs any <see cref="Exception"/> that occured while sending the request.</exception>
        private static async Task SendWebRequestAsync(string _Endpoint, Func<string, Task<string?>> _ResponseCallback, CancellationToken _CancellationToken)
        {
            var _attempt = 1;
            var _waitTime = RETRY_DELAY;
            var _message = string.Empty;

            try
            {
                do
                {
                    unityWebRequest = UnityWebRequest.Get(BASE_ADDRESS + _Endpoint);
                    unityWebRequest.timeout = TIMEOUT;
                    
                    foreach (var (_name, _value) in requestHeaders)
                    {
                        unityWebRequest.SetRequestHeader(_name, _value);
                    }

                    var _unityWebRequestAsyncOperation = unityWebRequest.SendWebRequest();
                    
                    while (!_unityWebRequestAsyncOperation.isDone)
                    {
                        await Task.Yield();
                    }
                    
                    if (unityWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        // The API returns success even when the response message contains warnings/errors.
                        // This lets the "_ResponseCallback" decide if the request should be sent again or if it was successful.
                        if (await _ResponseCallback(unityWebRequest.downloadHandler.text) is {} _retryMessage)
                        {
                            _message = _retryMessage;
                        }
                        else
                        { 
                            break;
                        }
                    }
                    if (!_CancellationToken.IsCancellationRequested)
                    {
                        Debug.LogError($"Attempt: {_attempt} [{unityWebRequest.result}/{(HttpStatusCode)unityWebRequest.responseCode}] {unityWebRequest.error}");
                    }
                
                    await Task.Delay(_waitTime *= 2, _CancellationToken);

                } while (_attempt++ < MAX_ATTEMPTS && !_CancellationToken.IsCancellationRequested);
            }
            catch (TaskCanceledException) when(_CancellationToken.IsCancellationRequested) { /* Expected behavior. */ }
            catch (OperationCanceledException) when (_CancellationToken.IsCancellationRequested) { /* Expected behavior. */ }
            catch (Exception _exception)
            {
                Debug.LogException(_exception);
            }
            finally
            {
                if (!_CancellationToken.IsCancellationRequested && _attempt > MAX_ATTEMPTS)
                {
                    Debug.LogError($"Download for [{unityWebRequest?.url.ToHyperlink() ?? _Endpoint}] failed{(_message == string.Empty ? "." : $": {_message}")}".Bold());
                }
            
                Cleanup();
                SentNextRequestAsync(_CancellationToken);   
            }
        }
        
        /// <summary>
        /// Cleans up after the current web request has completed.
        /// </summary>
        private static void Cleanup()
        {
            unityWebRequest?.Dispose();
            unityWebRequest = null;
        }
        #endregion
    }
}