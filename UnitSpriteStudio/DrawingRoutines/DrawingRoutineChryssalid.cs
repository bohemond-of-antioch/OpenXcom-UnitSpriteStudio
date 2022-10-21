using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineChryssalid : DrawingRoutine {
		private int DeathFrames = 3;
		private const int COMPOSITE_IMAGE_WIDTH = 32;
		private const int COMPOSITE_IMAGE_HEIGHT = 40;
		internal override bool CanHoldItems() {
			return true;
		}

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (256, 1160);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
		}

		private static readonly ELayer[,] LayerDrawingOrder ={
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.LeftArm,ELayer.RightArm,ELayer.Legs,ELayer.Torso}
		};
		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			} else {
				for (int l = 0; l < 4; l++) {
					if (highlightedLayer != -1 && (int)LayerDrawingOrder[metadata.Direction, l] != highlightedLayer) continue;
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction, l]);
					ImageSource image = sprite.GetFrame(frameInfo.Index);
					drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
				}
			}
		}
		internal override Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(COMPOSITE_IMAGE_WIDTH, COMPOSITE_IMAGE_HEIGHT);
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				BitmapSource image = sprite.GetFrame(frameInfo.Index);
				byte[] pixels = new byte[COMPOSITE_IMAGE_WIDTH * COMPOSITE_IMAGE_HEIGHT];
				image.CopyPixels(pixels, COMPOSITE_IMAGE_WIDTH, 0);
				for (int f = 0; f < pixels.Length; f++) {
					if (pixels[f] > 0) {
						outline.AddPoint((f % COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetX, f / COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetY));
					}
				}
			} else {
				for (int l = 0; l < 4; l++) {
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction, l]);
					BitmapSource image = sprite.GetFrame(frameInfo.Index);
					byte[] pixels = new byte[COMPOSITE_IMAGE_WIDTH * COMPOSITE_IMAGE_HEIGHT];
					image.CopyPixels(pixels, COMPOSITE_IMAGE_WIDTH, 0);
					for (int f = 0; f < pixels.Length; f++) {
						if (pixels[f] > 0) {
							outline.AddPoint((f % COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetX, f / COMPOSITE_IMAGE_WIDTH + frameInfo.OffsetY));
						}
					}
				}
			}
			return outline;
		}

		private const int FRAME_STAND_LEFT_ARM = 0;
		private const int FRAME_STAND_LEFT_ARM_WALK = 32;
		private const int FRAME_STAND_RIGHT_ARM = 8;
		private const int FRAME_STAND_RIGHT_ARM_WALK = 40;
		private const int FRAME_STAND_LEGS = 16;
		private const int FRAME_WALK_LEGS = 48;
		private const int FRAME_DEATH = 224;
		private const int FRAME_TORSO = 24;

		private static readonly int[] OFFSET_Y_TORSO_WALK = { 1, 0, -1, 0, 1, 0, -1, 0 };

		private (int X, int Y) WalkOffsets(FrameMetadata metadata) {
			int offsetX = 0;
			int offsetY = OFFSET_Y_TORSO_WALK[metadata.AnimationFrame];
			return (offsetX, offsetY);
		}

		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) { // Death animation
				return new LayerFrameInfo(FRAME_DEATH + metadata.AnimationFrame, 0, 0);
			}
			int offsetX, offsetY;
			switch ((ELayer)layer) {
				case ELayer.Torso:
					offsetY = 0;
					offsetX = 0;
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Walk) {
						(offsetX, offsetY) = WalkOffsets(metadata);
					}
					return new LayerFrameInfo(FRAME_TORSO + metadata.Direction, offsetX, offsetY);

				case ELayer.LeftArm:
					switch ((EPrimaryFrame)metadata.PrimaryFrame) {
						case EPrimaryFrame.Stand:
							return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction , 0, 0);
						case EPrimaryFrame.Walk:
							(offsetX, offsetY) = WalkOffsets(metadata);
							return new LayerFrameInfo(FRAME_STAND_LEFT_ARM_WALK + metadata.Direction * 24 + metadata.AnimationFrame, offsetX, offsetY);
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.RightArm:
					switch ((EPrimaryFrame)metadata.PrimaryFrame) {
						case EPrimaryFrame.Stand:
							return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, 0);
						case EPrimaryFrame.Walk:
							(offsetX, offsetY) = WalkOffsets(metadata);
							return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM_WALK + metadata.Direction * 24 + metadata.AnimationFrame, offsetX, offsetY);
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.Legs:
					switch ((EPrimaryFrame)metadata.PrimaryFrame) {
						case EPrimaryFrame.Stand:
							return new LayerFrameInfo(FRAME_STAND_LEGS + metadata.Direction, 0, 0);
						case EPrimaryFrame.Walk:
							return new LayerFrameInfo(FRAME_WALK_LEGS + metadata.Direction * 24 + metadata.AnimationFrame, 0, 0);
						default:
							throw new Exception("No such frame!");
					}
				default:
					throw new Exception("No such layer!");
			}
		}

		internal override int GetAnimationFrameCount(FrameMetadata metadata) {
			switch ((EPrimaryFrame)metadata.PrimaryFrame) {
				case EPrimaryFrame.Walk:
					return 8;
				case EPrimaryFrame.Death:
					return DeathFrames;
				default:
					return 1;
			}
		}

		private enum ELayer {
			Torso,
			LeftArm,
			RightArm,
			Legs
		}
		internal override string[] LayerNames() {
			return new string[] { "Torso", "Left arm", "Right arm", "Legs" };
		}
		internal override int ChangeArmsLayer(int layer) {
			if (layer == 1) return 2;
			if (layer == 2) return 1;
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
		internal override List<string> SupportedRuleValues() {
			return new List<string>() { "deathFrames" };
		}
		internal override void SetRuleValue(string Name, string Value) {
			switch (Name) {
				case "deathFrames":
					DeathFrames = int.Parse(Value);
					break;
			}
		}
		internal override string GetRuleValue(string Name) {
			switch (Name) {
				case "deathFrames":
					return DeathFrames.ToString();
			}
			return base.GetRuleValue(Name);
		}
	}
}
