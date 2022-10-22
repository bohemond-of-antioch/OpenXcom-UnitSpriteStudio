using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UnitSpriteStudio.DrawingRoutines;

namespace UnitSpriteStudio {
	class UnitSpriteSheet : SpriteSheet {
		internal readonly DrawingRoutines.DrawingRoutine drawingRoutine;

		internal void Save(string FileName = "") {
			if (FileName.Equals("")) FileName = sourceFileName;
			sourceFileName = FileName;
			BitmapEncoder encoder;
			if (Path.GetExtension(FileName).Equals(".png", StringComparison.OrdinalIgnoreCase)) {
				encoder = frameSource.GetPNGSpriteEncoder();
			} else if (Path.GetExtension(FileName).Equals(".gif", StringComparison.OrdinalIgnoreCase)) {
				encoder = frameSource.GetGIFSpriteEncoder();
			} else {
				throw new Exception("Unsupported file type!");
			}
			using (var stream = new FileStream(FileName, FileMode.Create)) {
				encoder.Save(stream);
			}
		}
		internal UnitSpriteSheet(UnitSpriteSheet cloneFrom) {
			this.drawingRoutine = cloneFrom.drawingRoutine;
			frameSource = cloneFrom.frameSource.Clone();
			sourceFileName = "";
		}

		internal UnitSpriteSheet(DrawingRoutine drawingRoutine, string FileName) {
			this.drawingRoutine = drawingRoutine;
			frameSource = drawingRoutine.CreateFrameSource(new BitmapImage(new Uri(FileName, UriKind.Absolute)));
			sourceFileName = FileName;
		}
		internal UnitSpriteSheet(DrawingRoutine drawingRoutine)
			: this(drawingRoutine, drawingRoutine.DefaultSpriteSheetSize(), drawingRoutine.DefaultSpriteSheetPalette()) { }
		internal UnitSpriteSheet(DrawingRoutine drawingRoutine, (int Width, int Height) spriteSize, BitmapPalette spritePalette) {
			this.drawingRoutine = drawingRoutine;
			var emptyBitmap = new WriteableBitmap(spriteSize.Width, spriteSize.Height, 96, 96, PixelFormats.Indexed8, spritePalette);
			frameSource = drawingRoutine.CreateFrameSource(emptyBitmap);
		}

		internal void ReloadFromSource() {
			if (sourceFileName != "") {
				var sourceImage = new BitmapImage();
				using (FileStream fs = new FileStream(sourceFileName, FileMode.Open)) {
					sourceImage.BeginInit();
					sourceImage.StreamSource = fs;
					sourceImage.CacheOption = BitmapCacheOption.OnLoad;
					sourceImage.EndInit();
				}
				frameSource = drawingRoutine.CreateFrameSource(sourceImage);
			}
		}

		internal void DrawCompositeImage(FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer = -1) {
			if (frameSource != null) drawingRoutine.DrawCompositeImage(frameSource, metadata, itemSprite, drawingContext, highlightedLayer);
		}

		internal Selection CompositeImageOutline(FrameMetadata metadata) {
			if (frameSource != null) {
				return drawingRoutine.GetCompositeOutline(frameSource, metadata);
			} else {
				return null;
			}
		}

		internal void SetPixel(FrameMetadata metadata, int layer, int X, int Y, byte Color) {
			DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, layer);
			frameSource.SetPixel(frameInfo.Index, X - frameInfo.OffsetX, Y - frameInfo.OffsetY, Color);
		}

		internal byte GetPixel(FrameMetadata metadata, int layer, int X, int Y) {
			DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, layer);
			return frameSource.GetPixel(frameInfo.Index, X - frameInfo.OffsetX, Y - frameInfo.OffsetY);
		}

		internal ImageSource GetLayerImage(FrameMetadata metadata, int layer) {
			DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, layer);
			switch (frameInfo.Target) {
				case DrawingRoutine.LayerFrameInfo.ETarget.Unit:
					return frameSource.GetFrame(frameInfo.Index);
				case DrawingRoutine.LayerFrameInfo.ETarget.Item:
				case DrawingRoutine.LayerFrameInfo.ETarget.None:
				default:
					return null;
			}
		}

		internal BitmapPalette GetColorPalette() {
			return frameSource.GetColorPalette();
		}

		internal string GenerateRulSnippet() {
			string rulText = "";

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
			return frameSource.sprite;
		}
	}
}
