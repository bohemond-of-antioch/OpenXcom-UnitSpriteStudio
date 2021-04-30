using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitSpriteStudio.PixelOperations {
	class ColorGroupBrightnessOperation : PixelOperation {
		internal byte SourceGroup;
		internal int BrightnessAdjustment;

		public ColorGroupBrightnessOperation(byte sourceGroup, int brightnessAdjustment) {
			SourceGroup = sourceGroup;
			BrightnessAdjustment = brightnessAdjustment;
		}
		public override string ToString() {
			return string.Format("Change Brightness of Color Group {0} by {1}", SourceGroup, -BrightnessAdjustment);
		}

		protected override void Transmogrify(byte[] pixelData) {
			for (int p = 0; p < pixelData.Length; p++) {
				byte pixel = pixelData[p];
				if (pixel == 0) continue;
				byte colorGroup = (byte)(pixel / 16);
				if (colorGroup != SourceGroup) continue;
				int newColor;
				newColor = Math.Max(Math.Min(pixel + BrightnessAdjustment, ((pixel / 16) + 1) * 16 - 1), (pixel / 16) * 16);
				pixelData[p] = (byte)newColor;
			}
		}

		internal override void Edit(Window owner) {
			ColorGroupBrightnessWindow window = new ColorGroupBrightnessWindow();
			window.Owner = owner;
			window.Show();
			window.BindOperation(this);
		}
	}
}
