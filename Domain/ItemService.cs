﻿using System.Collections.Generic;
using System.Linq;
using static StereoKitFBSpatialEntity.SpatialEntityFBExt;

namespace AR_Inventory.Domain
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

        public Anchor? TryGetSpatialAnchor(ItemDto item)
        {
            Anchor? anchor = null;
            if (item.SpatialAnchorUuid != null)
                anchor = App.SpatialEntity.TryGetSpatialAnchor(item.SpatialAnchorUuid.Value);

            return anchor;
        }
    }
}
