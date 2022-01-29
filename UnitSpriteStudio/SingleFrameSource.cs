using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace UnitSpriteStudio {
	internal class SingleFrameSource : FrameSource {
		internal override FrameSource Clone() {
			return new SingleFrameSource(sprite);
		}

		internal SingleFrameSource(BitmapSource sourceBitmap) {
			var originalSprite = new WriteableBitmap(sourceBitmap);
			if (originalSprite.Format != PixelFormats.Indexed8) {
				throw new Exception("Unsupported image format!");
			}
			string IdentifiedPalette = Palettes.Identify(sourceBitmap.Palette);
			BitmapPalette LoadedImagePalette;
			if (IdentifiedPalette.Equals("Image Palette")) {
				LoadedImagePalette = sourceBitmap.Palette;
			} else {
				LoadedImagePalette = Palettes.FromName(IdentifiedPalette);
			}
			sprite = new WriteableBitmap(originalSprite.PixelWidth, originalSprite.PixelHeight, originalSprite.DpiX, originalSprite.DpiY, PixelFormats.Indexed8, LoadedImagePalette);
			byte[] originalPixels = new byte[originalSprite.PixelWidth * originalSprite.PixelHeight];
			originalSprite.CopyPixels(originalPixels, originalSprite.BackBufferStride, 0);
			sprite.WritePixels(new System.Windows.Int32Rect(0, 0, originalSprite.PixelWidth, originalSprite.PixelHeight), originalPixels, sprite.BackBufferStride, 0);
			return;
		}


		internal override BitmapSource GetFrame(int FrameIndex) {
			return sprite;
		}

		internal override byte[] GetFramePixelData(int FrameIndex) {
			byte[] pixels = new byte[sprite.PixelWidth*sprite.PixelHeight];
			lock (sprite) {
				try {
					sprite.CopyPixels(new System.Windows.Int32Rect(0, 0, sprite.PixelWidth, sprite.PixelHeight), pixels, sprite.BackBufferStride, 0);
				} catch (Exception) {

				}
			}
			return pixels;
		}

		internal override byte GetPixel(int FrameIndex, int X, int Y) {
			if (X < 0 || Y < 0 || X >= sprite.PixelWidth || Y >= sprite.PixelHeight) return 0;
			byte[] pixels = new byte[1];
			lock (sprite) {
				try {
					sprite.CopyPixels(new System.Windows.Int32Rect(X, Y, 1, 1), pixels, sprite.BackBufferStride, 0);
				} catch (Exception) {

				}
			}
			return pixels[0];
		}

		internal override void SetFramePixelData(int FrameIndex, byte[] pixelData) {
			lock (sprite) {
				try {
					sprite.WritePixels(new System.Windows.Int32Rect(0, 0, sprite.PixelWidth, sprite.PixelHeight), pixelData, sprite.BackBufferStride, 0);
				} catch (Exception) {

				}
			}
			OnFrameChanged();
		}

		internal override void SetInternalSprite(WriteableBitmap newSprite) {
			this.sprite = newSprite;
			OnFrameChanged();
		}

		internal override void SetPixel(int FrameIndex, int X, int Y, byte Color) {
			if (X < 0 || Y < 0 || X >= sprite.PixelWidth|| Y >= sprite.PixelHeight) return;
			lock (sprite) {
				try {
					sprite.WritePixels(new System.Windows.Int32Rect(X, Y, 1, 1), new byte[] { Color }, 1, 0);
				} catch (Exception) {

				}
			}
			OnFrameChanged();
		}
	}
}
