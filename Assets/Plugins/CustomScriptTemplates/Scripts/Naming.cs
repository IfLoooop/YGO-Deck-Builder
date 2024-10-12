namespace Plugins.CustomScriptTemplates.Scripts
{
    /// <summary>
    /// TODO
    /// </summary>
    internal static class Naming
    {
        #region Constants
        /// <summary>
        /// The name of the plugin (With spaces).
        /// </summary>
        private const string PLUGIN_NAME = "Custom Script Templates";
        #endregion

        #region Properties
        /// <summary>
        /// The name of the Package.
        /// </summary>
        internal static string PackageName => PLUGIN_NAME;
        /// <summary>
        /// The name of the <c>.csproj</c> file.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal static string CSProjectFile => PLUGIN_NAME;
        #endregion
    }
}