using AR_Inventory.Entities;
using SpatialEntity;
using StereoKit;

namespace AR_Inventory;

public static class App
{
    internal static EntityContext Context { get; private set; }
    internal static ItemService ItemService { get; private set; }
    internal static PassthroughFBExt Passthrough { get; private set; }
    internal static SpatialEntityFBExt SpatialEntity { get; private set; }
    internal static bool IsAndroid { get; set; }

    // Toggle some useful dev visuals
    internal static bool DEBUG_ON = false;

    public static void Init()
    {
        // OpenXR Extensions. These must be created BEFORE SK.Init is called
        Passthrough = SK.AddStepper<PassthroughFBExt>();
        SpatialEntity = SK.AddStepper<SpatialEntityFBExt>();

        Context = new EntityContext();
        ItemService = new ItemService();
    }
}