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
        public EntitySet<Item> Items => _entities.Items;
        public EntitySet<ItemType> ItemTypes => _entities.ItemTypes;

        private readonly EntityBacking _entities;
        private readonly string FILE_NAME = "db.json";

        public EntityContext()
        {
            var json = Platform.ReadFileText(FILE_NAME);
            _entities = JsonConvert.DeserializeObject<EntityBacking>(json) ?? new EntityBacking();
        }

        /// <summary>
        /// Save the context to the file system
        /// </summary>
        /// <returns>True on success, False on failure</returns>
        public bool SaveChanges()
        {
            var json = JsonConvert.SerializeObject(_entities, Formatting.Indented);
            var isSuccessful = Platform.WriteFile(FILE_NAME, json);
            return isSuccessful;
        }

        /// <summary>
        /// A private backing class so we don't have to serialize/deserialize the EntityContext directly.
        /// That would lead to messy JSON serialization issues with the default constructor.
        /// </summary>
        private class EntityBacking
        {
            public readonly EntitySet<Item> Items         = new EntitySet<Item>();
            public readonly EntitySet<ItemType> ItemTypes = new EntitySet<ItemType>();
        }
    }

}
