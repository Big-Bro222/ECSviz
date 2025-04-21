using UnityEditor;
using UnityEngine;

public class SubModuleEditorWindow : EditorWindow
{
    private string folderPath;
    private Object folderObject;
    
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
        folderPath = EditorPrefs.GetString(UnityCommonConfig.EditorPrefsKey, "");
        if (!string.IsNullOrEmpty(folderPath))
        {
            folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
        }
    }

    // Optional: Draw something in the window
    private void OnGUI()
    {
        GUILayout.Label("Drag the submodule root folder here:", EditorStyles.boldLabel);

        // Object field that only accepts folders
        EditorGUI.BeginChangeCheck();
        folderObject = EditorGUILayout.ObjectField("Folder", folderObject, typeof(DefaultAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            string path = AssetDatabase.GetAssetPath(folderObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                UnityCommonConfig.CommonPacakgePath = path;
                folderPath = path;
                EditorPrefs.SetString(UnityCommonConfig.EditorPrefsKey, folderPath);
            }
            else
            {
                Debug.LogWarning("The selected object is not a valid folder.");
                folderObject = null;
                EditorPrefs.DeleteKey(UnityCommonConfig.EditorPrefsKey);
            }
        }
        
        if (!string.IsNullOrEmpty(folderPath))
        {
            EditorGUILayout.HelpBox("Current folder: " + folderPath, MessageType.Info);
        }
    }
}

