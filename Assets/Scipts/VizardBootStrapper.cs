using UnityEngine;
using VContainer;
using VContainer.Unity;

public class VizardBootStrapper:LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IVizardLogger, VizardLogger>(Lifetime.Singleton);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Bootstrap()
    {
        //print software version
        //setup screen mode
        //TODO: Authentication
        SetupBindings();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void MonoBootstrap()
    {

    }


    private static void SetupBindings()
    {
        //Setupbinding for classes
        //Setupbinding for monos
        //Setupbinding for ECS systems
    }
}
