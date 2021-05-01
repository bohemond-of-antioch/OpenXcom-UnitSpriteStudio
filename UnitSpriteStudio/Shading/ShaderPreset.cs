using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace UnitSpriteStudio.Shading {
	[Serializable]
	class ShaderPreset {
		internal string Name;

		//Shader values
		private Vector3D LightDirection;
		private Vector3D EyeDirection;
		private float DiffuseReflection;
		private float SpecularReflection;
		private double Shininess;
		private float ShadeRange;
		private float AmbientDarkness;

		// NormalMap
		private double SurfaceCurvePower;
		private Shading.NormalMap.EHeightCalculation HeightCalculation;
		private float HeightParameter;

		public ShaderPreset(string name, NormalMap normalMap, PhongShader shader) {
			Name = name;
			LightDirection = shader.LightDirection;
			EyeDirection = shader.EyeDirection;
			DiffuseReflection = shader.DiffuseReflection;
			SpecularReflection = shader.SpecularReflection;
			Shininess = shader.Shininess;
			ShadeRange = shader.ShadeRange;
			AmbientDarkness = shader.AmbientDarkness;

			SurfaceCurvePower = normalMap.SurfaceCurvePower;
			HeightCalculation = normalMap.HeightCalculation;
			HeightParameter = normalMap.HeightParameter;
		}

		public void Apply(NormalMap normalMap, PhongShader shader) {
			shader.LightDirection = LightDirection;
			shader.EyeDirection = EyeDirection;
			shader.DiffuseReflection = DiffuseReflection;
			shader.SpecularReflection = SpecularReflection;
			shader.Shininess = Shininess;
			shader.ShadeRange = ShadeRange;
			shader.AmbientDarkness = AmbientDarkness;

			normalMap.SurfaceCurvePower = SurfaceCurvePower;
			normalMap.HeightCalculation = HeightCalculation;
			normalMap.HeightParameter = HeightParameter;
		}
		public override string ToString() {
			return Name;
		}

		public static void WriteToBinaryFile(string filePath, ShaderPreset preset) {
			using (Stream stream = File.Open(filePath, FileMode.Create)) {
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				binaryFormatter.Serialize(stream, preset);
			}
		}

		public static ShaderPreset ReadFromBinaryFile(string filePath) {
			using (Stream stream = File.Open(filePath, FileMode.Open)) {
				var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				return (ShaderPreset)binaryFormatter.Deserialize(stream);
			}
		}
	}
}
