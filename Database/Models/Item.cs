using System;

namespace AR_Inventory.Database.Models
{
    public class Item
    {
        public Guid Id { get; set; }
        public Guid? SpatialAnchorUuid { get; set; }
		public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }
        public float OrientationX { get; set; }
        public float OrientationY { get; set; }
        public float OrientationZ { get; set; }
        public float OrientationW { get; set; }
        public string Title { get; set; }
    }
}
