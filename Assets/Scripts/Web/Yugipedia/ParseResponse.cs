#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace YGODeckBuilder.Web.Yugipedia
{
    /// <summary>
    /// <c>Json</c> class wrapper for <c>action=parse</c> responses. <br/>
    /// <i>Use &lt;<see cref="ParseResponse"/>.<see cref="Root"/>&gt; when deserializing.</i>
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    internal abstract class ParseResponse
    {
        /// <summary>
        /// Root of the <c>json</c> object.
        /// </summary>
        internal sealed class Root : IRoot
        {
            #region Properties
            /// <summary>
            /// <see cref="JSON"/>.
            /// </summary>
            [JsonPropertyName("parse")]
            public JSON? Json { get; init; }
            
            [JsonPropertyName("error")]
            public Dictionary<string, JsonElement> Error { get; init; } = new();
            [JsonPropertyName("warnings")]
            public Dictionary<string, JsonElement> Warnings { get; init; } = new();
            #endregion
        }
        
        /// <summary>
        /// Wrapper for the <c>json</c>.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal sealed class JSON
        {
            #region Properties
            /// <summary>
            /// For <c>prop=wikitext</c> responses.
            /// </summary>
            [JsonPropertyName("wikitext")]
            public string WikiText { get; init; } = string.Empty;
            /// <summary>
            /// For <c>prop=text</c> responses.
            /// </summary>
            [JsonPropertyName("text")]
            public string Text { get; init; } = string.Empty;
            #endregion
        }
    }
}