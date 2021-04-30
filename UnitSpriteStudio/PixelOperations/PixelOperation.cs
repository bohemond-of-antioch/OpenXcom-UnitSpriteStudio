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
		internal virtual void Run(SpriteSheet spriteSheet, List<int> targetFrames) {
			foreach (int f in targetFrames) {
				byte[] pixelData = spriteSheet.frameSource.GetFramePixelData(f);
				Transmogrify(pixelData);
				spriteSheet.frameSource.SetFramePixelData(f, pixelData);
			}
		}

	}
}
