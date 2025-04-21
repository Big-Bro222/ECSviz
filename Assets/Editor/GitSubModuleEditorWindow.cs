using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class GitSubmoduleStatusEditorWindow : EditorWindow
{
    private class SubmoduleInfo
    {
        public string name;
        public string path;
        public string branch;
        public int commitsAhead;
        public int commitsBehind;
        public bool hasLocalChanges;
        
    }

    private List<SubmoduleInfo> submodules = new List<SubmoduleInfo>();
    private double lastRefreshTime = 0f;
    private const float refreshInterval = 10f; // seconds
    private Texture2D _submoduleIcon_Default;
    private Texture2D _submoduleIcon_Ahead;
    private Texture2D _submoduleIcon_Behind;
    private Texture2D _submoduleIcon_AheadAndBehind;
    private Texture2D _submoduleIcon_Unstaged;
    private SubModuleSO _submoduleSaver;
    
    [MenuItem("Window/My Tools/Git Submodule Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitSubmoduleStatusEditorWindow>();
        window.titleContent = new GUIContent("Git Submodules");
        window.Show();
    }

    private void OnEnable()
    {
        RefreshSubmodules();
        EditorApplication.update += OnEditorUpdate;
        // Load the saved folder path when the window opens
        _submoduleSaver = SubModuleSO.LoadOrCreate();
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
        {
            RefreshSubmodules();
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawSubModuleStatus();
        
        GUILayout.Label("SubModulefolder Icons:", EditorStyles.boldLabel);

        // Object field that only accepts folders
        EditorGUI.BeginChangeCheck();
        _submoduleIcon_Default = (Texture2D)EditorGUILayout.ObjectField("Default",_submoduleSaver.SubmoduleIcon_Default, typeof(Texture2D), false);
        _submoduleIcon_Ahead = (Texture2D)EditorGUILayout.ObjectField("Ahead Commit",_submoduleSaver.SubmoduleIcon_Ahead, typeof(Texture2D), false);
        _submoduleIcon_Behind = (Texture2D)EditorGUILayout.ObjectField("Behind Commit",_submoduleSaver.SubmoduleIcon_Behind, typeof(Texture2D), false);
        _submoduleIcon_AheadAndBehind=(Texture2D)EditorGUILayout.ObjectField("Ahead&Behind Commit",_submoduleSaver.SubmoduleIcon_AheadAndBehind, typeof(Texture2D), false);
        _submoduleIcon_Unstaged = (Texture2D)EditorGUILayout.ObjectField("Unstaged",_submoduleSaver.SubmoduleIcon_Unstaged, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            _submoduleSaver.SubmoduleIcon_Default = _submoduleIcon_Default;
            _submoduleSaver.SubmoduleIcon_Ahead = _submoduleIcon_Ahead;
            _submoduleSaver.SubmoduleIcon_Behind = _submoduleIcon_Behind;
            _submoduleSaver.SubmoduleIcon_AheadAndBehind = _submoduleIcon_AheadAndBehind;
            _submoduleSaver.SubmoduleIcon_Unstaged = _submoduleIcon_Unstaged;
            EditorUtility.SetDirty(_submoduleSaver);
            AssetDatabase.SaveAssets();
        }
    }
    
    /// <summary>
    /// Block for showing Git SubModule status and simple git actions
    /// </summary>
    void DrawSubModuleStatus()
        {
            if (GUILayout.Button("🔄 Manual Refresh"))
            {
                RefreshSubmodules();
            }

            GUILayout.Space(10);

            if (submodules.Count == 0)
            {
                EditorGUILayout.HelpBox("No Git submodules found.", MessageType.Info);
                return;
            }

            foreach (var sub in submodules)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Submodule:", sub.name);
                EditorGUILayout.LabelField("Path:", sub.path);
                EditorGUILayout.LabelField("Branch:", sub.branch ?? "(unknown)");

                if (sub.hasLocalChanges)
                {
                    EditorGUILayout.HelpBox("✏️ Has local changes", MessageType.Warning);
                    _submoduleSaver.SetSubModuleIcon(GitSubmoduleStatus.Unstaged);
                }

                if (sub.commitsBehind > 0 && sub.commitsAhead > 0)
                {
                    EditorGUILayout.HelpBox($"⬇️ Behind by {sub.commitsBehind} commits, ⬆️ ahead by {sub.commitsAhead}",
                        MessageType.Warning);
                    _submoduleSaver.SetSubModuleIcon(GitSubmoduleStatus.AheadAndBehind);
                }
                else if (sub.commitsBehind > 0)
                {
                    EditorGUILayout.HelpBox($"⬇️ Needs Pull ({sub.commitsBehind} commits behind)", MessageType.Warning);
                    _submoduleSaver.SetSubModuleIcon(GitSubmoduleStatus.Behind);
                }
                else if (sub.commitsAhead > 0)
                {
                    EditorGUILayout.HelpBox($"⬆️ Needs Push ({sub.commitsAhead} commits ahead)", MessageType.Warning);
                    _submoduleSaver.SetSubModuleIcon(GitSubmoduleStatus.Ahead);
                }
                else if (!sub.hasLocalChanges)
                {
                    EditorGUILayout.HelpBox("✅ Clean & up to date", MessageType.Info);
                    _submoduleSaver.SetSubModuleIcon(GitSubmoduleStatus.Default);
                }


                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Pull")) RunGitCommand("pull", sub.path);
                if (GUILayout.Button("Push")) RunGitCommand("push", sub.path);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

    private void RefreshSubmodules()
    {
        lastRefreshTime = EditorApplication.timeSinceStartup;
        submodules.Clear();
        //_submoduleSaver.Submodules.Clear();

        string rootPath = Directory.GetParent(Application.dataPath).FullName;
        string gitmodulesPath = Path.Combine(rootPath, ".gitmodules");

        if (!File.Exists(gitmodulesPath)) return;

        var lines = File.ReadAllLines(gitmodulesPath);
        SubmoduleInfo current = null;

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("[submodule"))
            {
                current = new SubmoduleInfo();
                submodules.Add(current);
            }
            else if (line.Trim().StartsWith("path ="))
            {
                current.path = line.Split('=')[1].Trim();
                //_submoduleSaver.Submodules.Add(current.path);
                current.name = Path.GetFileName(current.path);
                current.branch = GetBranchName(current.path);
                int[] commitCounts = GetAheadBehindCounts(current.path);
                current.commitsBehind = commitCounts[0];
                current.commitsAhead = commitCounts[1];
                current.hasLocalChanges = HasLocalChanges(current.path);
            }
        }
    }

    private string GetBranchName(string relativePath)
    {
        return RunGitCommand("rev-parse --abbrev-ref HEAD", relativePath)?.Trim();
    }
    
    private int[] GetAheadBehindCounts(string relativePath)
    {
        string output = RunGitCommand("rev-list --left-right --count @{u}...HEAD", relativePath);
        int[] result = new int[2]; // [behind, ahead]

        if (!string.IsNullOrWhiteSpace(output))
        {
            var parts = output.Split('\t');
            if (parts.Length == 2)
            {
                int.TryParse(parts[0], out result[0]); // behind
                int.TryParse(parts[1], out result[1]); // ahead
            }
        }

        return result;
    }


    private bool HasLocalChanges(string relativePath)
    {
        string output = RunGitCommand("status --porcelain", relativePath);
        return !string.IsNullOrWhiteSpace(output);
    }

    private string RunGitCommand(string arguments, string relativePath)
    {
        try
        {
            string workingDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativePath);

            var startInfo = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Git error in [{relativePath}]: {ex.Message}");
            return null;
        }
    }
}

public class SubModuleSO : ScriptableObject
{
    public Texture2D SubmoduleIcon_Default = default;
    public Texture2D SubmoduleIcon_Ahead = default;
    public Texture2D SubmoduleIcon_Behind = default;
    public Texture2D SubmoduleIcon_AheadAndBehind = default;
    public Texture2D SubmoduleIcon_Unstaged = default;
    public List<string> Submodules=new List<string>();
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
    public Texture2D DesignatedIcon => designatedIcon;

    public void SetSubModuleIcon(GitSubmoduleStatus submoduleStatus)
    {
        switch (submoduleStatus)
        {
            case GitSubmoduleStatus.Ahead:
                designatedIcon = SubmoduleIcon_Ahead;
                break;
            case GitSubmoduleStatus.Behind:
                designatedIcon = SubmoduleIcon_Ahead;
                break;
            case GitSubmoduleStatus.AheadAndBehind:
                designatedIcon = SubmoduleIcon_Ahead;
                break;
            case GitSubmoduleStatus.Unstaged:
                designatedIcon = SubmoduleIcon_Ahead;
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
