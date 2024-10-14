#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace YGODeckBuilder.Web.Yugipedia
{
    /// <summary>
    /// <c>Json</c> class wrapper for <c>action=query</c> responses. <br/>
    /// <i>Use &lt;<see cref="QueryResponse"/>.<see cref="Root"/>&gt; when deserializing.</i>
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    internal abstract class QueryResponse
    {
        /// <summary>
        /// Root of the <c>json</c> object.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal class Root : IRoot
        {
            #region Properties
            /// <summary>
            /// Indicates if the request is complete or not.
            /// </summary>
            [JsonPropertyName("batchcomplete")]
            public bool BatchComplete { get; init; }
            /// <summary>
            /// <see cref="Continue"/>
            /// </summary>
            [JsonPropertyName("continue")]
            public Continue? Continue { get; init; }
            /// <summary>
            /// Describes the limits applied to the query
            /// </summary>
            [JsonPropertyName("limits")]
            public Dictionary<string, JsonElement> Limits { get; init; } = new();
            /// <summary>
            /// A collection of <see cref="CategoryMember"/>.
            /// </summary>
            [JsonPropertyName("query")] // ReSharper disable once CollectionNeverUpdated.Global
            public Dictionary<string, List<CategoryMember>> Query { get; init; } = new();
            
            [JsonPropertyName("error")]
            public Dictionary<string, JsonElement> Error { get; init; } = new();
            [JsonPropertyName("warnings")]
            public Dictionary<string, JsonElement> Warnings { get; init; } = new();
            #endregion
        }
        
        /// <summary>
        /// When all the data is not returned in the response of a query, there will be a continue attribute to indicate that there is more data.
        /// </summary>
        internal class Continue
        {
            #region Properties
            /// <summary>
            /// To get further data, add this value to the original request.
            /// </summary>
            [JsonPropertyName("cmcontinue")]
            public string CmContinue { get; init; } = string.Empty;
            #endregion
        }
        
        /// <summary>
        /// Represents one member of the requested category.
        /// </summary>
        internal class CategoryMember
        {
            #region Properties
            /// <summary>
            /// The unescaped endpoint.
            /// </summary>
            [JsonPropertyName("title")]
            public string Title { get; init; } = string.Empty;
            #endregion
        }
    }
}