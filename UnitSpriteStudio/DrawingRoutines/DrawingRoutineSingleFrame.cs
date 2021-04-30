using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineSingleFrame : DrawingRoutine {
		internal override bool CanHoldItems() {
			return false;
		}

		private int SpriteWidth = 32;
		private int SpriteHeight = 40;

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (SpriteWidth, SpriteHeight);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (SpriteWidth, SpriteHeight);
		}

		internal void SetSize(int Width,int Height) {
			SpriteWidth = Width;
			SpriteHeight = Height;
		}

		internal override void DrawCompositeImage(SpriteSheet.FrameSource sprite, FrameMetadata metadata, DrawingContext drawingContext, int highlightedLayer) {
			ImageSource image = sprite.GetFrame(0);
			drawingContext.DrawImage(image, new Rect(0, 0, SpriteWidth, SpriteHeight));
		}
		internal override Selection GetCompositeOutline(SpriteSheet.FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(SpriteWidth, SpriteHeight);
			BitmapSource image = sprite.GetFrame(0);
			byte[] pixels = new byte[SpriteWidth * SpriteHeight];
			image.CopyPixels(pixels, SpriteWidth, 0);
			for (int f = 0; f < pixels.Length; f++) {
				if (pixels[f] > 0) {
					outline.AddPoint((f % SpriteWidth + 0, f / SpriteWidth + 0));
				}
			}
			return outline;
		}


		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			return new LayerFrameInfo(0, 0, 0);
		}

		internal override int GetAnimationFrameCount(FrameMetadata metadata) {
				return 1;
		}

		private enum ELayer {
			Sprite
		}
		internal override string[] LayerNames() {
			return new string[] { "Sprite" };
		}
		internal override int ChangeArmsLayer(int layer) {
			return layer;
		}
		private enum EPrimaryFrame {
			None
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "None" };
		}
		internal override int[] PrimaryFrameMirroring() {
			return new int[] { 0 };
		}
		private enum ESecondaryFrame {
			None
		}
		internal override string[] SecondaryFrameNames() {
			return new string[] { "None" };
		}
		internal override int[] SecondaryFrameMirroring() {
			return new int[] { 0 };
		}
		private enum ETertiaryFrame {
			None
		}
		internal override string[] TertiaryFrameNames() {
			return new string[] { "None" };
		}
		internal override int[] TertiaryFrameMirroring() {
			return new int[] { 0 };
		}
	}
}
