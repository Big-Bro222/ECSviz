using System.IO;
using SimpleFolderIcon.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SubModuleIconDrawer
{
    private static SubModuleSO _submoduleSaver;

    static SubModuleIconDrawer()
    {
        _submoduleSaver = SubModuleSO.LoadOrCreate();
        EditorApplication.projectWindowItemOnGUI += DrawFolderIcon;
        SubModuleInfoFetcher.RefreshSubmodulesModel();
    }

    static void DrawFolderIcon(string guid, Rect rect)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
       
        

        if (path == "" ||
            Event.current.type != EventType.Repaint ||
            !File.GetAttributes(path).HasFlag(FileAttributes.Directory) ||
            !_submoduleSaver.Submodules.Contains(path))
        {
            return;
        }
        
        Rect imageRect;

        if (rect.height > 20)
        {
            imageRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.width + 2);
        }
        else if (rect.x > 20)
        {
            imageRect = new Rect(rect.x - 1, rect.y - 1, rect.height + 2, rect.height + 2);
        }
        else
        {
            imageRect = new Rect(rect.x + 2, rect.y - 1, rect.height + 2, rect.height + 2);
        }

        var texture = _submoduleSaver.DesignatedIcon;
        if (texture == null)
        {
            return;
        }

        GUI.DrawTexture(imageRect, texture);
    }
}