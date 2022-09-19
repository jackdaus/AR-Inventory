using ARInventory.Entities.Models;
using ARInventory.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StereoKit;
using Newtonsoft.Json;
using System.IO;

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

        private readonly string FILE_NAME = "db.json";

        public EntityContext()
        {
            var json = Platform.ReadFileText(FILE_NAME);
            var entityContext = JsonConvert.DeserializeObject<EntityContext>(json) ?? new EntityContext(true);
            this.Items = entityContext.Items;
            this.ItemTypes = entityContext.ItemTypes;
        }

        // TODO redesign so we don't need to do this...
        [JsonConstructor]
        private EntityContext(bool dummy)
        {
        }

        public bool SaveChanges()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Log.Info(json);

            //File.WriteAllText($"db-{ticks}-system.txt", json);
            //return true;

            var isSuccessful = Platform.WriteFile(FILE_NAME, json);
            return isSuccessful;
        }
    }
}
