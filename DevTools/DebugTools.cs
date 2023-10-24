using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace AR_Inventory
{
    static class DebugTools
    {
        public static void DisplayPosition(Pose pose)
        {
            Vec3 textPosition = pose.position;
            Quat textOrientation = Quat.LookAt(textPosition, Input.Head.position);
            Text.Add($"({pose.position.x.ToString("0.00")}, {pose.position.y.ToString("0.00")}, {pose.position.z.ToString("0.00")})", Matrix.TR(textPosition, textOrientation));
        }

        public static void DisplayOrientation(Quat orientation)
        {
            Vec3 textPosition = Input.Head.position + (Input.Head.Forward * 0.3f) + (Input.Head.Up * -0.05f);
            Quat textOrientation = Quat.LookAt(textPosition, Input.Head.position);
            Text.Add("O.x: " + orientation.x.ToString("0.00"), Matrix.TR(textPosition, textOrientation));
            Text.Add("O.y: " + orientation.y.ToString("0.00"), Matrix.TR(textPosition + (Input.Head.Up * -0.03f), textOrientation));
            Text.Add("O.z: " + orientation.z.ToString("0.00"), Matrix.TR(textPosition + (Input.Head.Up * -0.06f), textOrientation));
        }
    }
}
