using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UnitSpriteStudio.DrawingRoutines;

namespace UnitSpriteStudio {
	class FloatingSelectionBitmap {
		internal WriteableBitmap bitmap;

		public FloatingSelectionBitmap(SpriteSheet spriteSheet) {
			bitmap = new WriteableBitmap(spriteSheet.drawingRoutine.CompositeImageSize().Width, spriteSheet.drawingRoutine.CompositeImageSize().Height, 96, 96, spriteSheet.getSprite().Format, spriteSheet.getSprite().Palette);
		}

		internal byte[] GetAllPixels() {
			byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight];
			bitmap.CopyPixels(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth, 0);
			return pixels;
		}
		internal void PutAllPixels(byte[] pixels) {
			bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth, 0);
		}
		internal byte GetPixel(int X, int Y) {
			byte[] pixels = new byte[1];
			bitmap.CopyPixels(new System.Windows.Int32Rect(X, Y, 1, 1), pixels, 1, 0);
			return pixels[0];
		}

		internal void PastePixels(SpriteSheet spriteSheet, DrawingRoutines.FrameMetadata frameMetadata, int layer, int shiftX, int shiftY, System.Drawing.Bitmap mask = null) {
			byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight];
			bitmap.CopyPixels(pixels, bitmap.BackBufferStride, 0);
			for (int x = 0; x < bitmap.PixelWidth; x++) {
				for (int y = 0; y < bitmap.PixelHeight; y++) {
					byte colorIndex = pixels[x + y * bitmap.BackBufferStride];
					if (colorIndex > 0) {
						if (mask != null && mask.GetPixel(x, y).R <= 0) continue;
						spriteSheet.SetPixel(frameMetadata, layer, x + shiftX, y + shiftY, colorIndex);
					}
				}
			}
		}

		internal int CopyPixels(Selection selectedArea, SpriteSheet spriteSheet, DrawingRoutines.FrameMetadata frameMetadata, int layer, bool cut = false, bool transparency = false) {
			int x, y;
			int copiedPixels = 0;
			DrawingRoutine.LayerFrameInfo frameInfo = spriteSheet.drawingRoutine.GetLayerFrame(frameMetadata, layer);
			byte[] sourcePixels = spriteSheet.frameSource.GetFramePixelData(frameInfo.Index);
			byte[] destinationPixels = GetAllPixels();

			for (x = 0; x < selectedArea.SizeX; x++) {
				for (y = 0; y < selectedArea.SizeY; y++) {
					if (selectedArea.GetPoint(x, y)) {
						(int X, int Y) pointInSource;
						pointInSource.X = x - frameInfo.OffsetX;
						pointInSource.Y = y - frameInfo.OffsetY;
						if (pointInSource.X < 0 || pointInSource.Y < 0 || pointInSource.X >= 32 || pointInSource.Y >= 40) continue;
						copiedPixels++;
						byte colorIndex = sourcePixels[pointInSource.X + pointInSource.Y * 32];
						if (transparency && colorIndex == 0) continue; // Skip pixel if copying with transparency
						destinationPixels[x + y * selectedArea.SizeX] = colorIndex;

						if (cut) sourcePixels[pointInSource.X + pointInSource.Y * 32] = 0;
					}
				}
			}

			PutAllPixels(destinationPixels);
			if (cut) spriteSheet.frameSource.SetFramePixelData(frameInfo.Index, sourcePixels);

			return copiedPixels;
		}

		internal void SetPixel(int X, int Y, byte Color) {
			try {
				// Reserve the back buffer for updates.
				bitmap.Lock();

				unsafe {
					// Get a pointer to the back buffer.
					IntPtr pBackBuffer = bitmap.BackBuffer;

					// Find the address of the pixel to draw.
					pBackBuffer += (Y) * bitmap.BackBufferStride;
					pBackBuffer += (X) * 1;

					// Assign the color data to the pixel.
					*((byte*)pBackBuffer) = Color;
				}

				// Specify the area of the bitmap that changed.
				bitmap.AddDirtyRect(new System.Windows.Int32Rect(X, Y, 1, 1));
			} finally {
				// Release the back buffer and make it available for display.
				bitmap.Unlock();
			}
		}
	}
}
