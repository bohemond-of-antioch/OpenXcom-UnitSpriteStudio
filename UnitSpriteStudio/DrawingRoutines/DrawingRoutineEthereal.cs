using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineEthereal : DrawingRoutine {
		internal override bool CanHoldItems() {
			return false;
		}

		private const int COMPOSITE_IMAGE_WIDTH = 32;
		private const int COMPOSITE_IMAGE_HEIGHT = 40;

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (512, 200);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
		}

		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer) {
			LayerFrameInfo frameInfo = GetLayerFrame(metadata, 0);
			ImageSource image = sprite.GetFrame(frameInfo.Index);
			drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
		}
		internal override Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
			LayerFrameInfo frameInfo = GetLayerFrame(metadata, 0);
			BitmapSource image = sprite.GetFrame(frameInfo.Index);
			byte[] pixels = new byte[COMPOSITE_IMAGE_WIDTH * COMPOSITE_IMAGE_HEIGHT];
			image.CopyPixels(pixels, COMPOSITE_IMAGE_WIDTH, 0);
			for (int f = 0; f < pixels.Length; f++) {
				if (pixels[f] > 0) {
					outline.AddPoint((f % COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetX, f / COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetY));
				}
			}
			return outline;
		}


		private const int FRAME_WALK = 8;
		private const int FRAME_DEATH = 72;

		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				return new LayerFrameInfo(FRAME_DEATH + metadata.AnimationFrame, 0, 0);
			}
			int frameIndex;
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Walk) {
				frameIndex = FRAME_WALK + metadata.Direction * 8 + metadata.AnimationFrame;
			} else {
				frameIndex = metadata.Direction;
			}
			return new LayerFrameInfo(frameIndex, 0, 0);
		}

		internal override int GetAnimationFrameCount(FrameMetadata metadata) {
			switch ((EPrimaryFrame)metadata.PrimaryFrame) {
				case EPrimaryFrame.Walk:
					return 8;
				case EPrimaryFrame.Death:
					return 3;
				default:
					return 1;
			}
		}

		private enum ELayer {
			Body
		}
		internal override string[] LayerNames() {
			return new string[] { "Body" };
		}
		internal override int ChangeArmsLayer(int layer) {
			return layer;
		}
		private enum EPrimaryFrame {
			Stand,
			Walk,
			Death
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "Stand", "Walk", "Death" };
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

		internal override BitmapPalette DefaultSpriteSheetPalette() {
			return Palettes.FromName("UFO Battlescape");
		}
	}
}
