using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineFloater : DrawingRoutine {
		internal override bool CanHoldItems() {
			return true;
		}

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (512, 280);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (32, 40);
		}

		private static readonly ELayer[,] LayerDrawingOrder ={
			{ELayer.RightItem,ELayer.LeftItem,ELayer.LeftArm,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Torso,ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.LeftArm,ELayer.Torso,ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.Torso,ELayer.LeftArm,ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.Torso,ELayer.LeftArm,ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.RightArm,ELayer.Torso,ELayer.LeftArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem,ELayer.LeftArm,ELayer.Torso}
		};
		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			} else {
				for (int l = 0; l < 5; l++) {
					if (highlightedLayer != -1 && (int)LayerDrawingOrder[metadata.Direction, l] != highlightedLayer) continue;
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction, l]);
					if (frameInfo.Target == LayerFrameInfo.ETarget.Unit) {
						ImageSource image = sprite.GetFrame(frameInfo.Index);
						drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
					} else if (frameInfo.Target == LayerFrameInfo.ETarget.Item && itemSprite != null) {
						ImageSource image = itemSprite.GetFrame(frameInfo.Index);
						drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
					}
				}
			}
		}
		internal override Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata) {
			Selection outline = new Selection(32, 40);
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				BitmapSource image = sprite.GetFrame(frameInfo.Index);
				byte[] pixels = new byte[32 * 40];
				image.CopyPixels(pixels, 32, 0);
				for (int f = 0; f < pixels.Length; f++) {
					if (pixels[f] > 0) {
						outline.AddPoint((f % 32 + frameInfo.OffsetX, f / 32 + frameInfo.OffsetY));
					}
				}
			} else {
				for (int l = 0; l < 5; l++) {
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder[metadata.Direction, l]);
					if (frameInfo.Target == LayerFrameInfo.ETarget.Unit) {
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
			}
			return outline;
		}

		private const int FRAME_STAND_LEFT_ARM = 8;
		private const int FRAME_STAND_RIGHT_ARM = 0;
		private const int FRAME_STAND = 16;
		private const int FRAME_WALK = 24;
		private const int FRAME_DEATH = 64;
		private const int FRAME_RIGHT_ARM_HOLDING = 91;
		private const int FRAME_LEFT_ARM_HOLDING = 67;
		private const int FRAME_RIGHT_ARM_TWO_HANDED = 75;
		private const int FRAME_RIGHT_ARM_SHOOT = 83;

		private static readonly int[] OFFSET_Y_TORSO_WALK = { 1, 0, -1, 0, 1, 0, -1, 0 };

		private static readonly int[] ITEM_RIGHT_HAND_AIMING_OFFSET_X = { 8, 10, 7, 4, -9, -11, -7, -3 };
		private static readonly int[] ITEM_RIGHT_HAND_AIMING_OFFSET_Y = { -6, -3, 0, 2, 0, -4, -7, -9 };
		private static readonly int[] ITEM_LEFT_HAND_OFFSET_X = { -8, 3, 7, 13, 6, -3, -5, -13 };
		private static readonly int[] ITEM_LEFT_HAND_OFFSET_Y = { 1, -4, -1, 0, 3, 3, 5, 0 };
		private static readonly int[] ITEM_LEFT_HAND_AIMING_OFFSET_X = { 0, 6, 6, 12, -4, -5, -5, -13 };
		private static readonly int[] ITEM_LEFT_HAND_AIMING_OFFSET_Y = { -4, -4, -1, 0, 5, 0, 1, 0 };

		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) { // Death animation
				return new LayerFrameInfo(FRAME_DEATH + metadata.AnimationFrame, 0, 0);
			}
			switch ((ELayer)layer) {
				case ELayer.Torso:
					switch ((EPrimaryFrame)metadata.PrimaryFrame) {
						case EPrimaryFrame.Stand:
						case EPrimaryFrame.Shoot:
							return new LayerFrameInfo(FRAME_STAND + metadata.Direction, 0, 0);
						case EPrimaryFrame.Walk:
							return new LayerFrameInfo(FRAME_WALK + metadata.Direction * 5 + (int)(metadata.AnimationFrame / 1.6), 0, 0);
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.LeftArm:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.FreeHands:
						case ESecondaryFrame.RightHandItem:
							return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, 0);
						case ESecondaryFrame.LeftHandItem:
						case ESecondaryFrame.TwoItems:
						case ESecondaryFrame.TwoHandedItem:
							return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, 0);
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.RightArm:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.FreeHands:
						case ESecondaryFrame.LeftHandItem:
							return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, 0);
						case ESecondaryFrame.RightHandItem:
						case ESecondaryFrame.TwoItems:
							return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, 0);
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, 0);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_SHOOT + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, 0);
								default:
									throw new Exception("No such frame!");
							}
						default:
							throw new Exception("No such frame!");
					}
				case ELayer.RightItem:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.RightHandItem:
						case ESecondaryFrame.TwoItems:
							return new LayerFrameInfo(metadata.Direction, 0, 0, LayerFrameInfo.ETarget.Item);
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
								case EPrimaryFrame.Walk:
									return new LayerFrameInfo(metadata.Direction, 0, 0, LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo((metadata.Direction + 2) % 8, ITEM_RIGHT_HAND_AIMING_OFFSET_X[metadata.Direction], ITEM_RIGHT_HAND_AIMING_OFFSET_Y[metadata.Direction], LayerFrameInfo.ETarget.Item);
								default:
									throw new Exception("No such frame!");
							}
						default:
							return new LayerFrameInfo(0, 0, 0, LayerFrameInfo.ETarget.None);
					}
				case ELayer.LeftItem:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.LeftHandItem:
						case ESecondaryFrame.TwoItems:
							return new LayerFrameInfo(metadata.Direction, ITEM_LEFT_HAND_OFFSET_X[metadata.Direction], ITEM_LEFT_HAND_OFFSET_Y[metadata.Direction], LayerFrameInfo.ETarget.Item);
						default:
							return new LayerFrameInfo(0, 0, 0, LayerFrameInfo.ETarget.None);
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
			Legs,
			LeftItem,
			RightItem
		}
		internal override string[] LayerNames() {
			return new string[] { "Torso", "Left arm", "Right arm" };
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
			return new int[] { 0 };
		}
		internal override BitmapPalette DefaultSpriteSheetPalette() {
			return Palettes.FromName("UFO Battlescape");
		}
		internal override bool ItemSupported() {
			return true;
		}
	}
}
