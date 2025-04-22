using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubModuleSO : ScriptableObject
{
    public Texture2D SubmoduleIcon_Default = default;
    public Texture2D SubmoduleIcon_Ahead = default;
    public Texture2D SubmoduleIcon_Behind = default;
    public Texture2D SubmoduleIcon_AheadAndBehind = default;
    public Texture2D SubmoduleIcon_Unstaged = default;
    public List<string> Submodules=new();
    public const string AssetPath = "Assets/Editor/GitSubModuleConfig.asset";
    private Texture2D designatedIcon = default;

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

    public Texture2D DesignatedIcon=>designatedIcon;

    public void SetSubModuleIcon(GitSubmoduleStatus submoduleStatus)
    {
        switch (submoduleStatus)
        {
            case GitSubmoduleStatus.Ahead:
                designatedIcon = SubmoduleIcon_Ahead;
                break;
            case GitSubmoduleStatus.Behind:
                designatedIcon = SubmoduleIcon_Behind;
                break;
            case GitSubmoduleStatus.AheadAndBehind:
                designatedIcon = SubmoduleIcon_AheadAndBehind;
                break;
            case GitSubmoduleStatus.Unstaged:
                designatedIcon = SubmoduleIcon_Unstaged;
                break;
            default:
                designatedIcon = SubmoduleIcon_Default;
                break;
        }
    }
}

public enum GitSubmoduleStatus
{
    Default,
    Ahead,
    Behind,
    AheadAndBehind,
    Unstaged
}
