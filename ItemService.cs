using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SpatialEntity.SpatialEntityFBExt;

namespace ARInventory
{
    internal class ItemService
    {
        public List<ItemDto> Items = new List<ItemDto>();
        public ItemDto SearchedItem;
        public ItemDto FocusedItem;

        public ItemService()
        {
            Items = Factory.GetItemDtos().ToList();
        }

        public void ReloadItemsFromStorage()
        {
            // TODO this might cause issues with object reference since the DTO objects are new...
            // Consider refactoring to use struct based data? And then reference by ID? TBD
            Items = Factory.GetItemDtos().ToList();
        }

		public Anchor TryGetSpatialAnchor(ItemDto item)
		{
			Anchor anchor = null;
			if (item.SpatialAnchorUuid != null)
				App.SpatialEntity.Anchors.TryGetValue(item.SpatialAnchorUuid.Value, out anchor);

			return anchor;
		}
	}
}
