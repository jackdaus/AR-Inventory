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
                SpatialAnchorUuid = item.SpatialAnchorUuid,
                Pose = new Pose(item.LocationX, item.LocationY, item.LocationZ, new Quat(item.OrientationX, item.OrientationY, item.OrientationZ, item.OrientationW)),
                Title = item.Title,
            })
            .AsQueryable();
        }
    }
}
