using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace UnitSpriteStudio.Shading {
	class PhongShader {
		private Selection Area;
		private NormalMap NormalMap;
		private SpriteSheet SpriteSheet;

		// Settings
		private Vector3D p_lightDirection;
		public Vector3D LightDirection {
			get { return p_lightDirection; }
			set { p_lightDirection = value; p_lightDirection.Normalize(); }
		}
		Vector3D p_eyeDirection = new Vector3D(0, 0, 1);
		public Vector3D EyeDirection {
			get { return p_eyeDirection; }
			set { p_eyeDirection = value; p_eyeDirection.Normalize(); }
		}
		public float DiffuseReflection = 0.7f;
		public float SpecularReflection = 0.25f;
		public double Shininess = 2;
		public float ShadeRange = 16;
		public float AmbientDarkness = 0;

		public PhongShader(Selection area, NormalMap normalMap, SpriteSheet spriteSheet) {
			Area = area;
			NormalMap = normalMap;
			SpriteSheet = spriteSheet;
			LightDirection = new Vector3D(-1, -1, 3);
		}


		internal void Shade(DrawingRoutines.DrawingRoutine.LayerFrameInfo frameInfo) {
			byte[] pixels = SpriteSheet.frameSource.GetFramePixelData(frameInfo.Index);
			(int Width, int Height) frameSize = SpriteSheet.drawingRoutine.FrameImageSize();
			for (int x = 0; x < Area.SizeX; x++) {
				for (int y = 0; y < Area.SizeY; y++) {
					if (Area.GetPoint(x, y)) {
						int FrameX = x - frameInfo.OffsetX;
						int FrameY = y - frameInfo.OffsetY;
						if (FrameX < 0 || FrameY < 0 || FrameX >= frameSize.Width || FrameY >= frameSize.Height) continue;

						int brightnessShift;
						//brightnessShift = (int)Math.Round(furthestEdge - distanceMap[x, y]); // Linear step 1
						//brightnessShift = (int)Math.Round((1-distanceMap[x, y])*10); // Linear Normalized to 10
						//brightnessShift = (int)Math.Round(((1 - distanceMap[x, y]) * (1 - distanceMap[x, y])) * 10); // Squared normalized to 10
						//brightnessShift = (int)Math.Round(Math.Cos(distanceMap[x, y] * (Math.PI / 2)) * furthestEdge);
						//brightnessShift = (int)Math.Round(Math.Sqrt((1-distanceMap[x, y])) * furthestEdge);
						//brightnessShift = (int)Math.Round(((1 - distanceMap[x, y]) * (1 - distanceMap[x, y])) * furthestEdge); // Squared normalized to furthestEdge
						float diffuse = (float)Vector3D.DotProduct(LightDirection, NormalMap.Map[x, y]) * DiffuseReflection;
						if (diffuse < 0) {
							brightnessShift = 16;
						} else {
							Vector3D reflected = Vector3D.Subtract(Vector3D.Multiply(NormalMap.Map[x, y], Vector3D.DotProduct(LightDirection, NormalMap.Map[x, y]) * 2), LightDirection);
							reflected.Normalize();
							float specular = (float)Math.Pow(Vector3D.DotProduct(reflected, EyeDirection), Shininess) * SpecularReflection;
							if (specular < 0) specular = 0;
							brightnessShift = (int)Math.Round((1 - ((diffuse + specular) / (DiffuseReflection + SpecularReflection))) * ShadeRange + AmbientDarkness);
						}
						brightnessShift = Math.Max(0, Math.Min(16, brightnessShift));

						int newColor = pixels[FrameX + FrameY * frameSize.Width];
						if (newColor == 0) continue;
						newColor = Math.Max(1, (newColor / 16) * 16); // Brightest color in the group
						newColor = Math.Min(newColor + brightnessShift, ((newColor / 16) + 1) * 16 - 1);

						pixels[FrameX + FrameY * frameSize.Width] = (byte)newColor;
					}
				}
			}
			SpriteSheet.frameSource.SetFramePixelData(frameInfo.Index, pixels);
		}
	}
}
