using StereoKit;
using System.Linq;

namespace AR_Inventory
{
    internal static class Factory
    {
        public static IQueryable<ItemDto> GetItemDtos()
        {
            return App.Db.Items.Select(item => new ItemDto
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
