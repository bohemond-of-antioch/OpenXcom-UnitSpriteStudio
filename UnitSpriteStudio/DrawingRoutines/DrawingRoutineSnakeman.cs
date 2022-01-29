using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineSnakeman : DrawingRoutine {
		internal override bool CanHoldItems() {
			return true;
		}

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (320, 560);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (32, 40);
		}

		private static readonly ELayer[,] LayerDrawingOrder={
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.LeftArm,ELayer.RightArm,ELayer.Legs,ELayer.Torso}
		};
		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, DrawingContext drawingContext,int highlightedLayer) {
			if (metadata.PrimaryFrame==(int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			} else {
				for (int l=0;l<4;l++) {
					if (highlightedLayer != -1 && (int)LayerDrawingOrder[metadata.Direction, l] != highlightedLayer) continue;
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction,l]);
					ImageSource image = sprite.GetFrame(frameInfo.Index);
					drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
				}
			}
		}
		internal override Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(32,40);
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				BitmapSource image = sprite.GetFrame(frameInfo.Index);
				byte[] pixels = new byte[32 * 40];
				image.CopyPixels(pixels, 32, 0);
				for (int f=0;f<pixels.Length;f++) {
					if (pixels[f]>0) {
						outline.AddPoint((f % 32+frameInfo.OffsetX, f / 32 + frameInfo.OffsetY));
					}
				}
			} else {
				for (int l = 0; l < 4; l++) {
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction, l]);
					BitmapSource image = sprite.GetFrame(frameInfo.Index);
					byte[] pixels = new byte[32 * 40];
					image.CopyPixels(pixels, 32, 0);
					for (int f = 0; f < pixels.Length; f++) {
						if (pixels[f] > 0) {
							outline.AddPoint((f % 32 + frameInfo.OffsetX, f / 32 + frameInfo.OffsetY));
						}
					}
				}
			}
			return outline;
		}

		private const int FRAME_STAND_LEFT_ARM = 0;
		private const int FRAME_STAND_RIGHT_ARM = 8;
		private const int FRAME_STAND_LEGS = 16;
		private const int FRAME_KNEEL_LEGS = 24;
		private const int FRAME_WALK_LEGS = 32;
		private const int FRAME_DEATH = 96;
		private const int FRAME_TORSO = 24;
		private const int FRAME_RIGHT_ARM_HOLDING = 99;
		private const int FRAME_LEFT_ARM_HOLDING = 107;
		private const int FRAME_RIGHT_ARM_TWO_HANDED = 115;
		private const int FRAME_RIGHT_ARM_SHOOT = 123;

		private static readonly int[] OFFSET_Y_TORSO_WALK = { 3, 3, 2, 1, 0, 0, 1, 2 };
		private static readonly int[] OFFSET_X_TORSO_WALK_A = { 0, 0, 1, 2, 3, 3, 2, 1 };
		private static readonly int[] OFFSET_X_TORSO_WALK_B = { 0, 0, -1, -2, -3, -3, -2, -1 };

		private (int X, int Y) WalkOffsets(FrameMetadata metadata) {
			int offsetX = 0;
			int offsetY = OFFSET_Y_TORSO_WALK[metadata.AnimationFrame];
			if (metadata.Direction < 3) {
				offsetX = OFFSET_X_TORSO_WALK_A[metadata.AnimationFrame];
			} else if (metadata.Direction > 3 && metadata.Direction < 7) {
				offsetX = OFFSET_X_TORSO_WALK_B[metadata.AnimationFrame];
			}
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
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.FreeHands:
						case ESecondaryFrame.RightHandItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									(offsetX, offsetY) = WalkOffsets(metadata);
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, offsetX, offsetY);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.LeftHandItem:
						case ESecondaryFrame.TwoItems:
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									(offsetX, offsetY) = WalkOffsets(metadata);
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, offsetX, offsetY);
								default:
									throw new Exception("No such frame!");
							}
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.RightArm:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.FreeHands:
						case ESecondaryFrame.LeftHandItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									(offsetX, offsetY) = WalkOffsets(metadata);
									return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, offsetX, offsetY);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.RightHandItem:
						case ESecondaryFrame.TwoItems:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									(offsetX, offsetY) = WalkOffsets(metadata);
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, offsetX, offsetY);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_SHOOT + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									(offsetX, offsetY) = WalkOffsets(metadata);
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, offsetX, offsetY);
								default:
									throw new Exception("No such frame!");
							}
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.Legs:
					switch ((EPrimaryFrame)metadata.PrimaryFrame) {
						case EPrimaryFrame.Stand:
						case EPrimaryFrame.Shoot:
							return new LayerFrameInfo(FRAME_STAND_LEGS + metadata.Direction, 0, 0);
						case EPrimaryFrame.Walk:
							return new LayerFrameInfo(FRAME_WALK_LEGS + metadata.Direction * 8 + metadata.AnimationFrame, 0, 0);
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
					return 3;
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
			Shoot,
			Death
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "Stand", "Walk", "Shoot", "Death" };
		}
		internal override int[] PrimaryFrameMirroring() {
			return new int[] { 0, 1, 2, 3 };
		}
		private enum ESecondaryFrame {
			FreeHands,
			LeftHandItem,
			RightHandItem,
			TwoItems,
			TwoHandedItem
		}
		internal override string[] SecondaryFrameNames() {
			return new string[] { "Free hands", "Left hand item", "Right hand item", "Two items", "Two-handed item" };
		}
		internal override int[] SecondaryFrameMirroring() {
			return new int[] { 0, 3 };
		}
		private enum ETertiaryFrame {
			None
		}
		internal override string[] TertiaryFrameNames() {
			return new string[] { "None" };
		}
		internal override int[] TertiaryFrameMirroring() {
			return new int[] { 0};
		}
		internal override BitmapPalette DefaultSpriteSheetPalette() {
			return Palettes.FromName("UFO Battlescape");
		}
	}
}
