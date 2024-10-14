#nullable enable

using Sirenix.OdinInspector;
using UnityEngine;

namespace YGODeckBuilder.Settings
{
    /// <summary>
    /// Contains settings/data that should be accessible from the entire project. <br/>
    /// <i>For editor and build.</i>
    /// </summary>
    [CreateAssetMenu(menuName = "YGO/GlobalSettings", fileName = "GlobalSettings")]
    internal sealed class GlobalSettings : ScriptableObject
    {
        #region Inspector Fields
#if UNITY_EDITOR
        [Header("Editor References")]
        [FilePath(ParentFolder = OUTPUT_FOLDER, Extensions = "txt", AbsolutePath = true, RequireExistingPath = true)]
        [Tooltip("Absolute filepath to the Text.txt file.")]
        [SerializeField] private string textFilePath = string.Empty;
        [FilePath(ParentFolder = OUTPUT_FOLDER, Extensions = "json", AbsolutePath = true, RequireExistingPath = true)]
        [Tooltip("Absolute filepath to the Json.txt file.")]
        [SerializeField] private string jsonFilePath = string.Empty;
#endif
        #endregion

        #region Constants
        /// <summary>
        /// Relative path to the output folder.
        /// </summary>
        private const string OUTPUT_FOLDER = "Assets/Output";
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="GlobalSettings"/>.
        /// </summary>
        private static GlobalSettings? instance;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="instance"/>.
        /// </summary>
        private static GlobalSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance is null)
                {
                    Init();
                }          
#endif
                return instance!;
            }
        }
        /// <summary>
        /// <see cref="textFilePath"/>.
        /// </summary>
        internal static string TextFilePath => Instance.textFilePath;
        /// <summary>
        /// <see cref="jsonFilePath"/>.
        /// </summary>
        internal static string JsonFilePath => Instance.jsonFilePath;
        #endregion
        
        #region Methods
        /// <summary>
        /// Loads the <see cref="GlobalSettings"/> asset from the <see cref="Resources"/> folder and initializes <see cref="instance"/>.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            instance = Resources.Load<GlobalSettings>(nameof(GlobalSettings));
        }
        #endregion
    }
}