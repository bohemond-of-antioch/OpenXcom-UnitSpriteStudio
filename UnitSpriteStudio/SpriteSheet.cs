using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UnitSpriteStudio.DrawingRoutines;

namespace UnitSpriteStudio {
	class SpriteSheet {
		internal class FrameSource {
			private WriteableBitmap sprite;
			private Dictionary<int, ImageSource> imageSourceCache;
			private int columnCount;
			internal FrameSource(string FileName) {
				var originalSprite = new WriteableBitmap(new BitmapImage(new Uri(FileName, UriKind.Absolute)));
				if (originalSprite.Format!=PixelFormats.Indexed8) {
					throw new Exception("Unspported image format!");
				}
				BitmapPalette UFOBattlescapePalette = GetUFOBattlescapePalette();
				sprite = new WriteableBitmap(originalSprite.PixelWidth, originalSprite.PixelHeight, originalSprite.DpiX, originalSprite.DpiY, PixelFormats.Indexed8, UFOBattlescapePalette);
				byte[] originalPixels = new byte[originalSprite.PixelWidth * originalSprite.PixelHeight];
				originalSprite.CopyPixels(originalPixels, originalSprite.BackBufferStride, 0);
				sprite.WritePixels(new System.Windows.Int32Rect(0, 0, originalSprite.PixelWidth, originalSprite.PixelHeight), originalPixels, sprite.BackBufferStride, 0);
				columnCount = sprite.PixelWidth / 32;
				imageSourceCache = new Dictionary<int, ImageSource>();
			}

			internal BitmapPalette GetUFOBattlescapePalette() {
				System.Drawing.Imaging.ColorPalette UFOBattlescapePalette = Resources.UFOBattlescapePalette.Palette;
				List<Color> outputColorList = new List<Color>();
				foreach (System.Drawing.Color c in UFOBattlescapePalette.Entries) {
					outputColorList.Add(Color.FromArgb(c.A,c.R,c.G,c.B));
				}
				outputColorList[0] = Color.FromArgb(0, 0, 255, 0);
				return new BitmapPalette(outputColorList);
			}

			internal (int Width,int Height) GetSize() {
				return (sprite.PixelWidth, sprite.PixelHeight);
			}

			internal WriteableBitmap getSprite() {
				return sprite;
			}

			internal PngBitmapEncoder GetSpriteEncoder() {
				PngBitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(sprite));
				return encoder;
			}

			private (int X, int Y) GetFrameCoords(int FrameIndex) {
				(int X, int Y) result;
				result.X = (FrameIndex % columnCount) * 32;
				result.Y = ((int)(FrameIndex / columnCount)) * 40;
				return result;
			}

			internal ImageSource GetFrame(int FrameIndex) {
				if (imageSourceCache.ContainsKey(FrameIndex)) {
					return imageSourceCache[FrameIndex];
				}
				(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
				CroppedBitmap result = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
				imageSourceCache.Add(FrameIndex, result);
				return result;
			}

			internal byte[] GetFramePixelData(int FrameIndex) {
				(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
				byte[] pixels = new byte[32*40];
				sprite.CopyPixels(new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y , 32, 40), pixels, 32, 0);
				return pixels;
			}

			internal byte GetPixel(int FrameIndex, int X, int Y) {
				if (X < 0 || Y < 0 || X >= 32 || Y >= 40) return 0;
				(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
				byte[] pixels = new byte[1];
				sprite.CopyPixels(new System.Windows.Int32Rect(frameCoords.X + X, frameCoords.Y + Y, 1, 1), pixels, sprite.BackBufferStride, 0);
				return pixels[0];
			}
			internal void SetFramePixelData(int FrameIndex, byte[] pixelData) {
				(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
				try {
					// Reserve the back buffer for updates.
					sprite.Lock();

					unsafe {
						for (int x = 0; x < 32; x++) {
							for (int y = 0; y < 40; y++) {
								// Get a pointer to the back buffer.
								IntPtr pBackBuffer = sprite.BackBuffer;

								// Find the address of the pixel to draw.
								pBackBuffer += (frameCoords.Y + y) * sprite.BackBufferStride;
								pBackBuffer += (frameCoords.X + x) * 1;

								// Assign the color data to the pixel.
								*((byte*)pBackBuffer) = pixelData[x + y * 32];
							}
						}
					}

					// Specify the area of the bitmap that changed.
					sprite.AddDirtyRect(new System.Windows.Int32Rect(0, 0, 32, 40));
				} finally {
					// Release the back buffer and make it available for display.
					sprite.Unlock();
				}
				imageSourceCache[FrameIndex] = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
			}

			internal void SetPixel(int FrameIndex, int X, int Y, byte Color) {
				if (X < 0 || Y < 0 || X >= 32 || Y >= 40) return;
				(int X, int Y) frameCoords = GetFrameCoords(FrameIndex);
				try {
					// Reserve the back buffer for updates.
					sprite.Lock();

					unsafe {
						// Get a pointer to the back buffer.
						IntPtr pBackBuffer = sprite.BackBuffer;

						// Find the address of the pixel to draw.
						pBackBuffer += (frameCoords.Y + Y) * sprite.BackBufferStride;
						pBackBuffer += (frameCoords.X + X) * 1;

						// Assign the color data to the pixel.
						*((byte*)pBackBuffer) = Color;
					}

					// Specify the area of the bitmap that changed.
					sprite.AddDirtyRect(new System.Windows.Int32Rect(X, Y, 1, 1));
				} finally {
					// Release the back buffer and make it available for display.
					sprite.Unlock();
				}
				imageSourceCache[FrameIndex] = new CroppedBitmap(sprite, new System.Windows.Int32Rect(frameCoords.X, frameCoords.Y, 32, 40));
			}

			internal BitmapPalette GetColorPalette() {
				return sprite.Palette;
			}
		}

		internal readonly DrawingRoutines.DrawingRoutine drawingRoutine;
		internal FrameSource frameSource=null;
		private string sourceFileName;

		internal void LoadSprite(string FileName) {
			frameSource = new FrameSource(FileName);
			sourceFileName = FileName;
		}

		internal void Save(string FileName="") {
			if (FileName.Equals("")) FileName = sourceFileName;

			using (var stream = new FileStream(FileName, FileMode.Create)) {
				PngBitmapEncoder encoder = frameSource.GetSpriteEncoder();
				encoder.Save(stream);
			}
		}

		internal SpriteSheet(DrawingRoutine drawingRoutine) {
			this.drawingRoutine = drawingRoutine;
		}

		internal void DrawCompositeImage(FrameMetadata metadata,DrawingContext drawingContext, int highlightedLayer=-1) {
			if (frameSource!=null) drawingRoutine.DrawCompositeImage(frameSource, metadata, drawingContext, highlightedLayer);
		}

		internal void SetPixel(FrameMetadata metadata,int layer, int X, int Y, byte Color) {
			DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, layer);
			frameSource.SetPixel(frameInfo.Index, X-frameInfo.OffsetX, Y-frameInfo.OffsetY, Color);
		}

		internal byte GetPixel(FrameMetadata metadata, int layer, int X, int Y) {
			DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, layer);
			return frameSource.GetPixel(frameInfo.Index, X - frameInfo.OffsetX, Y - frameInfo.OffsetY);
		}

		internal ImageSource GetLayerImage(FrameMetadata metadata,int layer) {
			return frameSource.GetFrame(drawingRoutine.GetLayerFrame(metadata,layer).Index);
		}

		internal BitmapPalette GetColorPalette() {
			return frameSource.GetColorPalette();
		}

		internal string GenerateRulSnippet() {
			string rulText="";

			(int Width, int Height) spriteSize = frameSource.GetSize();
			rulText += string.Format("  - type: {0}.PCK\n", Path.GetFileNameWithoutExtension(sourceFileName).ToUpper());
			rulText += "    subX: 32\n";
			rulText += "    subY: 40\n";
			rulText += string.Format("    width: {0}\n", spriteSize.Width);
			rulText += string.Format("    height: {0}\n", spriteSize.Height);
			rulText += "    files:\n";
			rulText += string.Format("      0: Path/{0}\n", Path.GetFileName(sourceFileName));
			return rulText;
		}

		internal WriteableBitmap getSprite() {
			return frameSource.getSprite();
		}
	}
}
