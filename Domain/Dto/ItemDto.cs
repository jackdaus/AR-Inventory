using StereoKit;
using System;

namespace AR_Inventory
{
    internal class ItemDto
    {
        public Guid Id;
        public Pose Pose;
        public Guid? SpatialAnchorUuid;
        public bool SpatialAnchorCreateFailed;
		public string Title;
    }
}
