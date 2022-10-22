using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitSpriteStudio {
	[Serializable()]
	internal class UnitSpriteStudioClipboardFormat {
		internal int Width;
		internal int Height;
		byte[] Pixels;

		internal UnitSpriteStudioClipboardFormat(FloatingSelectionBitmap floatingSelection) {
			Width = floatingSelection.bitmap.PixelWidth;
			Height = floatingSelection.bitmap.PixelHeight;
			Pixels = floatingSelection.GetAllPixels();
		}

		internal FloatingSelectionBitmap GetFloatingSelection(UnitSpriteSheet spriteSheet) {
			FloatingSelectionBitmap result;
			result = new FloatingSelectionBitmap(spriteSheet);
			result.bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), Pixels, Width, 0);
			return result;
		}

	}
}
