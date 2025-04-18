using UnityEngine;

public class VizardLogger:IVizardLogger
{
    //TODO:implement a logger file in standalone version
    public VizardLogger()
    {
        Log($"{Application.productName} version :{Application.version} Platform: {Application.platform}");
    }
    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void LogError(string message)
    {
        Debug.LogError(message);
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void Log(string message, Object context)
    {
        Debug.Log( message, context);
    }

    public void LogError(string message, Object context)
    {
        Debug.LogError( message, context);
    }

    public void LogWarning(string message, Object context)
    {
        Debug.LogWarning( message, context);
    }
}
