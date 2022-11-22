using StereoKit.Framework;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using static ARInventory.Catalog;

namespace ARInventory
{
    // TODO
    // 1. Fade map visible with angle OR Activte / Deactivate Minimap
    // 2. Circular Map
    // 3. Render Layers
    // 4. Add some sounds!
    class Minimap : IStepper
    {
        public bool Enabled { get; set; }

        Tex      _renderTex;
        Material _minimapMaterial;
        Mesh     _minimapMesh;
        Material _gridLineMaterial;
        Material _backgroundMaterial;
        Matrix   _floorTransform = Matrix.TS(new Vec3(0, -1.5f, 0), new Vec3(30, 0.1f, 30));
        int      _width  = 500;
        int      _height = 500;

        public bool Initialize()
        {
            _renderTex = new Tex(TexType.Rendertarget, TexFormat.Rgba32);
            _renderTex.SetSize(_width, _height);
            _renderTex.AddZBuffer(TexFormat.Depth32);

            _minimapMaterial = Default.MaterialUnlit.Copy();
            _minimapMaterial[MatParamName.DiffuseTex] = _renderTex;
            _minimapMaterial.FaceCull = Cull.None;

            _minimapMesh = Mesh.GeneratePlane(Vec2.One * 10 * U.cm, Vec3.UnitZ, Vec3.UnitY);

            _gridLineMaterial = new Material(Shader.FromFile("floor.hlsl"));
            _gridLineMaterial.Transparency = Transparency.Blend;

            _backgroundMaterial = Default.MaterialUnlit.Copy();
            _backgroundMaterial[MatParamName.DiffuseTex] = Tex.Flat;

            return true;
        }

        public void Shutdown()
        {
            
        }

        public void Step()
        {
            Pose rHandPalmPose = Input.Hand(Handed.Right).palm;

            // Minimap located 5 cm above user's hand
            Vec3 minimapPosition = rHandPalmPose.position;
            minimapPosition += rHandPalmPose.Forward * -5 * U.cm;

            // Oriented in same direction as user's palm. The "Up" direction should always point along the Z-Axis 
            // to make sure the orientation matches with the world.
            Quat minimapOrientation = Quat.LookAt(minimapPosition, rHandPalmPose.position, -Vec3.UnitZ);
            Matrix minimapTransform = Matrix.TR(minimapPosition, minimapOrientation);

            Hierarchy.Push(minimapTransform);
            _minimapMesh.Draw(_minimapMaterial, Matrix.Identity);
            Hierarchy.Pop();

            // Camera is located 3m above user, looking down
            Quat cameraOrientation = Quat.LookAt(Vec3.Zero, new Vec3(0, -1, 0));
            Vec3 cameraLocation    = Input.Head.position + Vec3.UnitY * 3 * U.m;
            Matrix camera          = Matrix.TR(cameraLocation, cameraOrientation);

            // TODO draw a grid on the minimap without drawing it in user's virtual world... maybe use Blit?
            // Draw background color and grid lines
            //Default.MeshCube.Draw(_backgroundMaterial, _floorTransform * Matrix.T(-Vec3.UnitY), Color.White, RenderLayer.Layer1);
            //Default.MeshCube.Draw(_gridLineMaterial,   _floorTransform,                         Color.White, RenderLayer.Layer1);

            // Orthographic projection of a 3m x 3m sqaure area
            Renderer.RenderTo(_renderTex,
                camera,
                Matrix.Orthographic(3 * U.m, 3 * U.m, 0.01f, 100),
                layerFilter: RenderLayer.Layer1);
        }
    }
}
