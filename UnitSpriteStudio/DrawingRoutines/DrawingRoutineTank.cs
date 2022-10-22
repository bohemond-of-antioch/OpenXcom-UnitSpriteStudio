using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineTank : DrawingRoutine {
		internal override bool CanHoldItems() {
			return false;
		}

		private const int COMPOSITE_IMAGE_WIDTH = 64;
		private const int COMPOSITE_IMAGE_HEIGHT = 56;

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (256, 640);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
		}

		private static readonly ELayer[] LayerDrawingOrder = { ELayer.PropulsionRight, ELayer.PropulsionLeft, ELayer.PropulsionCenter, ELayer.Top, ELayer.Left, ELayer.Right, ELayer.Center, ELayer.Turret };

		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer) {
			for (int l = 0; l < 8; l++) {
				if (highlightedLayer != -1 && (int)LayerDrawingOrder[l] != highlightedLayer) continue;
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[l]);
				if (frameInfo.Target == LayerFrameInfo.ETarget.None) continue;
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			}
		}
		internal override Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
			for (int l = 0; l < 8; l++) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[l]);
				if (frameInfo.Target == LayerFrameInfo.ETarget.None) continue;
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


		private const int FRAME_GROUND = 0;
		private const int FRAME_HOVER = 32;
		private const int FRAME_PROPULSION_RIGHT = 104;
		private const int FRAME_PROPULSION_LEFT = 112;
		private const int FRAME_PROPULSION_CENTER = 120;
		private const int FRAME_TURRET = 64;

		private static readonly int[] OFFSET_LAYER_X = { 16, 32, 0, 16, 16, 32, 0, 16 };
		private static readonly int[] OFFSET_LAYER_Y = { 0, 8, 8, 16, 16, 8, 8, 16 };

		private static readonly int[] OFFSET_HOVER_X = { -2, -7, -5, 0, 5, 7, 2, 0 };
		private static readonly int[] OFFSET_HOVER_Y = { -1, -3, -4, -5, -4, -3, -1, -1 };

		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			int offsetX, offsetY;
			offsetX = OFFSET_LAYER_X[layer];
			offsetY = OFFSET_LAYER_Y[layer];
			switch ((ELayer)layer) {
				case ELayer.PropulsionRight:
					if (metadata.SecondaryFrame == (int)ESecondaryFrame.Hover) {
						return new LayerFrameInfo(FRAME_PROPULSION_RIGHT + metadata.AnimationFrame, offsetX, offsetY);
					} else {
						return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
					}
				case ELayer.PropulsionLeft:
					if (metadata.SecondaryFrame == (int)ESecondaryFrame.Hover) {
						return new LayerFrameInfo(FRAME_PROPULSION_LEFT + metadata.AnimationFrame, offsetX, offsetY);
					} else {
						return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
					}
				case ELayer.PropulsionCenter:
					if (metadata.SecondaryFrame == (int)ESecondaryFrame.Hover) {
						return new LayerFrameInfo(FRAME_PROPULSION_CENTER + metadata.AnimationFrame, offsetX, offsetY);
					} else {
						return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
					}
				case ELayer.Top:
				case ELayer.Left:
				case ELayer.Right:
				case ELayer.Center:
					if (metadata.SecondaryFrame == (int)ESecondaryFrame.Ground) {
						return new LayerFrameInfo(FRAME_GROUND + layer * 8 + metadata.Direction, offsetX, offsetY);
					} else {
						return new LayerFrameInfo(FRAME_HOVER + layer * 8 + metadata.Direction, offsetX, offsetY);
					}
				case ELayer.Turret:
					if (metadata.TertiaryFrame == (int)ETertiaryFrame.None) {
						return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
					} else {
						offsetX += 0;
						offsetY += -4;
						if (metadata.SecondaryFrame == (int)ESecondaryFrame.Hover) {
							offsetX += OFFSET_HOVER_X[metadata.Direction];
							offsetY += OFFSET_HOVER_Y[metadata.Direction];
						}
						return new LayerFrameInfo(FRAME_TURRET + (metadata.TertiaryFrame - 1) * 8 + metadata.Direction, offsetX, offsetY);
					}
				default:
					throw new Exception("No such layer!");

			}
		}

		internal override int GetAnimationFrameCount(FrameMetadata metadata) {
			if (metadata.SecondaryFrame == (int)ESecondaryFrame.Hover) {
				return 8;
			} else {
				return 1;
			}
		}

		private enum ELayer {
			Top,
			Right,
			Left,
			Center,
			Turret,
			PropulsionRight,
			PropulsionLeft,
			PropulsionCenter
		}
		internal override string[] LayerNames() {
			return new string[] { "Top", "Right", "Left", "Center", "Turret", "Propulsion Right", "Propulsion Left", "Propulsion Center" };
		}
		internal override int ChangeArmsLayer(int layer) {
			return layer;
		}
		private enum EPrimaryFrame {
			Stand
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "Stand" };
		}
		internal override int[] PrimaryFrameMirroring() {
			return new int[] { 0 };
		}
		private enum ESecondaryFrame {
			Ground,
			Hover
		}
		internal override string[] SecondaryFrameNames() {
			return new string[] { "Ground", "Hover" };
		}
		internal override int[] SecondaryFrameMirroring() {
			return new int[] { 0, 1 };
		}
		private enum ETertiaryFrame {
			None,
			Cannon,
			Rocket,
			Laser,
			Plasma,
			Launcher
		}
		internal override string[] TertiaryFrameNames() {
			return new string[] { "None", "Cannon", "Rocket", "Laser", "Plasma", "Launcher" };
		}
		internal override int[] TertiaryFrameMirroring() {
			return new int[] { 1, 2, 3, 4, 5 };
		}

		internal override SmartLayerType SmartLayerSupported() {
			return SmartLayerType.HWP;
		}
		internal override BitmapPalette DefaultSpriteSheetPalette() {
			return Palettes.FromName("UFO Battlescape");
		}
	}
}
