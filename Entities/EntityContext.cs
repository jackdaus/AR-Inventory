using ARInventory.Entities.Models;
using ARInventory.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StereoKit;
using Newtonsoft.Json;

namespace ARInventory.Entities
{
    /// <summary>
    /// Access to the persistence layer.
    /// </summary>
    // TODO are there tread saftey concerns here? Do I need to implement a thread safe singelton pattern?
    public class EntityContext
    {
        public readonly MyDbSet<Item> Items         = new MyDbSet<Item>();
        public readonly MyDbSet<ItemType> ItemTypes = new MyDbSet<ItemType>();

        public EntityContext()
        {
            // TODO initialize
            // load from file
        }

        public bool SaveChanges()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Log.Info(json);

            var isSuccessful = Platform.WriteFile($"db-{DateTime.Now.Ticks}.json", json);
            return isSuccessful;
        }
    }
}
