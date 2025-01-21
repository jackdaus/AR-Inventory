using AR_Inventory.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AR_Inventory
{
    internal class Db : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public string DbPath { get; }

        public Db()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path   = Environment.GetFolderPath(folder);
            DbPath     = System.IO.Path.Join(path, "inventory.db");
        }

        // The following configures EF to connect to the a SQLite db
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
