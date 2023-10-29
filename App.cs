using AR_Inventory.Entities;
using StereoKitFBSpatialEntity;
using StereoKit;

namespace AR_Inventory;

public static class App
{
    internal static PassthroughFBExt Passthrough { get; private set; }
    internal static SpatialEntityFBExt SpatialEntity { get; private set; }
    internal static EntityContext Context { get; private set; }
    internal static ItemService ItemService { get; private set; }
    internal static bool IsAndroid { get; set; }

    // Toggle some useful dev visuals
    internal static bool DEBUG_ON = false;

    /// <summary>
    /// Called before SK.Initialize
    /// </summary>
    public static void PreSKInit()
    {
        // OpenXR Extensions. These must be created BEFORE SK.Init is called
        Passthrough   = SK.AddStepper<PassthroughFBExt>();
        SpatialEntity = SK.AddStepper(new SpatialEntityFBExt(loadAnchorsOnInit: false));
    }

    /// <summary>
    /// Called after SK.Initialize
    /// </summary>
    public static void PostSKInit()
    {
        Context     = new EntityContext();
        ItemService = new ItemService();
    }
}