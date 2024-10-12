using Plugins.CustomScriptTemplates.Scripts.Exceptions;
using UnityEditor;

namespace Plugins.CustomScriptTemplates.Scripts.PostProcess
{
    /// <summary>
    /// Compiles the MenuItems after the Package has been imported.
    /// </summary>
    [InitializeOnLoad]
    internal static class PackageImporter
    {
        #region Constructor
        /// <summary>
        /// <see cref="PackageImporter"/>.
        /// </summary>
        static PackageImporter()
        {
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Is called after the Package has successfully been imported. <br/>
        /// -> <see cref="AssetDatabase"/>.<see cref="AssetDatabase.importPackageCompleted"/>.
        /// </summary>
        /// <param name="_PackageName">The name of the imported Package.</param>
        private static void OnImportPackageCompleted(string _PackageName)
        {
            if (_PackageName != Naming.PackageName)
            {
                return;
            }
            
            var _scriptTemplatesSettingsAssets = AssetDatabase.FindAssets($"t:{typeof(ScriptTemplatesSettings)}", new[] { "Assets" });
            if (_scriptTemplatesSettingsAssets != null && _scriptTemplatesSettingsAssets.Length > 0)
            {
                var _scriptTemplatesSettingsAssetPath = AssetDatabase.GUIDToAssetPath(_scriptTemplatesSettingsAssets[0]);
                var _scriptTemplatesSettingsAsset = AssetDatabase.LoadAssetAtPath(_scriptTemplatesSettingsAssetPath, typeof(ScriptTemplatesSettings)) as ScriptTemplatesSettings;
                
                _scriptTemplatesSettingsAsset.CompileMenuItems();
            }
            else
            {
                throw new AssetNotFoundException($"No asset of type: [{typeof(ScriptTemplatesSettings)}], could be found. Reimport the package or uncomment the \"CreateAssetMenu\" line in the \"{nameof(ScriptTemplatesSettings)}.cs\"-script, and create the asset manually.");
            }
        }
        #endregion
    }
}