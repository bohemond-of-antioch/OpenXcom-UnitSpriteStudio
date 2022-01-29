using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio {
	internal class FrameSource {
		internal event Action OnChanged;
		public WriteableBitmap sprite { get; protected set; }
		private Dictionary<int, BitmapSource> imageSourceCache;
		private int columnCount;

		protected void OnFrameChanged() {
			OnChanged?.Invoke();
		}

		virtual internal void SetInternalSprite(WriteableBitmap newSprite) {
			this.sprite = newSprite;
			this.imageSourceCache = new Dictionary<int, BitmapSource>();
			OnFrameChanged();
		}
		virtual internal FrameSource Clone() {
			return new FrameSource(sprite);
		}
		protected FrameSource() { }
		internal FrameSource(BitmapSource sourceBitmap) {
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
			columnCount = sprite.PixelWidth / 32;
			imageSourceCache = new Dictionary<int, BitmapSource>();
		}

		internal void SwitchToPalette(BitmapPalette NewPalette) {
			var SpriteWithNewPalette=new WriteableBitmap(sprite.PixelWidth, sprite.PixelHeight, sprite.DpiX, sprite.DpiY, PixelFormats.Indexed8, NewPalette);
			byte[] PixelData = new byte[sprite.PixelWidth * sprite.PixelHeight];
			sprite.CopyPixels(PixelData, sprite.BackBufferStride, 0);
			SpriteWithNewPalette.WritePixels(new System.Windows.Int32Rect(0, 0, sprite.PixelWidth, sprite.PixelHeight), PixelData, sprite.BackBufferStride, 0);
			sprite = SpriteWithNewPalette;
			imageSourceCache = new Dictionary<int, BitmapSource>();
		}

		internal void MatchToPalette(BitmapPalette palette) {
			var spriteWithNewPalette = new WriteableBitmap(sprite.PixelWidth, sprite.PixelHeight, sprite.DpiX, sprite.DpiY, PixelFormats.Indexed8, palette);
			byte[] pixelData = new byte[sprite.PixelWidth * sprite.PixelHeight];
			sprite.CopyPixels(pixelData, sprite.BackBufferStride, 0);

			IList<Color> paletteWPF = palette.Colors;
			List<System.Drawing.Color> newPalette = new List<System.Drawing.Color>();
			foreach (var c in paletteWPF) {
				newPalette.Add(System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B));
			}
			paletteWPF = sprite.Palette.Colors;
			List<System.Drawing.Color> spritePalette = new List<System.Drawing.Color>();
			foreach (var c in paletteWPF) {
				spritePalette.Add(System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B));
			}
			byte[] paletteMap = new byte[256];
			for (int t=0;t<256;t++) {
				paletteMap[t] = Palettes.FindNearestColor(spritePalette[t], newPalette);
			}

			byte[] transformedPixels = new byte[pixelData.Length];
			for (int f = 0; f < transformedPixels.Length; f++) {
				transformedPixels[f] = paletteMap[pixelData[f]];
			}

			spriteWithNewPalette.WritePixels(new System.Windows.Int32Rect(0, 0, sprite.PixelWidth, sprite.PixelHeight), transformedPixels, sprite.BackBufferStride, 0);
			sprite = spriteWithNewPalette;
			imageSourceCache = new Dictionary<int, BitmapSource>();
		}

		internal (int Width, int Height) GetSize() {
			return (sprite.PixelWidth, sprite.PixelHeight);
		}

		internal PngBitmapEncoder GetPNGSpriteEncoder() {
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(sprite));
			return encoder;
		}
		internal GifBitmapEncoder GetGIFSpriteEncoder() {
			GifBitmapEncoder encoder = new GifBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(sprite));
			return encoder;
		}

		private (int X, int Y) GetFrameCoords(int FrameIndex) {
			(int X, int Y) result;
			result.X = (FrameIndex % columnCount) * 32;
			result.Y = ((int)(FrameIndex / columnCount)) * 40;
			return result;
		}

		virtual internal BitmapSource GetFrame(int FrameIndex) {
			if (imageSourceCache.ContainsKey(FrameIndex)) {
				return imageSourceCache[FrameIndex];
			}
			(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
			CroppedBitmap result;
			lock (sprite) {
				result = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
				imageSourceCache.Add(FrameIndex, result);
			}
			return result;
		}

		virtual internal byte[] GetFramePixelData(int FrameIndex) {
			(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
			byte[] pixels = new byte[32 * 40];
			lock (sprite) {
				try {
					sprite.CopyPixels(new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40), pixels, 32, 0);
				} catch (Exception) {

				}
			}
			return pixels;
		}

		virtual internal byte GetPixel(int FrameIndex, int X, int Y) {
			if (X < 0 || Y < 0 || X >= 32 || Y >= 40) return 0;
			(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
			byte[] pixels = new byte[1];
			lock (sprite) {
				try {
					sprite.CopyPixels(new System.Windows.Int32Rect(frameCoords.X + X, frameCoords.Y + Y, 1, 1), pixels, sprite.BackBufferStride, 0);
				} catch (Exception) {

				}
			}
			return pixels[0];
		}
		virtual internal void SetFramePixelData(int FrameIndex, byte[] pixelData) {
			(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
			lock (sprite) {
				try {
					sprite.WritePixels(new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40), pixelData, 32, 0);
					imageSourceCache[FrameIndex] = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
				} catch (Exception) {

				}
			}
			OnFrameChanged();
		}

		virtual internal void SetPixel(int FrameIndex, int X, int Y, byte Color) {
			if (X < 0 || Y < 0 || X >= 32 || Y >= 40) return;
			(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);

			lock (sprite) {
				try {
					sprite.WritePixels(new System.Windows.Int32Rect(frameCoords.X + X, frameCoords.Y + Y, 1, 1), new byte[] { Color }, 1, 0);
					imageSourceCache[FrameIndex] = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
				} catch (Exception) {

				}
			}
			OnFrameChanged();
		}

		internal BitmapPalette GetColorPalette() {
			return sprite.Palette;
		}
	}
}
