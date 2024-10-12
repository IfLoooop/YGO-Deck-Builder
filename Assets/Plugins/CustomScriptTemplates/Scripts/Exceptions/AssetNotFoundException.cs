using System;

namespace Plugins.CustomScriptTemplates.Scripts.Exceptions
{
    /// <summary>
    /// Exception for when an asset couldn't be found.
    /// </summary>
    internal sealed class AssetNotFoundException : Exception
    {
        #region Constructors
        /// <summary>
        /// <see cref="AssetNotFoundException"/>.
        /// </summary>
        /// <param name="_Message">The error message that explains the reason for the exception.</param>
        internal AssetNotFoundException(string _Message) : base(_Message) { }
        #endregion
    }
}