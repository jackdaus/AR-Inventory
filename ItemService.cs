using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void ReloadItems()
        {
            // TODO this might cause issues with object reference since the DTO objects are new...
            // Consider refactoring to use struct based data? And then reference by ID? TBD
            Items = Factory.GetItemDtos().ToList();
        }

    }
}
