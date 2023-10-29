using AR_Inventory.Database.Models;
using System;

namespace AR_Inventory
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

            App.Db.Items.Add(item);
            App.Db.SaveChanges();
        }

        public static void UpdateItem(ItemDto itemDto)
        {
            var item = App.Db.Items.Find(itemDto.Id);
            item.SpatialAnchorUuid = itemDto.SpatialAnchorUuid;
            item.LocationX = itemDto.Pose.position.x;
			item.LocationY = itemDto.Pose.position.y;
            item.LocationZ = itemDto.Pose.position.z;
            item.OrientationX = itemDto.Pose.orientation.x;
            item.OrientationY = itemDto.Pose.orientation.y;
            item.OrientationZ = itemDto.Pose.orientation.z;
            item.OrientationW = itemDto.Pose.orientation.w;
            item.Title = itemDto.Title;

            App.Db.Items.Update(item);
            App.Db.SaveChanges();
        }

        public static void DeleteItem(Guid itemId)
        {
            var item = App.Db.Items.Find(itemId);

            // Delete anchor if it exists.
            // TODO consider the edge case where we want to delete and Item when the Anchor has not been loaded.
            if (item.SpatialAnchorUuid != null) 
                App.SpatialEntity.DeleteAnchor(item.SpatialAnchorUuid.Value);

            App.Db.Items.Remove(item);
			App.Db.SaveChanges();
		}
	}
}
