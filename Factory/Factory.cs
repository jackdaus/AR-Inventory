using ARInventory.Entities.Models;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory
{
    internal static class Factory
    {
        public static IQueryable<ItemDto> GetItemDtos()
        {
            return App.Context.Items.Select(item => new ItemDto
            {
                Id = item.Id,
                Pose = new Pose(item.LocationX, item.LocationY, item.LocationZ, new Quat(item.OrientationX, item.OrientationY, item.OrientationZ, item.OrientationW)),
                Title = item.Title,
                Quantity = item.Quantity,
            })
            .AsQueryable();
        }

        public static void AddItem(ItemDto itemDto)
        {
            Item item = new Item();
            item.Id = itemDto.Id;
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

        public static void UpdateItemPose(ItemDto itemDto)
        {
            var item = App.Context.Items.Find(itemDto.Id);
            item.LocationX = itemDto.Pose.position.x;
            item.LocationY = itemDto.Pose.position.y;
            item.LocationZ = itemDto.Pose.position.z;
            item.OrientationX = itemDto.Pose.orientation.x;
            item.OrientationY = itemDto.Pose.orientation.y;
            item.OrientationZ = itemDto.Pose.orientation.z;
            item.OrientationW = itemDto.Pose.orientation.w;

            App.Context.Items.Update(item);
            App.Context.SaveChanges();
        }

        public static void UpdateItemDto(ItemDto itemDto)
        {
            var item = App.Context.Items.Find(itemDto.Id);
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
    }
}
