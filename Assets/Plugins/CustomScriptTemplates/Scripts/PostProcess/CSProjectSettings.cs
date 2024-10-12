namespace Plugins.CustomScriptTemplates.Scripts.PostProcess
{
#if CUSTOM_SCRIPT_TEMPLATES
    /// <summary>
    /// Sets settings in the <c>.csproj</c>-file. <br/>
    /// <i>Only needed for development.</i>
    /// </summary>
    [InitializeOnLoad]
    // ReSharper disable once InconsistentNaming
    internal static class CSProjectSettings
    {
        #region Constructor
        /// <summary>
        /// <see cref="CSProjectSettings"/>.
        /// </summary>
        static CSProjectSettings()
        {
            CompilationPipeline.assemblyCompilationFinished += SetCSProjectSettings;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets settings in the <c>.csproj</c>-file.
        /// </summary>
        /// <param name="_AssemblyPath">The path to the <c>.dll</c> file.</param>
        /// <param name="_Messages">The compiler messages <i>(Can be empty)</i>.</param>
        // ReSharper disable once InconsistentNaming
        private static void SetCSProjectSettings(string _AssemblyPath, CompilerMessage[] _Messages)
        {
            const string _PROJECT_FILE_EXTENSION = ".csproj";
            const string _C_SHARP_VERSION_PATTERN = "<LangVersion>.*</LangVersion>";
            const string _TARGET_C_SHARP_VERSION = "<LangVersion>7.3</LangVersion>";
            const string _NULLABLE_PATTERN = "<Nullable>.*</Nullable>";
            const string _TARGET_NULLABILITY = "<Nullable>enable</Nullable>";
            const string _DOT_NET_VERSION_PATTERN = "<TargetFrameworkVersion>.*</TargetFrameworkVersion>";
            const string _TARGET_DOT_NET_VERSION = "<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>";
            const string _PROPERTY_GROUP = "</PropertyGroup>";
            
            var _searchedStrings = new (string Pattern, string Replacement, bool Found)[]
            {
                (_C_SHARP_VERSION_PATTERN, _TARGET_C_SHARP_VERSION, false),
                (_NULLABLE_PATTERN, _TARGET_NULLABILITY, false),
                (_DOT_NET_VERSION_PATTERN, _TARGET_DOT_NET_VERSION, false)
            };
            
            // ReSharper disable once PossibleNullReferenceException
            var _projectFilePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, Naming.CSProjectFile + _PROJECT_FILE_EXTENSION);
            
            if (File.Exists(_projectFilePath))
            {
                var _projectFileContents = File.ReadAllLines(_projectFilePath);

                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < _projectFileContents.Length; i++)
                {
                    ref var _line = ref _projectFileContents[i];
                    
                    // ReSharper disable once InconsistentNaming
                    for (var j = 0; j < _searchedStrings.Length; j++)
                    {
                        ref var _searchedString = ref _searchedStrings[j];
                        
                        if (_searchedString.Found)
                        {
                            continue;
                        }

                        if (Regex.IsMatch(_line, _searchedString.Pattern))
                        {
                            _line = _searchedString.Replacement;
                            _searchedString.Found = true;
                            break;
                        }
                    }

                    var _nullabilityIndex = _searchedStrings.FindIndex(_SearchedString => _SearchedString.Pattern == _NULLABLE_PATTERN);
                    if (!_searchedStrings[_nullabilityIndex].Found && _line.Contains(_PROPERTY_GROUP))
                    {
                        var _previousLineIndex = i - 1;
                        if (_previousLineIndex < 0)
                        {
                            _previousLineIndex = 0;
                        }
                        
                        var _whiteSpaceCount = _projectFileContents[_previousLineIndex].TakeWhile(_Character => _Character == ' ').Count();
                        
                        _line = string.Empty.PadRight(_whiteSpaceCount) + _TARGET_NULLABILITY + "\n" + _line;
                        _searchedStrings[_nullabilityIndex].Found = true;
                    }
                    
                    if (_searchedStrings.All(_SearchString => _SearchString.Found))
                    {
                        break;
                    }
                }
                
                File.WriteAllLines(_projectFilePath, _projectFileContents);
            }
            else
            {
                throw new FileNotFoundException($"The project file: [{Path.GetFileName(_projectFilePath)}], at path: [{_projectFilePath}], could not be found.");
            }
        }
        #endregion
    }
#endif
}