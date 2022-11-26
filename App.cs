using ARInventory.Entities;
using SpatialEntity;
using StereoKit;

namespace ARInventory
{
    public class App
    {
        public SKSettings Settings => new SKSettings { 
            appName           = "AR Inventory",
            assetsFolder      = "Assets",
            displayPreference = DisplayMode.MixedReality
        };

        internal static EntityContext Context;
        internal static ItemService ItemService;

        Matrix   floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
        Material floorMaterial;

        public static PassthroughFBExt   Passthrough;
        public static SpatialEntityFBExt SpatialEntity;
        public static bool IsAndroid;

        // Toggle some useful dev visuals
        public const bool DEBUG_ON = false;

        public void Init()
        {
            floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;

            Context     = new EntityContext();
            ItemService = new ItemService();

            // Start out with passthrough off
            Passthrough.EnabledPassthrough = false;
            
            // Load all system anchors (if feature is available)
            SpatialEntity.Enabled = true;
            if(SpatialEntity.Available)
                SpatialEntity.LoadAllAnchors();

            SK.AddStepper<Logger>();
            SK.AddStepper<ManageInventory>();
            SK.AddStepper<Search>();
            SK.AddStepper<Minimap>();

            if (DEBUG_ON)
            {
                SK.AddStepper<DebugWindow>();
                SK.AddStepper<DebugFBSpatialEntity>();
            }
		}
        
        public void Step()
        {
            // Only draw floor when using VR headset with no AR passthrough
            if (SK.System.displayType == Display.Opaque && !App.Passthrough.EnabledPassthrough)
                Default.MeshCube.Draw(floorMaterial, floorTransform);
        }
    }
}