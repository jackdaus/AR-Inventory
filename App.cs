using ARInventory.Entities;
using StereoKit;
using System;
using System.Linq;

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

		Pose  cubePose			= new Pose(0, 0, -0.5f, Quat.Identity);
		Model cube;
		Matrix   floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
		Material floorMaterial;

		public void Init()
		{
			// Create assets used by the app
			cube = Model.FromMesh(
				Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
				Default.MaterialUI);

			floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
			floorMaterial.Transparency = Transparency.Blend;

            Context = new EntityContext();

			SK.AddStepper<Logger>();
			SK.AddStepper<DebugWindow>();

			testContext();
        }

		public void Step()
		{
			if (SK.System.displayType == Display.Opaque)
				Default.MeshCube.Draw(floorMaterial, floorTransform);

			UI.Handle("Cube", ref cubePose, cube.Bounds);
			cube.Draw(cubePose.ToMatrix());
        }

        private void testContext()
        {
            App.Context.Items.Add(new Entities.Models.Item
            {
                Id = Guid.NewGuid(),
                //Location = Vec3.Zero,
                Title = "Paper towels",
                Quantity = 1
            });

            App.Context.Items.Add(new Entities.Models.Item
            {
                Id = Guid.NewGuid(),
                //Location = null,
                Title = "Can opener",
                Quantity = 1
            });

            var tempId = Guid.NewGuid();

            App.Context.Items.Add(new Entities.Models.Item
            {
                Id = tempId,
                //Location = null,
                Title = "e1",
                Quantity = 1
            });

            App.Context.Items.Add(new Entities.Models.Item
            {
                //Id = tempId,
                //Location = null,
                Title = "e2",
                Quantity = 1
            });

            App.Context.SaveChanges();
        }
    }
}