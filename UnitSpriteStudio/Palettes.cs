using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio {
	class Palettes {
		private static BitmapPalette ConvertPalette(System.Drawing.Imaging.ColorPalette palette) {
			List<Color> outputColorList = new List<Color>();
			foreach (System.Drawing.Color c in palette.Entries) {
				outputColorList.Add(Color.FromArgb(c.A, c.R, c.G, c.B));
			}
			outputColorList[0] = Color.FromArgb(0, 0, 255, 0);
			return new BitmapPalette(outputColorList);
		}
		private static Dictionary<string, BitmapPalette> CachedPalettes = new Dictionary<string, BitmapPalette> {
   {"UFO Battlescape", ConvertPalette(Resources.UFOBattlescapePalette.Palette)},
   {"UFO Basescape", ConvertPalette(Resources.UFOBasescapePalette.Palette)},
   {"UFO Geoscape", ConvertPalette(Resources.UFOGeoscapePalette.Palette)},
   {"UFO Graphs", ConvertPalette(Resources.UFOGraphsPalette.Palette)},
   {"UFO Ufopaedia", ConvertPalette(Resources.UFOUfopaediaPalette.Palette)},
   {"TFTD Basescape", ConvertPalette(Resources.TFTDBasescapePalette.Palette)},
   {"TFTD Battlescape", ConvertPalette(Resources.TFTDBattlescapePalette.Palette)},
   {"TFTD Geoscape", ConvertPalette(Resources.TFTDGeoscapePalette.Palette)},
   {"TFTD Graphs", ConvertPalette(Resources.TFTDGraphsPalette.Palette)}
};
		public static BitmapPalette FromName(string Name) {
			try {
				return CachedPalettes[Name];
			} catch (Exception) {
				return CachedPalettes["UFO Battlescape"];
			}
		}
		public static string Identify(BitmapPalette PaletteToIdentify) {
			foreach (var CachedPalette in CachedPalettes) {
				int MatchingColors = 0;
				for (int f = 1; f < 240; f++) {
					if (CachedPalette.Value.Colors[f].Equals(PaletteToIdentify.Colors[f])) {
						MatchingColors++;
					}
				}
				if (MatchingColors == 239) return CachedPalette.Key;
			}
			return "Image Palette";
		}
		// Dumb RGB match seems to work best after all
		public static byte FindNearestColorRGB(System.Drawing.Color color, List<System.Drawing.Color> palette) {
			int BestMatchIndex = -1;
			float BestMatchCloseness = 1000;
			for (int i = 0; i < palette.Count; i++) {
				float closeness = 0;
				float rDelta = Math.Abs(color.R - palette[i].R);
				float gDelta = Math.Abs(color.G - palette[i].G);
				float bDelta = Math.Abs(color.B - palette[i].B);
				closeness = rDelta + gDelta + bDelta;
				if (BestMatchIndex == -1 || BestMatchCloseness > closeness) {
					BestMatchCloseness = closeness;
					BestMatchIndex = i;
				}
			}
			return (byte)BestMatchIndex;
		}
		public static byte FindNearestColor(System.Drawing.Color color, List<System.Drawing.Color> palette) {
			int BestMatchIndex = -1;
			float BestMatchCloseness = 1000;
			for (int i = 0; i < palette.Count; i++) {
				float closeness = 0;
				float hueDelta = Math.Abs(color.GetHue() - palette[i].GetHue());
				float saturationDelta = Math.Abs(color.GetSaturation() - palette[i].GetSaturation());
				float brightnessDelta = Math.Abs(color.GetBrightness() - palette[i].GetBrightness());
				closeness = hueDelta * 0.48f + saturationDelta * 0.25f + brightnessDelta * 0.25f;
				if (BestMatchIndex == -1 || BestMatchCloseness > closeness) {
					BestMatchCloseness = closeness;
					BestMatchIndex = i;
				}
			}
			return (byte)BestMatchIndex;
		}
	}
}
