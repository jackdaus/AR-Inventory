using StereoKit.Framework;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace ARInventory
{
    class Minimap : IStepper
    {
        public bool Enabled { get; set; }
        Tex _renderTex;
        Material _minimapMaterial;
        Mesh     _minimapMesh;

        int _width  = 500;
        int _height = 500;

        public bool Initialize()
        {
            _renderTex = new Tex(TexType.Rendertarget, TexFormat.Rgba32);
            _renderTex.SetSize(_width, _height);
            _renderTex.AddZBuffer(TexFormat.Depth32);

            _minimapMaterial = Default.MaterialUnlit.Copy();
            _minimapMaterial[MatParamName.DiffuseTex] = _renderTex;
            _minimapMaterial.FaceCull = Cull.None;

            _minimapMesh = Mesh.GeneratePlane(Vec2.One * 10 * U.cm, Vec3.UnitZ, Vec3.UnitY);

            return true;
        }

        public void Shutdown()
        {
            
        }

        public void Step()
        {
            Pose rHandPalmPose = Input.Hand(Handed.Right).palm;
            DebugTools.DisplayPosition(rHandPalmPose);

            // Minimap located 5 cm above user's hand
            Vec3 minimapPosition = rHandPalmPose.position;
            minimapPosition += rHandPalmPose.Forward * -5 * U.cm;

            // Oriented in same direction as user's palm. The "Up" direction should always point along the Z-Axis 
            // to make sure the orientation matches with the world.
            Quat minimapOrientation = Quat.LookAt(minimapPosition, rHandPalmPose.position, -Vec3.UnitZ);
            Matrix minimapTransform = Matrix.TR(minimapPosition, minimapOrientation);

            Hierarchy.Push(minimapTransform);
            Lines.AddAxis(new Pose());
            _minimapMesh.Draw(_minimapMaterial, Matrix.Identity); 
            Hierarchy.Pop();

            // Camera is located 3m above user, looking down
            Quat cameraOrientation = Quat.LookAt(Vec3.Zero, new Vec3(0, -1, 0));
            Vec3 cameraLocation    = Input.Head.position + Vec3.UnitY * 3 * U.m;
            Matrix camera          = Matrix.TR(cameraLocation, cameraOrientation);

            // TODO orthographic projection would be better, but can't seem to get it to work
            //Renderer.RenderTo(_rendersDTex, Matrix.Identity, Matrix.Orthographic(_width, _height, 0.01f, 100));
            //Renderer.RenderTo(_renderTex, 
            //    camera, 
            //    Matrix.Orthographic(_width, _height, 0.01f, 100));

            Renderer.RenderTo(_renderTex,
                camera,
                Matrix.Perspective(70, (float)_width / _height, 0.01f, 100));
        }
    }
}
