#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace YGODeckBuilder.Web.Yugipedia
{
    /// <summary>
    /// Contains <see cref="Error"/> and <see cref="Warnings"/> for the <see cref="YugipediaAPI"/> responses.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal interface IRoot
    {
        #region Properties
        /// <summary>
        /// Contains all error.
        /// </summary>
        public Dictionary<string, JsonElement> Error { get; init; }
        /// <summary>
        /// Contains all warnings.
        /// </summary>
        public Dictionary<string, JsonElement> Warnings { get; init; }
        #endregion
    }
}