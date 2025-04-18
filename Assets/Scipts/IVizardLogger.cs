using UnityEngine;

public interface IVizardLogger
{
    public void Log(string message);
    public void LogError(string message);
    public void LogWarning(string message);
#if UNITY_EDITOR
    public void Log(string message,Object context);
    public void LogError(string message,Object context);
    public void LogWarning(string message,Object context);
#endif

}
