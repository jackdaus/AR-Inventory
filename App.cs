using StereoKitFBSpatialEntity;
using StereoKit;
using AR_Inventory.Domain;

namespace AR_Inventory;

public static class App
{
    internal static PassthroughFBExt Passthrough { get; private set; }
    internal static SpatialEntityFBExt SpatialEntity { get; private set; }
    internal static Db Db { get; private set; }
    internal static ItemService ItemService { get; private set; }
    internal static bool IsAndroid { get; set; }

    // Toggle some useful dev visuals
    internal static bool DEBUG_ON = false;

    /// <summary>
    /// Should be called before SK.Initialize
    /// </summary>
    public static void PreSKInit()
    {
        // OpenXR Extensions. These must be created BEFORE SK.Init is called
        Passthrough   = SK.AddStepper<PassthroughFBExt>();
        SpatialEntity = SK.AddStepper(new SpatialEntityFBExt(loadAnchorsOnInit: true));
    }

    /// <summary>
    /// Should be called after SK.Initialize
    /// </summary>
    public static void PostSKInit()
    {
        Db     = new Db();
        ItemService = new ItemService();
    }
}