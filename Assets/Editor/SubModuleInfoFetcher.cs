using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SubModuleInfoFetcher
{
    public static double LastRefreshTime => lastRefreshTime;
    private static double lastRefreshTime = 0;
    private static SubModuleSO _submoduleSaver;
    
    static SubModuleInfoFetcher()
    {
        _submoduleSaver = SubModuleSO.LoadOrCreate();
    }
    
    private static List<SubmoduleInfo> FetchSubModuleInfo()
    {
        List<SubmoduleInfo> submodules = new();
        submodules.Clear();

        string rootPath = Directory.GetParent(Application.dataPath).FullName;
        string gitmodulesPath = Path.Combine(rootPath, ".gitmodules");

        if (!File.Exists(gitmodulesPath)) return null;

        var lines = File.ReadAllLines(gitmodulesPath);
        SubmoduleInfo current = null;

        foreach (var line in lines)
        {
            Debug.LogError(line);
            if (line.Trim().StartsWith("[submodule"))
            {
                current = new SubmoduleInfo();
                submodules.Add(current);
            }
            else if (line.Trim().StartsWith("path ="))
            {
                current.path = line.Split('=')[1].Trim();
                current.name = Path.GetFileName(current.path);
                current.branch = GetBranchName(current.path);
                int[] commitCounts = GetAheadBehindCounts(current.path);
                current.commitsBehind = commitCounts[0];
                current.commitsAhead = commitCounts[1];
                current.hasLocalChanges = HasLocalChanges(current.path);
            }
        }

        return submodules;
    }
    
    private static string GetBranchName(string relativePath)
    {
        return RunGitCommand("rev-parse --abbrev-ref HEAD", relativePath)?.Trim();
    }
    
    private static int[] GetAheadBehindCounts(string relativePath)
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


    private static bool HasLocalChanges(string relativePath)
    {
        string output = RunGitCommand("status --porcelain", relativePath);
        return !string.IsNullOrWhiteSpace(output);
    }

    public static string RunGitCommand(string arguments, string relativePath)
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
    
    public static List<SubmoduleInfo> RefreshSubmodulesModel()
    {
        lastRefreshTime = EditorApplication.timeSinceStartup;
        List<SubmoduleInfo> submoduleInfos = SubModuleInfoFetcher.FetchSubModuleInfo();
        _submoduleSaver.Submodules.Clear();
        foreach (var submoduleInfo in submoduleInfos)
        {
            _submoduleSaver.Submodules.Add(submoduleInfo.path);
        }
        EditorUtility.SetDirty(_submoduleSaver);
        AssetDatabase.SaveAssets();
        return submoduleInfos;
    }
}



public class SubmoduleInfo
{
    public string name;
    public string path;
    public string branch;
    public int commitsAhead;
    public int commitsBehind;
    public bool hasLocalChanges;
        
}
