#nullable enable

using System;

namespace YGODeckBuilder.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="string"/>.
    /// </summary>
    internal static class StringExtensions
    {
        #region Methods
        /// <summary>
        /// Escapes all invalid URL characters.
        /// </summary>
        /// <param name="_String">The <see cref="string"/>to escape the characters of.</param>
        /// <returns>A <see cref="string"/> with all invalid URL characters escaped.</returns>
        internal static string ToURLString(this string _String)
        {
            return Uri.EscapeDataString(_String.Replace(' ', '_'));
        }
        
        /// <summary>
        /// Converts this <see cref="string"/> into a hyperlink.
        /// </summary>
        /// <param name="_UriString">The URL/Path.</param>
        /// <param name="_Content">The display name. (Will be the <c>_UriString</c> if not set)</param>
        /// <param name="_Line">
        /// If the link is for a file on the local machine, this is the line number where to jump to when opening the file. <br/>
        /// <i>Doesn't seem to work right now.</i>
        /// </param>
        /// <returns>This <see cref="string"/> converted into a hyperlink.</returns>
        internal static string ToHyperlink(this string _UriString, object? _Content = null, int? _Line = null)
        {
            return $"<a href=\"{_UriString}\"{(_Line == null ? string.Empty : $" line=\"{_Line}\"")}>{_Content ?? _UriString}</a>";
        }
        #endregion
    }
}