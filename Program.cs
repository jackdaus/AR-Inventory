using AR_Inventory.DevTools;
using StereoKit;
using StereoKit.Framework;

namespace AR_Inventory;

public class Program
{
    static void Main(string[] args)
	{
        // Initialize AR-Inventory app
        App.Init();

        // Initialize StereoKit
        SKSettings settings = new SKSettings
		{
			appName = "AR Inventory",
			assetsFolder = "Assets",
		};
        if (!SK.Initialize(settings))
			return;

        // Floor asset
        Matrix   floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
		Material floorMaterial  = new Material("floor.hlsl");
		floorMaterial.Transparency = Transparency.Blend;

        // Load all system anchors (if feature is available)
        App.SpatialEntity.Enabled = true;
        if (App.SpatialEntity.Available)
            App.SpatialEntity.LoadAllAnchors();

        // Initialize Steppers
        ManageInventory manageInventory = SK.AddStepper<ManageInventory>();
        Minimap minimap = SK.AddStepper<Minimap>();
        Search search = SK.AddStepper<Search>();

        SK.AddStepper<Logger>();
        SK.AddStepper<DebugWindow>();
        SK.AddStepper<DebugFBSpatialEntity>();

        // Radial hand menu
        SK.AddStepper(new HandMenuRadial(
            new HandRadialLayer("Root",
                new HandMenuItem("", Catalog.Sprites.IconAdd, () => manageInventory.AddItem()),
                new HandMenuItem("", Catalog.Sprites.IconRadar, () => minimap.Enabled = !minimap.Enabled),
                new HandMenuItem("", Catalog.Sprites.IconSearch, () =>
                {
                    Vec3 position = Input.Head.position + Input.Head.Forward * 50 * U.cm;
                    Quat orientation = Quat.LookAt(position, Input.Head.position);
                    search.TeleportMenu(new Pose(position, orientation));
                }),
                new HandMenuItem("", Catalog.Sprites.IconBug, () => App.DEBUG_ON = !App.DEBUG_ON),
                new HandMenuItem("Cancel", null, null))
            ));

        // Core application loop
        SK.Run(() => {
            // Only draw floor when using VR headset with no AR passthrough
            if (SK.System.displayType == Display.Opaque && !App.Passthrough.EnabledPassthrough)
				Mesh.Cube.Draw(floorMaterial, floorTransform);
		});
	}
}