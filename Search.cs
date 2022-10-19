using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory
{
    public class Search : IStepper
    {
        private Pose _menuPose = new Pose(0, 0.2f, -0.4f, Quat.LookDir(0, 0, 1));
        private String _searchInput = String.Empty;
        private Vec2 _inputSize = new Vec2(15 * U.cm, 3 * U.cm);

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
                _searchInput = String.Empty;
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
        }
    }
}
