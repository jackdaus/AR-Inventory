using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;
using static SpatialEntity.SpatialEntityFBExt;

namespace ARInventory
{
    internal class ItemDto
    {
        public Guid Id;
        public Pose Pose;
        public Anchor SpatialAnchor;
		public string Title;
        public int Quantity;
    }
}
