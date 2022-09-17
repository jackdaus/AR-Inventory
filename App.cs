using AR_Inventory;
using StereoKit;
using System;

namespace StereoKitApp
{
	public class App
	{
		public SKSettings Settings => new SKSettings { 
			appName           = "StereoKit Template",
			assetsFolder      = "Assets",
			displayPreference = DisplayMode.MixedReality
		};

		Pose  cubePose			= new Pose(0, 0, -0.5f, Quat.Identity);
		Model cube;
		Matrix   floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
		Material floorMaterial;
		Pose menuPose			= new Pose(0.4f, 0, -0.4f, Quat.LookDir(-1, 0, 1));

		public void Init()
		{
			// Create assets used by the app
			cube = Model.FromMesh(
				Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
				Default.MaterialUI);

			floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
			floorMaterial.Transparency = Transparency.Blend;

			SK.AddStepper<Logger>();
		}

		public void Step()
		{
			if (SK.System.displayType == Display.Opaque)
				Default.MeshCube.Draw(floorMaterial, floorTransform);

			UI.Handle("Cube", ref cubePose, cube.Bounds);
			cube.Draw(cubePose.ToMatrix());

			testWindow();

        }

		private void testWindow()
		{
			UI.WindowBegin("Test Window", ref menuPose);
			if (UI.Button("Test file")) testWriteFile();
			if (UI.Button("Pick file")) pickFile();
            //if (UI.Button("Quit")) SK.Quit();
            UI.WindowEnd();
		}

		private void testWriteFile()
		{
			Log.Info($"Begin testing file! {DateTime.Now}");
			var isSuccessful = Platform.WriteFile("my-new-file.txt", "this is the content");
			Log.Info($"Result of flile write: {isSuccessful}");
		}

		private void pickFile()
		{
			Platform.FilePicker(PickerMode.Open, logFileContents, null);
		}


		private void logFileContents(string file)
		{
			Platform.ReadFile(file, out string text);
			Log.Info(file);
			Log.Info(text);
        }
	}
}