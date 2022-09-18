using ARInventory.Entities;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory
{
    public class DebugWindow : IStepper
    {
        private Pose menuPose = new Pose(0.4f, 0, -0.4f, Quat.LookDir(-1, 0, 1));

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
            UI.WindowBegin("Test Window", ref menuPose);
            if (UI.Button("Test file"))
            {
                Log.Info($"Begin testing file! {DateTime.Now}");
                var isSuccessful = Platform.WriteFile($"my-new-file-{DateTime.Now.Ticks}.txt", $"this is the content {DateTime.Now}");
                Log.Info($"Result of flile write: {isSuccessful}");
            }
            if (UI.Button("Pick file"))
            {
                Platform.FilePicker(PickerMode.Open, logFileContents, null);
            }
            if (UI.Button("Test Context"))
            {
                testContext();
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
                //Location = Vec3.Zero,
                Title = "Towels",
                Quantity = 1
            });

            App.Context.Items.Add(new Entities.Models.Item
            {
                Id = Guid.NewGuid(),
                //Location = null,
                Title = "Salt",
                Quantity = 1
            });

            App.Context.SaveChanges();
        }
    }
}
