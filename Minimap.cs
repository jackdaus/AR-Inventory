using StereoKit.Framework;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARInventory
{
	internal class Minimap : IStepper
	{
		public bool Enabled { get; set; }
		private Tex _renderTex;
		private Material _minimapMaterial;
		private Mesh _minimapMesh;

		private int _width  = 500;
		private int _height = 500;

		public bool Initialize()
		{
			_renderTex = new Tex(TexType.Rendertarget, TexFormat.Rgba32);
			_renderTex.SetSize(_width, _height);
			_renderTex.AddZBuffer(TexFormat.Depth32);

			_minimapMaterial = Default.MaterialUnlit.Copy();
			_minimapMaterial[MatParamName.DiffuseTex] = _renderTex;
			_minimapMaterial.FaceCull = Cull.None;

			_minimapMesh = Mesh.GeneratePlane(Vec2.One * 2 * U.cm, Vec3.UnitZ, Vec3.UnitY);

            return true;
		}

		public void Shutdown()
		{
			
		}

		public void Step()
		{
			//Vec3 minimapPosition = Input.Head.position + Input.Head.Forward * 0.5f;
			//Quat minimapOrientation = Quat.LookAt(minimapPosition, Input.Head.position);
			//Default.MeshQuad.Draw(_minimapMaterial, Matrix.TR(minimapPosition, minimapOrientation));

			Hierarchy.Push(Input.Head.ToMatrix());
			//Default.MeshQuad.Draw(_minimapMaterial, Matrix.TR(1, -0.4f, -1, -Vec3.UnitY * 2));
			//_minimapMesh.Draw(_minimapMaterial, Matrix.TR(1, -0.4f, -1, -Vec3.UnitY * 2));
			_minimapMesh.Draw(_minimapMaterial, Matrix.T(0.03f, -0.01f, -0.025f));
			
            Hierarchy.Pop();

            // Camera is located 3m above user and the Yaw has same orientation as the user's head
            Quat cameraOrientation = Quat.LookAt(Vec3.Zero, new Vec3(0, -1, 0), Input.Head.Forward);
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

		private void debugDisplay()
		{
			Vec3 textPosition = Input.Head.position + (Input.Head.Forward * 0.3f) + (Input.Head.Up * -0.15f);
			Quat textOrientation = Quat.LookAt(textPosition, Input.Head.position);
			Text.Add("O.x: " + Input.Head.orientation.x.ToString("0.000"), Matrix.TR(textPosition, textOrientation));
			Text.Add("O.y: " + Input.Head.orientation.y.ToString("0.000"), Matrix.TR(textPosition + (Input.Head.Up * -0.03f), textOrientation));
			Text.Add("O.z: " + Input.Head.orientation.z.ToString("0.000"), Matrix.TR(textPosition + (Input.Head.Up * -0.06f), textOrientation));
		}
	}
}
