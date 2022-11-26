using StereoKit;
using System;

namespace ARInventory
{
    internal class ItemDto
    {
        public Guid Id;
        public Pose Pose;
        public Guid? SpatialAnchorUuid;
        public bool SpatialAnchorCreateFailed;
		public string Title;
        public int Quantity;
    }
}
