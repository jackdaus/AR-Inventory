using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory
{
    internal class ItemDto
    {
        public Guid Id { get; set; }
        
        // This needs to be a field (not a property) so that we can pass it to methods with ref
        public Pose Pose;
        public string Title { get; set; }
        public int Quantity { get; set; }
    }
}
