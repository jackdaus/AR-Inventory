using ARInventory.Entities.Models;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory
{
    /// <summary>
    /// Methods to map the Entity (aka Data) layer to/from the Domain layer
    /// </summary>
    internal static class Controller
    {
        public static void AddItem(ItemDto itemDto)
        {
            Item item = new Item();
            item.Id = itemDto.Id;
            item.SpatialAnchorUuid = itemDto.SpatialAnchorUuid;
            item.LocationX = itemDto.Pose.position.x;
            item.LocationY = itemDto.Pose.position.y;
            item.LocationZ = itemDto.Pose.position.z;
            item.OrientationX = itemDto.Pose.orientation.x;
            item.OrientationY = itemDto.Pose.orientation.y;
            item.OrientationZ = itemDto.Pose.orientation.z;
            item.OrientationW = itemDto.Pose.orientation.w;
            item.Title = itemDto.Title;
            item.Quantity = item.Quantity;

            App.Context.Items.Add(item);
            App.Context.SaveChanges();
        }

        public static void UpdateItem(ItemDto itemDto)
        {
            var item = App.Context.Items.Find(itemDto.Id);
            item.SpatialAnchorUuid = itemDto.SpatialAnchorUuid;
            item.LocationX = itemDto.Pose.position.x;
			item.LocationY = itemDto.Pose.position.y;
            item.LocationZ = itemDto.Pose.position.z;
            item.OrientationX = itemDto.Pose.orientation.x;
            item.OrientationY = itemDto.Pose.orientation.y;
            item.OrientationZ = itemDto.Pose.orientation.z;
            item.OrientationW = itemDto.Pose.orientation.w;
            item.Title = itemDto.Title;
            item.Quantity = item.Quantity;

            App.Context.Items.Update(item);
            App.Context.SaveChanges();
        }

        public static void DeleteItem(Guid itemId)
        {
            var item = App.Context.Items.Find(itemId);
            //TODO erase anchor?
            App.Context.Items.Remove(item);
			App.Context.SaveChanges();
		}
	}
}
