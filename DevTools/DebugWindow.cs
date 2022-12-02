using ARInventory.Entities;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARInventory
{
    public class DebugWindow : IStepper
    {
        private Pose _menuPose = new Pose(0.4f, -0.1f, -0.4f, Quat.LookDir(-1, 0, 1));

        public bool Enabled { get; set; }

        public bool Initialize()
        {
            return true;
        }

        public void Shutdown()
        {
        }

        public void Step()
        {
            if (!App.DEBUG_ON)
                return;

            UI.WindowBegin("Test Window", ref _menuPose);
            if (UI.Button("Test write file"))
            {
                var appDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filename = Path.Combine(appDocsPath, "my-file.txt");

                Log.Info($"Begin writing test file! {DateTime.Now}");
                var isSuccessful = Platform.WriteFile(filename, $"this is the content {DateTime.Now}");
                Log.Info($"Result of flile write: {isSuccessful}");
            }
			if (UI.Button("Test read file"))
			{
				var temp = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				Log.Info($"Begin reading test file! {DateTime.Now}");
				var isSuccessful = Platform.ReadFile($"{temp}/my-file.txt", out string data);
				Log.Info($"Result of flile read: {isSuccessful}");
                Log.Info($"data: {data}");
			}
			if (UI.Button("Pick file"))
            {
                Platform.FilePicker(PickerMode.Open, logFileContents, null);
            }

            UI.Label("Context");
            if (UI.Button("Test: Add Item to Context"))
            {
                testContext();
            }
			if (UI.Button("Test: Load all items from db"))
			{
				App.ItemService.ReloadItemsFromStorage();
			}
			if (UI.Button("Test: Clean db"))
			{
                var allItems = App.Context.Items.ToList();

                allItems.ForEach(item => App.Context.Items.Remove(item));
                App.Context.SaveChanges();
                App.ItemService.ReloadItemsFromStorage();
			}

			//if (UI.Button("Quit")) SK.Quit();
			UI.WindowEnd();
        }

        private void logFileContents(string file)
        {
            Platform.ReadFile(file, out string text);
            Log.Info(file);
            Log.Info(text);
        }

        private void testContext()
        {
            App.Context.Items.Add(new Entities.Models.Item
            {
                Id = Guid.NewGuid(),
                LocationX = -(float)new Random().NextDouble(), 
                LocationY = 0, 
                LocationZ = -1,
                OrientationX = 0, 
                OrientationY = 0, 
                OrientationZ = 0,
                OrientationW = 1,
                Title = "Context test",
            });

            var isSuccessful = App.Context.SaveChanges();
            Log.Info($"Save was successful: {isSuccessful}");
        }

    }
}
