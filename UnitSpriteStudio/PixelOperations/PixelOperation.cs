using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitSpriteStudio.PixelOperations {
	internal abstract class PixelOperation {
		protected abstract void Transmogrify(byte[] pixelData);
		internal abstract void Edit(Window owner);
		internal virtual void Run(SpriteSheet spriteSheet, List<int> targetFrames, bool IncludeBaseFrames, bool IncludeAlternateFrames) {
			foreach (int f in targetFrames) {
				if (IncludeBaseFrames) {
					byte[] pixelData = spriteSheet.frameSource.GetFramePixelData(f, true);
					Transmogrify(pixelData);
					spriteSheet.frameSource.SetFramePixelData(f, pixelData, true);
				}
				if (IncludeAlternateFrames) {
					foreach (AlternateFrame alternateFrame in ((App)App.Current).AlternateFrames) {
						if (alternateFrame.Replaces(f)) {
							int alternateTarget = alternateFrame.GetTargetIndex(f);
							byte[] pixelData = spriteSheet.frameSource.GetFramePixelData(alternateTarget, true);
							Transmogrify(pixelData);
							spriteSheet.frameSource.SetFramePixelData(alternateTarget, pixelData, true);
						}
					}
				}
			}
		}
	}
}
