using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	class DrawingRoutineSoldier : DrawingRoutine {
		internal override bool CanHoldItems() {
			return true;
		}

		internal override (int Width, int Height) CompositeImageSize() {
			return (32, 40);
		}

		private static readonly ELayer[,] LayerDrawingOrder={
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightArm},
			{ELayer.Legs,ELayer.RightArm,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.LeftArm,ELayer.RightArm,ELayer.Legs,ELayer.Torso}
		};
		internal override void DrawCompositeImage(SpriteSheet.FrameSource sprite, FrameMetadata metadata, DrawingContext drawingContext,int highlightedLayer) {
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

		private const int FRAME_STAND_LEFT_ARM = 0;
		private const int FRAME_STAND_RIGHT_ARM = 8;
		private const int FRAME_STAND_LEGS = 16;
		private const int FRAME_KNEEL_LEGS = 24;
		private const int FRAME_WALK_LEGS = 56;
		private const int FRAME_WALK_LEFT_ARM = 40;
		private const int FRAME_WALK_RIGHT_ARM = 48;
		private const int FRAME_DEATH = 264;
		private const int FRAME_TORSO_MALE = 32;
		private const int FRAME_TORSO_FEMALE = 267;
		private const int FRAME_RIGHT_ARM_HOLDING = 232;
		private const int FRAME_LEFT_ARM_HOLDING = 240;
		private const int FRAME_RIGHT_ARM_TWO_HANDED = 248;
		private const int FRAME_RIGHT_ARM_SHOOT = 256;
		private const int FRAME_LEGS_FLOAT = 275;

		private const int OFFSET_Y_KNEEL = 4;
		private static readonly int[] OFFSET_Y_TORSO_WALK = { 1, 0, -1, 0, 1, 0, -1, 0 };


		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) { // Death animation
				return new LayerFrameInfo(FRAME_DEATH + metadata.AnimationFrame, 0, 0);
			}
			int offsetX, offsetY;
			switch ((ELayer)layer) {
				case ELayer.Torso:
					offsetY = 0;
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Walk) offsetY = OFFSET_Y_TORSO_WALK[metadata.AnimationFrame];
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Float) offsetY = OFFSET_Y_TORSO_WALK[metadata.AnimationFrame];
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Kneel) offsetY = OFFSET_Y_KNEEL;
					switch ((ETertiaryFrame)metadata.TertiaryFrame) {
						case ETertiaryFrame.Male:
							return new LayerFrameInfo(FRAME_TORSO_MALE + metadata.Direction, 0, offsetY);
						case ETertiaryFrame.Female:
							return new LayerFrameInfo(FRAME_TORSO_FEMALE + metadata.Direction, 0, offsetY);
						default:
							throw new Exception("No such frame!");
					}

				case ELayer.LeftArm:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.FreeHands:
						case ESecondaryFrame.RightHandItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, OFFSET_Y_KNEEL);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_STAND_LEFT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(FRAME_WALK_LEFT_ARM + metadata.Direction * 24 + metadata.AnimationFrame, 0, 0);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.LeftHandItem:
						case ESecondaryFrame.TwoItems:
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, OFFSET_Y_KNEEL);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(FRAME_LEFT_ARM_HOLDING + metadata.Direction, 0, OFFSET_Y_TORSO_WALK[metadata.AnimationFrame]);
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
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, OFFSET_Y_KNEEL);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_STAND_RIGHT_ARM + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(FRAME_WALK_RIGHT_ARM + metadata.Direction * 24 + metadata.AnimationFrame, 0, 0);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.RightHandItem:
						case ESecondaryFrame.TwoItems:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, OFFSET_Y_KNEEL);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_HOLDING + metadata.Direction, 0, OFFSET_Y_TORSO_WALK[metadata.AnimationFrame]);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, 0);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, OFFSET_Y_KNEEL);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_SHOOT + metadata.Direction, 0, 0);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(FRAME_RIGHT_ARM_TWO_HANDED + metadata.Direction, 0, OFFSET_Y_TORSO_WALK[metadata.AnimationFrame]);
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
						case EPrimaryFrame.Kneel:
							return new LayerFrameInfo(FRAME_KNEEL_LEGS + metadata.Direction, 0, 0);
						case EPrimaryFrame.Walk:
							return new LayerFrameInfo(FRAME_WALK_LEGS + metadata.Direction * 24 + metadata.AnimationFrame, 0, 0);
						case EPrimaryFrame.Float:
							return new LayerFrameInfo(FRAME_LEGS_FLOAT + metadata.Direction, 0, 0);
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
		private enum EPrimaryFrame {
			Stand,
			Kneel,
			Walk,
			Float,
			Shoot,
			Death
		}
		internal override string[] PrimaryFrameNames() {
			return new string[] { "Stand", "Kneel", "Walk", "Float", "Shoot", "Death" };
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
		private enum ETertiaryFrame {
			Male,
			Female
		}
		internal override string[] TertiaryFrameNames() {
			return new string[] { "Male", "Female" };
		}
	}
}
