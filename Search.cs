using SpatialEntity;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Linq;
using static SpatialEntity.SpatialEntityFBExt;

namespace ARInventory
{
    public class Search : IStepper
    {
        private Pose   _menuPose    = new Pose(0, 0.2f, -0.4f, Quat.LookDir(0, 0, 1));
        private Vec2   _inputSize   = new Vec2(15 * U.cm, 3 * U.cm);
        private String _searchInput = String.Empty;

        public bool Enabled { get; set; }

        public bool Initialize()
        {
            return true;
        }

        public void Shutdown()
        {
            
        }

        public void Step()
        {
            UI.WindowBegin("search-window", ref _menuPose);
            UI.Input("search-input", ref _searchInput, _inputSize);
            UI.SameLine();
            if (UI.ButtonRound("clear-search-input", Catalog.Sprites.IconClear))
            {
                _searchInput = String.Empty;
                App.ItemService.SearchedItem = null;
                App.ItemService.FocusedItem  = null;
            }
            UI.HSeparator();
            App.ItemService.Items
                .Where(item => item.Title.ToLower().Contains(_searchInput.ToLower()))
                .ToList()
                .ForEach(item =>
                {
                    if (UI.Button(item.Title))
                    {
                        Log.Info("Selected " + item.Title);
                        App.ItemService.SearchedItem = item;
                    }
                });
            UI.WindowEnd();

            if (App.ItemService.SearchedItem != null)
            {
                // Draw a path from the user's hand to the item, one dimension at a time
                Matrix itemPoseMatrix = App.ItemService.SearchedItem.Pose.ToMatrix();
                Anchor anchor         = App.ItemService.TryGetSpatialAnchor(App.ItemService.SearchedItem);

                if (anchor != null)
                    itemPoseMatrix = itemPoseMatrix * anchor.Pose.ToMatrix();

                Vec3 p0 = Input.Hand(Handed.Right).wrist.position;
				Vec3 p1 = new Vec3(p0.x, p0.y, itemPoseMatrix.Translation.z);
				Vec3 p2 = new Vec3(itemPoseMatrix.Translation.x, p0.y, itemPoseMatrix.Translation.z);
				Vec3 p3 = itemPoseMatrix.Translation;

				Lines.Add(p0, p1, new Color(1, 0, 0), 0.01f);
				Lines.Add(p1, p2, new Color(1, 0, 0), 0.01f);
				Lines.Add(p2, p3, new Color(1, 0, 0), 0.01f);
			}
        }

        public void TeleportMenu(Pose pose)
        {
            _menuPose = pose;
        }
	}
}
