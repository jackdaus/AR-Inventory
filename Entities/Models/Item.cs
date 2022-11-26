using ARInventory.Entities.Interfaces;
using System;
using System.Numerics;

namespace ARInventory.Entities.Models
{
    public class Item : IGotId
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
        public int Quantity { get; set; }
    }
}
