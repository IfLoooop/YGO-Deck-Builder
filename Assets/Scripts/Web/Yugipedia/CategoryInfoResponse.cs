#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YGODeckBuilder.Web.Yugipedia
{
    /// <summary>
    /// <c>Json</c> class wrapper for <c>action=query&amp;prop=categoryinfo</c> responses. <br/>
    /// <i>Use &lt;<see cref="CategoryInfoResponse"/>.<see cref="Root"/>&gt; when deserializing.</i>
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal abstract class CategoryInfoResponse
    {
        /// <summary>
        /// Root of the <c>json</c> object.
        /// </summary>
        internal class Root : IRoot
        {
            #region Properties
            /// <summary>
            /// <see cref="CategoryInfoResponse.Query"/>.
            /// </summary>
            [JsonPropertyName("query")]
            public Query? Query { get; init; }
            
            [JsonPropertyName("error")]
            public Dictionary<string, JsonElement> Error { get; init; } = new();
            [JsonPropertyName("warnings")]
            public Dictionary<string, JsonElement> Warnings { get; init; } = new();
            #endregion
        }
        
        /// <summary>
        /// Contains the query results.
        /// </summary>
        internal class Query
        {
            #region Properties
            /// <summary>
            /// Contains information about the pages.
            /// </summary>
            [JsonPropertyName("pages")] // ReSharper disable once CollectionNeverUpdated.Global
            public List<Page> Pages { get; init; } = new();
            #endregion
        }
        
        /// <summary>
        /// Contains information about a page.
        /// </summary>
        internal class Page
        {
            #region Properties
            /// <summary>
            /// <see cref="CategoryInfoResponse.CategoryInfo"/>.
            /// </summary>
            [JsonPropertyName("categoryinfo")]
            public CategoryInfo? CategoryInfo { get; init; }
            #endregion
        }
        
        /// <summary>
        /// Contains metadata about the category that the page belongs to.
        /// </summary>
        internal class CategoryInfo
        {
            #region Properties
            /// <summary>
            /// The total number of pages in the category.
            /// </summary>
            [JsonPropertyName("pages")]
            public int Pages { get; init; }
            #endregion
        }
    }
}