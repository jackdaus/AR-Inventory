using ARInventory.Entities;
using ARInventory.Entities.Models;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
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

		internal static EntityContext Context; // TODO make a singleton?

		Matrix   floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
		Material floorMaterial;

		public void Init()
		{
			floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
			floorMaterial.Transparency = Transparency.Blend;

            Context = new EntityContext();

			//SK.AddStepper<Logger>();
			//SK.AddStepper<DebugWindow>();
			SK.AddStepper<ManageInventory>();
        }
		
		public void Step()
		{
			if (SK.System.displayType == Display.Opaque)
				Default.MeshCube.Draw(floorMaterial, floorTransform);
        }
    }
}