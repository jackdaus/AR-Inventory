﻿using ARInventory.Entities;
using ARInventory.Entities.Models;
using StereoKit;
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
			SK.AddStepper<ManageInventory>();
        }
		
		public void Step()
		{
			if (SK.System.displayType == Display.Opaque)
				Default.MeshCube.Draw(floorMaterial, floorTransform);

			UI.Handle("Cube", ref cubePose, cube.Bounds);
			cube.Draw(cubePose.ToMatrix());
        }
    }
}