using ARInventory.DevTools;
using ARInventory.Entities;
using SpatialEntity;
using StereoKit;
using StereoKit.Framework;

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
        public static bool DEBUG_ON = false;

        public void Init()
        {
            floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;

            Context     = new EntityContext();
            ItemService = new ItemService();

            // Start out with passthrough on
            Passthrough.EnabledPassthrough = true;
            
            // Load all system anchors (if feature is available)
            SpatialEntity.Enabled = true;
            if(SpatialEntity.Available)
                SpatialEntity.LoadAllAnchors();


            // Initialize Steppers
			ManageInventory manageInventory = SK.AddStepper<ManageInventory>();
			Minimap         minimap         = SK.AddStepper<Minimap>();
			Search          search          = SK.AddStepper<Search>();
            
            SK.AddStepper<Logger>();
            SK.AddStepper<DebugWindow>();
            SK.AddStepper<DebugFBSpatialEntity>();

            // Radial hand menu
			SK.AddStepper(new HandMenuRadial(
	            new HandRadialLayer("Root",
		            new HandMenuItem("", Catalog.Sprites.IconAdd,    () => manageInventory.AddItem()),
		            new HandMenuItem("", Catalog.Sprites.IconRadar,  () => minimap.Enabled = !minimap.Enabled),
		            new HandMenuItem("", Catalog.Sprites.IconSearch, () =>
                    {
                        Vec3 position    = Input.Head.position + Input.Head.Forward * 50 * U.cm;
                        Quat orientation = Quat.LookAt(position, Input.Head.position);
						search.TeleportMenu(new Pose(position, orientation));
                    }),
					new HandMenuItem("", Catalog.Sprites.IconBug, () => DEBUG_ON = !DEBUG_ON),
					new HandMenuItem("Cancel",  null, null))
	            ));
		}
        
        public void Step()
        {
            // Only draw floor when using VR headset with no AR passthrough
            if (SK.System.displayType == Display.Opaque && !App.Passthrough.EnabledPassthrough)
                Default.MeshCube.Draw(floorMaterial, floorTransform);
        }
    }
}