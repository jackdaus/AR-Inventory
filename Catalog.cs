using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory
{
    static class Catalog
    {
        internal static class Sprites
        {
            internal static Sprite IconDown   { get; } = Sprite.FromFile("icons/outline_arrow_downward_white_24dp.png");
            internal static Sprite IconUp     { get; } = Sprite.FromFile("icons/outline_arrow_upward_white_24dp.png");
            internal static Sprite IconClose  { get; } = Sprite.FromFile("icons/outline_close_white_24dp.png");
            internal static Sprite IconDelete { get; } = Sprite.FromFile("icons/outline_delete_white_24dp.png");
            internal static Sprite IconPower  { get; } = Sprite.FromFile("icons/outline_power_settings_new_white_24dp.png");
            internal static Sprite IconEye    { get; } = Sprite.FromFile("icons/outline_visibility_white_24dp.png");
        }

        internal static class Models
        {
            // Nothing to see here, yet
        }
    }
}
