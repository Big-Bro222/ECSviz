using UnityEditor;
using UnityEngine;

public class SubModuleEditorWindow : EditorWindow
{
    private string _folderPath;
    private Object _folderObject;
    private Texture2D _submoduleIcon_Default;
    private SubModuleSO _submoduleSaver;
    
    [MenuItem("Window/My Tools/Git SubModule Config")]
    public static void ShowWindow()
    {
        // Create and show the window
        var window = GetWindow<SubModuleEditorWindow>();
        window.titleContent = new GUIContent("Git SubModule Configuration");
        window.Show();
    }
    
    private void OnEnable()
    {
        // Load the saved folder path when the window opens
        _folderPath = EditorPrefs.GetString(UnityCommonConfig.EditorPrefsKey, "");
        if (!string.IsNullOrEmpty(_folderPath))
        {
            _folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_folderPath);
        }

        _submoduleSaver = SubModuleSO.LoadOrCreate();
    }

    // Optional: Draw something in the window
    private void OnGUI()
    {
        GUILayout.Label("Drag the submodule root folder here:", EditorStyles.boldLabel);

        // Object field that only accepts folders
        EditorGUI.BeginChangeCheck();
        _folderObject = EditorGUILayout.ObjectField("SubModule Root Folder", _folderObject, typeof(DefaultAsset), false);
        _submoduleSaver.DefaultFolderIcon = (Texture2D)EditorGUILayout.ObjectField("SubModule Icon Default", _submoduleSaver.DefaultFolderIcon, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            string path = AssetDatabase.GetAssetPath(_folderObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                UnityCommonConfig.CommonPacakgePath = path;
                _folderPath = path;
                EditorPrefs.SetString(UnityCommonConfig.EditorPrefsKey, _folderPath);
            }
            else
            {
                Debug.LogWarning("The selected object is not a valid folder.");
                _folderObject = null;
                EditorPrefs.DeleteKey(UnityCommonConfig.EditorPrefsKey);
            }
            
            EditorUtility.SetDirty(_submoduleSaver);
            AssetDatabase.SaveAssets();
        }
        
        if (!string.IsNullOrEmpty(_folderPath))
        {
            EditorGUILayout.HelpBox("Current folder: " + _folderPath, MessageType.Info);
        }
    }
}

public class SubModuleSO : ScriptableObject
{
    public Texture2D DefaultFolderIcon;

    public const string AssetPath = "Assets/Editor/GitSubModuleConfig.asset";

    public static SubModuleSO LoadOrCreate()
    {
        var settings = AssetDatabase.LoadAssetAtPath<SubModuleSO>(AssetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<SubModuleSO>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }
}

