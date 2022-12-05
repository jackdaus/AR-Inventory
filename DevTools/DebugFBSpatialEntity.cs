using StereoKit;
using ARInventory;
using StereoKit.Framework;
using System;

class DebugFBSpatialEntity : IStepper
{
	Pose _windowPose  = new Pose(0.6f, 0.1f, -0.2f, Quat.LookDir(-1, 0, 1));
	Guid _selectedAnchorId = Guid.Empty; 

	public bool Enabled => App.SpatialEntity.Available;

	public bool Initialize() { return true; }
	public void Shutdown() { }

	public void Step()
	{
		if (!App.DEBUG_ON)
			return;

		UI.WindowBegin("Spatial Entity Debug", ref _windowPose);

		if (!App.SpatialEntity.Available)
		{
			UI.Label("No FB Spatial Entity EXT available :(");
		}
		else
		{
			UI.Label("FB Spatial Entity EXT available!");
			if (UI.Button("Create Anchor"))
			{
				// Create an anchor at pose of the right index finger tip
				Pose fingerPose = Input.Hand(Handed.Right)[FingerId.Index, JointId.Tip].Pose;
				App.SpatialEntity.CreateAnchor(fingerPose);
			}

			if (UI.Button("Load Anchors"))
				App.SpatialEntity.LoadAllAnchors();

			UI.Label($"Anchor count loaded: {App.SpatialEntity.Anchors.Count}");

			if (UI.Button("Erase All Anchors"))
				App.SpatialEntity.EraseAllAnchors();
		}

		foreach(var key in App.SpatialEntity.Anchors.Keys)
		{
			var anchor = App.SpatialEntity.Anchors[key];

			UI.PanelBegin();
			if (UI.Button($"{key.ToString()}"))
			{
				_selectedAnchorId = key;
			}
			if (_selectedAnchorId == key)
			{
				UI.Label("XrSpace: " + anchor.XrSpace);
				UI.Label("Located: " + anchor.LocateSuccess);
				UI.Label(anchor.Pose.ToString());
			}
			UI.PanelEnd();
		}


		UI.WindowEnd();

		// Spatial anchor visual
		foreach(var anchor in App.SpatialEntity.Anchors.Values)
		{
			Mesh.Cube.Draw(Material.Default, anchor.Pose.ToMatrix(0.01f), new Color(1, 0.5f, 0));
		}

	}
}