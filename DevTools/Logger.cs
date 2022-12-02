using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StereoKit;
using StereoKit.Framework;

namespace ARInventory.DevTools
{
    public class Logger : IStepper
    {
        public bool Enabled { get; set; }

        private Pose windowPose = new Pose(-0.4f, 0, -0.4f, Quat.LookDir(1, 0, 1));
        private Vec2 windowSize = new Vec2(0.3f);
        private List<string> logList = new List<string>();
        private string logText;

        public bool Initialize()
        {
            Log.Subscribe(onLog);
            return true;
        }

        public void Shutdown()
        {
        }

        public void Step()
        {
            if (!App.DEBUG_ON)
                return;

            // Display logs in XR window
            UI.WindowBegin("Log!", ref windowPose, windowSize);
            UI.Text(logText);
            UI.WindowEnd();
        }

        private void onLog(LogLevel level, string text)
        {
            logList.Add(text);

            logText = string.Join("", logList.Reverse<string>().Take(6).Reverse());
        }
    }
}
