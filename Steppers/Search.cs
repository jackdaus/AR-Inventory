using StereoKit;
using StereoKit.Framework;
using System;
using System.Linq;
using static StereoKitFBSpatialEntity.SpatialEntityFBExt;

namespace AR_Inventory.Steppers
{
    public class Search : IStepper
    {
        private Pose _menuPose = new Pose(0, 0.2f, -0.4f, Quat.LookDir(0, 0, 1));
        private Vec2 _inputSize = new Vec2(15 * U.cm, 3 * U.cm);
        private string _searchInput = string.Empty;

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
                _searchInput = string.Empty;
                App.ItemService.SearchedItem = null;
                App.ItemService.FocusedItem = null;
            }
            UI.HSeparator();
            App.ItemService.Items
                .Where(item => item.Title.ToLower().Contains(_searchInput.ToLower()))
                .ToList()
                .ForEach(item =>
                {
                    UI.PushId(item.Id.ToString());
                    if (UI.Button(item.Title))
                    {
                        Log.Info("Selected " + item.Title);
                        App.ItemService.SearchedItem = item;
                    }
                    UI.PopId();
                });
            UI.WindowEnd();

            if (App.ItemService.SearchedItem != null)
            {
                // Draw a path from the user's hand to the item, one dimension at a time
                Matrix itemPoseMatrix = App.ItemService.SearchedItem.Pose.ToMatrix();
                StereoKitFBSpatialEntity.SpatialEntityFBExt.Anchor? anchor = App.ItemService.TryGetSpatialAnchor(App.ItemService.SearchedItem);
                
                if (anchor != null)
                    itemPoseMatrix = itemPoseMatrix * anchor.Value.Pose.ToMatrix();

                Vec3 p0 = Input.Hand(Handed.Right).wrist.position;
                Vec3 p1 = new Vec3(p0.x, p0.y, itemPoseMatrix.Translation.z);
                Vec3 p2 = new Vec3(itemPoseMatrix.Translation.x, p0.y, itemPoseMatrix.Translation.z);
                Vec3 p3 = itemPoseMatrix.Translation;

                Color color = new Color(0, 1, 1);
                Lines.Add(p0, p1, color, 0.005f);
                Lines.Add(p1, p2, color, 0.005f);
                Lines.Add(p2, p3, color, 0.005f);
            }
        }

        public void TeleportMenu(Pose pose)
        {
            _menuPose = pose;
        }
    }
}
