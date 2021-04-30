using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitSpriteStudio.PixelOperations {
	class ChangeColorGroupOperation : PixelOperation {
		internal byte SourceGroup, DestinationGroup;

		public ChangeColorGroupOperation(byte sourceGroup, byte targetGroup) {
			SourceGroup = sourceGroup;
			DestinationGroup = targetGroup;
		}

		public override string ToString() {
			return string.Format("Change Color Group ({0}->{1})", SourceGroup, DestinationGroup);
		}

		protected override void Transmogrify(byte[] pixelData) {
			for (int p=0;p<pixelData.Length;p++) {
				byte pixel = pixelData[p];
				if (pixel == 0) continue;
				byte colorGroup = (byte)(pixel / 16);
				if (colorGroup != SourceGroup) continue;
				int pixelBrightness = pixel - colorGroup * 16;
				int newColor;
				newColor = Math.Max((byte)1, DestinationGroup*16); // Brightest color in the group
				newColor = Math.Min(newColor + pixelBrightness, ((newColor / 16) + 1) * 16 - 1);
				pixelData[p] = (byte)newColor;
			}
		}

		internal override void Edit(Window owner) {
			ChangeColorGroupWindow window = new ChangeColorGroupWindow();
			window.Owner = owner;
			window.Show();
			window.BindOperation(this);
		}
	}
}
