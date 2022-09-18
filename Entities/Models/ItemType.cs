using ARInventory.Entities.Interfaces;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory.Entities.Models
{
    public class ItemType : IGotId
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Vec3 Color { get; set; }
        public Model DefaultModel { get; set; }
    }
}
