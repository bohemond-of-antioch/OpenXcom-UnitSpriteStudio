using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineSectopod : DrawingRoutine {
		internal override bool CanHoldItems() {
			return false;
		}

		private const int COMPOSITE_IMAGE_WIDTH = 64;
		private const int COMPOSITE_IMAGE_HEIGHT = 56;

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (256, 840);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
		}

		internal override void DrawCompositeImage(SpriteSheet.FrameSource sprite, FrameMetadata metadata, DrawingContext drawingContext, int highlightedLayer) {
			for (int l = 0; l < 4; l++) {
				if (highlightedLayer != -1 && l != highlightedLayer) continue;
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, l);
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			}
		}
		internal override Selection GetCompositeOutline(SpriteSheet.FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
			for (int l = 0; l < 4; l++) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, l);
				BitmapSource image = sprite.GetFrame(frameInfo.Index);
				byte[] pixels = new byte[COMPOSITE_IMAGE_WIDTH * COMPOSITE_IMAGE_HEIGHT];
				image.CopyPixels(pixels, COMPOSITE_IMAGE_WIDTH, 0);
				for (int f = 0; f < pixels.Length; f++) {
					if (pixels[f] > 0) {
						outline.AddPoint((f % COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetX, f / COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetY));
					}
				}
			}
			return outline;
		}


		private const int FRAME_WALK = 32;
		private static readonly int[] OFFSET_LAYER_X = { 16, 32, 0, 16 };
		private static readonly int[] OFFSET_LAYER_Y = { 0, 8, 8, 16 };

		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			int offsetX, offsetY, frameIndex;
			offsetX = OFFSET_LAYER_X[layer];
			offsetY = OFFSET_LAYER_Y[layer];
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Walk) {
				frameIndex = layer * 4 + FRAME_WALK + metadata.Direction * 16 + metadata.AnimationFrame;
			} else {
				frameIndex = layer * 8 + metadata.Direction;
			}
			return new LayerFrameInfo(frameIndex, offsetX, offsetY);
		}

		internal override int GetAnimationFrameCount(FrameMetadata metadata) {
			switch ((EPrimaryFrame)metadata.PrimaryFrame) {
				case EPrimaryFrame.Walk:
					return 4;
				default:
					return 1;
			}
		}

		private enum ELayer {
			Top,
			Right,
			Left,
			Center
		}
		internal override string[] LayerNames() {
			return new string[] { "Top", "Right", "Left", "Center" };
		}
		internal override int ChangeArmsLayer(int layer) {
			if (layer == 1) return 2;
			if (layer == 2) return 1;
			return layer;
		}
		private enum EPrimaryFrame {
			Stand,
			Walk
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "Stand", "Walk" };
		}
		internal override int[] PrimaryFrameMirroring() {
			return new int[] { 0, 1 };
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

		internal override SmartLayerType SmartLayerSupported() {
			return SmartLayerType.HWP;
		}
	}
}
