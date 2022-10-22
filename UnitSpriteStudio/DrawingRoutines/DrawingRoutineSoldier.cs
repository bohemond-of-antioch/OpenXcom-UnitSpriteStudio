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
		private int StandHeight = DEFAULT_STAND_HEIGHT;
		private int DeathFrames = 3;
		internal override bool CanHoldItems() {
			return true;
		}

		internal override (int Width, int Height) DefaultSpriteSheetSize() {
			return (512, 720);
		}
		internal override (int Width, int Height) CompositeImageSize() {
			return (32, 40);
		}

		private const int DrawLayerCount = 6;

		private static readonly ELayer[,] LayerDrawingOrderRegular = {
			{ELayer.RightItem,ELayer.LeftItem,ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.LeftItem,ELayer.Torso,ELayer.RightItem,ELayer.RightArm},
			{ELayer.LeftArm,ELayer.Legs,ELayer.Torso,ELayer.LeftItem,ELayer.RightItem,ELayer.RightArm},
			{ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.Legs,ELayer.RightArm,ELayer.Torso,ELayer.LeftArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.RightArm,ELayer.Legs,ELayer.RightItem,ELayer.LeftItem,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem,ELayer.Legs,ELayer.Torso,ELayer.LeftArm},
			{ELayer.RightItem,ELayer.LeftItem,ELayer.LeftArm,ELayer.RightArm,ELayer.Legs,ELayer.Torso}
		};

		private static readonly ELayer[,] LayerDrawingOrderItems = {
			{ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightItem,ELayer.LeftItem,ELayer.RightArm},
			{ELayer.RightArm,ELayer.Legs,ELayer.Torso,ELayer.LeftArm,ELayer.RightItem,ELayer.LeftItem},
			{ELayer.RightArm,ELayer.RightItem,ELayer.LeftItem,ELayer.LeftArm,ELayer.Legs,ELayer.Torso}
		};

		private ELayer LayerDrawingOrder(FrameMetadata metadata, int Order) {
			switch (metadata.Direction) {
				case 3:
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Shoot) {
						return LayerDrawingOrderRegular[3, Order];
					} else if (metadata.SecondaryFrame == (int)ESecondaryFrame.TwoHandedItem) {
						return LayerDrawingOrderItems[0, Order];
					} else {
						return LayerDrawingOrderRegular[3, Order];
					}
				case 5:
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Shoot) {
						return LayerDrawingOrderRegular[5, Order];
					} else if (metadata.SecondaryFrame == (int)ESecondaryFrame.TwoHandedItem) {
						return LayerDrawingOrderItems[1, Order];
					} else {
						return LayerDrawingOrderRegular[5, Order];
					}
				case 7:
					if (metadata.PrimaryFrame == (int)EPrimaryFrame.Shoot) {
						return LayerDrawingOrderRegular[7, Order];
					} else if (metadata.SecondaryFrame == (int)ESecondaryFrame.TwoHandedItem) {
						return LayerDrawingOrderItems[2, Order];
					} else {
						return LayerDrawingOrderRegular[7, Order];
					}
				default:
					return LayerDrawingOrderRegular[metadata.Direction, Order];
			}
		}

		internal override void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) {
				LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)ELayer.Torso);
				ImageSource image = sprite.GetFrame(frameInfo.Index);
				drawingContext.DrawImage(image, new Rect(frameInfo.OffsetX, frameInfo.OffsetY, 32, 40));
			} else {
				for (int l = 0; l < DrawLayerCount; l++) {
					if (highlightedLayer != -1 && (int)LayerDrawingOrder(metadata, l) != highlightedLayer) continue;
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder(metadata, l));
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
				for (int l = 0; l < DrawLayerCount; l++) {
					LayerFrameInfo frameInfo = GetLayerFrame(metadata, (int)LayerDrawingOrder(metadata, l));
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

		private const int DEFAULT_STAND_HEIGHT = 22;

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

		private static readonly int[] ITEM_RIGHT_HAND_AIMING_OFFSET_X = { 8, 10, 7, 4, -9, -11, -7, -3 };
		private static readonly int[] ITEM_RIGHT_HAND_AIMING_OFFSET_Y = { -6, -3, 0, 2, 0, -4, -7, -9 };
		private static readonly int[] ITEM_LEFT_HAND_OFFSET_X = { -8, 3, 5, 12, 6, -1, -5, -13 };
		private static readonly int[] ITEM_LEFT_HAND_OFFSET_Y = { 1, -4, -2, 0, 3, 3, 5, 0 };

		private const int OFFSET_Y_KNEEL = 4;
		private static readonly int[] OFFSET_Y_TORSO_WALK = { 1, 0, -1, 0, 1, 0, -1, 0 };


		internal override LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer) {
			if (metadata.PrimaryFrame == (int)EPrimaryFrame.Death) { // Death animation
				return new LayerFrameInfo(FRAME_DEATH + metadata.AnimationFrame, 0, 0);
			}
			int offsetY;
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
				case ELayer.RightItem:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.RightHandItem:
						case ESecondaryFrame.TwoItems:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(metadata.Direction, 0, DEFAULT_STAND_HEIGHT - StandHeight, LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(metadata.Direction, 0, OFFSET_Y_KNEEL + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(metadata.Direction, 0, OFFSET_Y_TORSO_WALK[metadata.AnimationFrame] + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								default:
									throw new Exception("No such frame!");
							}
						case ESecondaryFrame.TwoHandedItem:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
									return new LayerFrameInfo(metadata.Direction, 0, DEFAULT_STAND_HEIGHT - StandHeight, LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(metadata.Direction, 0, OFFSET_Y_TORSO_WALK[metadata.AnimationFrame] + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(metadata.Direction, 0, OFFSET_Y_KNEEL + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo((metadata.Direction + 2) % 8, ITEM_RIGHT_HAND_AIMING_OFFSET_X[metadata.Direction], ITEM_RIGHT_HAND_AIMING_OFFSET_Y[metadata.Direction] + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								default:
									throw new Exception("No such frame!");
							}
						default:
							return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
					}
				case ELayer.LeftItem:
					switch ((ESecondaryFrame)metadata.SecondaryFrame) {
						case ESecondaryFrame.LeftHandItem:
						case ESecondaryFrame.TwoItems:
							switch ((EPrimaryFrame)metadata.PrimaryFrame) {
								case EPrimaryFrame.Stand:
								case EPrimaryFrame.Shoot:
									return new LayerFrameInfo(metadata.Direction, ITEM_LEFT_HAND_OFFSET_X[metadata.Direction], ITEM_LEFT_HAND_OFFSET_Y[metadata.Direction] + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Kneel:
									return new LayerFrameInfo(metadata.Direction, ITEM_LEFT_HAND_OFFSET_X[metadata.Direction], ITEM_LEFT_HAND_OFFSET_Y[metadata.Direction] + OFFSET_Y_KNEEL + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								case EPrimaryFrame.Walk:
								case EPrimaryFrame.Float:
									return new LayerFrameInfo(metadata.Direction, ITEM_LEFT_HAND_OFFSET_X[metadata.Direction], ITEM_LEFT_HAND_OFFSET_Y[metadata.Direction] + OFFSET_Y_TORSO_WALK[metadata.AnimationFrame] + (DEFAULT_STAND_HEIGHT - StandHeight), LayerFrameInfo.ETarget.Item);
								default:
									throw new Exception("No such frame!");
							}
						default:
							return new LayerFrameInfo(-1, 0, 0, LayerFrameInfo.ETarget.None);
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
			Legs,
			RightItem,
			LeftItem
		}
		internal override string[] LayerNames() {
			return new string[] { "Torso", "Left arm", "Right arm", "Legs", "Right Item" };
		}
		internal override int ChangeArmsLayer(int layer) {
			if (layer == 1) return 2;
			if (layer == 2) return 1;
			return layer;
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
		internal override int[] PrimaryFrameMirroring() {
			return new int[] { 0, 1, 2, 3, 4 };
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
			Male,
			Female
		}
		internal override string[] TertiaryFrameNames() {
			return new string[] { "Male", "Female" };
		}
		internal override int[] TertiaryFrameMirroring() {
			return new int[] { 0, 1 };
		}
		internal override BitmapPalette DefaultSpriteSheetPalette() {
			return Palettes.FromName("UFO Battlescape");
		}
		internal override bool ItemSupported() {
			return true;
		}
		internal override List<string> SupportedRuleValues() {
			return new List<string>() { "deathFrames", "standHeight" };
		}
		internal override void SetRuleValue(string Name, string Value) {
			switch (Name) {
				case "deathFrames":
					DeathFrames = int.Parse(Value);
					break;
				case "standHeight":
					StandHeight = int.Parse(Value);
					break;
			}
		}
		internal override string GetRuleValue(string Name) {
			switch (Name) {
				case "deathFrames":
					return DeathFrames.ToString();
				case "standHeight":
					return StandHeight.ToString();
			}
			return base.GetRuleValue(Name);
		}
	}
}
