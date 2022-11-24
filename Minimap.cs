using StereoKit.Framework;
using StereoKit;

namespace ARInventory
{
    // TODO
    // 1. Fade map visible with angle OR Activte / Deactivate Minimap
    // 2. Render Layers
    // 3. Add some sounds!
    class Minimap : IStepper
    {
        public bool Enabled { get; set; }

        Tex      _renderTex;
        Mesh     _minimapMesh;
        Material _minimapMaterial;
        Material _backgroundMaterial;

        public bool Initialize()
        {
            _renderTex = new Tex(TexType.Rendertarget);
            _renderTex.SetSize(500, 500);
            _renderTex.AddZBuffer(TexFormat.Depth32);

            _minimapMaterial = Default.MaterialUnlit.Copy();
            _minimapMaterial[MatParamName.DiffuseTex] = _renderTex;
            _minimapMaterial.FaceCull = Cull.None;

			_minimapMesh = Mesh.GenerateCircle(10 * U.cm, Vec3.UnitZ, Vec3.UnitY, 50);

            // TODO v0.3.7 use built-in blit shader
			_backgroundMaterial = new Material(Shader.Find("default/shader_blit"));
			_backgroundMaterial["source"] = Tex.Gray;

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

            // Draw a background color
            Renderer.Blit(_renderTex, _backgroundMaterial);

			// Orthographic projection of a 3m x 3m sqaure area Don't clear color
			// buffer, or else our background color will get overwritten with black!
			Renderer.RenderTo(_renderTex,
                camera,
                Matrix.Orthographic(3 * U.m, 3 * U.m, 0.01f, 100),
                layerFilter: RenderLayer.Layer1, 
                RenderClear.Depth);
        }
    }
}
