using ARInventory.Entities.Interfaces;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory.Entities.Models
{
    public class Item : IGotId
    {
        public Guid Id { get; set; }
        //public Vec3 Location { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public bool IsArchived { get; set; }
    }
}
