using StereoKit;
using ARInventory;
using StereoKit.Framework;

class DebugFBSpatialEntity : IStepper
{
	Pose   windowPose  = new Pose(0.4f, 0.2f, -0.4f, Quat.LookDir(-1, 0, 1));

	public bool Enabled => App.SpatialEntity.Available;

	public bool Initialize() { return true; }
	public void Shutdown() { }

	public void Step()
	{
		UI.WindowBegin("Spatial Entity Debug", ref windowPose);

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

		UI.WindowEnd();

		// Spatial anchor visual
		foreach(var anchor in App.SpatialEntity.Anchors.Values)
		{
			Mesh.Cube.Draw(Material.Default, anchor.Pose.ToMatrix(0.01f), new Color(1, 0.5f, 0));
		}
	}
}