using ARInventory.Entities.Models;
using StereoKit;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ARInventory.Entities
{
    /// <summary>
    /// Access to the persistence layer.
    /// </summary>
    public class EntityContext
    {
        public EntitySet<Item> Items => _entities.Items;
        public EntitySet<ItemType> ItemTypes => _entities.ItemTypes;

        private readonly EntityBacking _entities;
        private readonly string FILE_NAME = "db.json";

        public EntityContext()
        {
            try
            {
                var filename = getPlatformFilename();
				var json     = Platform.ReadFileText(filename);
                _entities    = JsonConvert.DeserializeObject<EntityBacking>(json) ?? new EntityBacking();
            }
            catch(Exception e)
            {
                Log.Err($"[AR Inventory] Unable to load entities from {FILE_NAME}!");
                _entities = new EntityBacking();
            }
        }

        /// <summary>
        /// Save the context to the file system
        /// </summary>
        /// <returns>True on success, False on failure</returns>
        public bool SaveChanges()
        {
			var json         = JsonConvert.SerializeObject(_entities, Formatting.Indented);
			var filename     = getPlatformFilename();
            var isSuccessful = Platform.WriteFile(filename, json);
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

		/// <summary>
		/// For Android, we need to take care to read/write from a part of the file system where our app has
		/// 'permission' to access. See https://learn.microsoft.com/en-us/xamarin/android/platform/files/ 
		/// </summary>
		/// <returns></returns>
		private string getPlatformFilename()
        {
			string fileName = FILE_NAME;
			if (App.IsAndroid)
			{
				string specialPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				fileName           = Path.Combine(specialPath, FILE_NAME);
			}

            return fileName;
		}
    }
}
