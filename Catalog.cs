using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace AR_Inventory
{
    static class Catalog
    {
        internal static class Sprites
        {
            internal static Sprite IconAdd     { get; } = Sprite.FromFile("icons/outline_add_circle_white_24dp.png");
            internal static Sprite IconAnchor  { get; } = Sprite.FromFile("icons/outline_anchor_white_24dp.png");
			internal static Sprite IconDown    { get; } = Sprite.FromFile("icons/outline_arrow_downward_white_24dp.png");
			internal static Sprite IconUp      { get; } = Sprite.FromFile("icons/outline_arrow_upward_white_24dp.png");
            internal static Sprite IconBug     { get; } = Sprite.FromFile("icons/outline_bug_report_white_24dp.png");
            internal static Sprite IconClear   { get; } = Sprite.FromFile("icons/outline_clear_white_24dp.png");
			internal static Sprite IconClose   { get; } = Sprite.FromFile("icons/outline_close_white_24dp.png");
            internal static Sprite IconDelete  { get; } = Sprite.FromFile("icons/outline_delete_white_24dp.png");
            internal static Sprite IconError   { get; } = Sprite.FromFile("icons/outline_error_white_24dp.png");
            internal static Sprite IconInfo    { get; } = Sprite.FromFile("icons/outline_delete_white_24dp.png");
			internal static Sprite IconPower   { get; } = Sprite.FromFile("icons/outline_power_settings_new_white_24dp.png");
			internal static Sprite IconRadar   { get; } = Sprite.FromFile("icons/outline_radar_white_24dp.png");
			internal static Sprite IconSearch  { get; } = Sprite.FromFile("icons/outline_search_white_24dp.png");
			internal static Sprite IconEye     { get; } = Sprite.FromFile("icons/outline_visibility_white_24dp.png");
			internal static Sprite IconWarning { get; } = Sprite.FromFile("icons/outline_warning_white_24dp.png");
		}

        internal static class Models
        {
            // Nothing to see here, yet
        }
    }
}
