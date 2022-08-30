using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitSpriteStudio.FrameProcessing {
	class MirroredDirectionsOperation {
		internal struct Direction {
			internal int Source;
			internal int Destination;
			internal bool Active;
			internal bool Mirror;
			internal bool Flip;
			internal bool ChangeArms;
		}

		internal bool EntirePrimaryGroup, EntireSecondaryGroup, EntireTertiaryGroup;
		internal bool EntireAnimation;
		internal Direction[] Directions = new Direction[8];

		public MirroredDirectionsOperation() {
			ApplyDefaultSettings();
		}

		public void ApplyDefaultSettings() {
			for (int f = 0; f < 8; f++) {
				Directions[f].Source = f;
				Directions[f].Destination = 0;
				Directions[f].Active = false;
				Directions[f].Mirror = true;
				Directions[f].Flip = false;
				Directions[f].ChangeArms = true;
			}
		}

		public void ApplyPresetRightToLeft() {
			ApplyDefaultSettings();
			Directions[0].Destination = 6;
			Directions[0].Active = true;
			Directions[1].Destination = 5;
			Directions[1].Active = true;
			Directions[2].Destination = 4;
			Directions[2].Active = true;
		}
		public void ApplyPresetLeftToRight() {
			ApplyDefaultSettings();
			Directions[6].Destination = 0;
			Directions[6].Active = true;
			Directions[5].Destination = 1;
			Directions[5].Active = true;
			Directions[4].Destination = 2;
			Directions[4].Active = true;
		}
		public void ApplyPresetFixSwappedArms() {
			ApplyDefaultSettings();
			Directions[4].Destination = 4;
			Directions[4].Active = true;
			Directions[4].Mirror = false;
			Directions[4].ChangeArms = true;
			EntireAnimation = true;
		}

		private void ProcessComposite(List<DrawingRoutines.FrameMetadata> metadataList, SpriteSheet sourceSpriteSheet,SpriteSheet destinationSpriteSheet) {
			DrawingRoutines.DrawingRoutine routine = sourceSpriteSheet.drawingRoutine;
			int layerCount = routine.LayerNames().Length;
			for (int d = 0; d < 8; d++) {
				if (!Directions[d].Active) continue;
				Dictionary<int, int> frameMappings = new Dictionary<int, int>();
				foreach (var metadata in metadataList) {
					for (int layer = 0; layer < layerCount; layer++) {
						metadata.Direction = d;
						DrawingRoutines.DrawingRoutine.LayerFrameInfo sourceFrameInfo = routine.GetLayerFrame(metadata, layer);
						int destinationLayer = layer;
						if (Directions[d].ChangeArms) destinationLayer = routine.ChangeArmsLayer(destinationLayer);
						metadata.Direction = Directions[d].Destination;
						DrawingRoutines.DrawingRoutine.LayerFrameInfo destinationFrameInfo = routine.GetLayerFrame(metadata, destinationLayer);
						frameMappings[sourceFrameInfo.Index] = destinationFrameInfo.Index;
					}
				}
				foreach (KeyValuePair<int, int> frameMapping in frameMappings) {
					byte[] framePixelData = sourceSpriteSheet.frameSource.GetFramePixelData(frameMapping.Key);
					if (Directions[d].Mirror) ApplyMirror(ref framePixelData);
					if (Directions[d].Flip) ApplyFlip(ref framePixelData);
					destinationSpriteSheet.frameSource.SetFramePixelData(frameMapping.Value, framePixelData);
				}
			}
		}

		private void ApplyFlip(ref byte[] framePixelData) {
			for (int x = 0; x < 32; x++) {
				for (int y = 0; y < 20; y++) {
					byte temp;
					temp = framePixelData[x + y * 32];
					framePixelData[x + y * 32] = framePixelData[x + (39 - y) * 32];
					framePixelData[x + (39 - y) * 32] = temp;
				}
			}
		}

		private void ApplyMirror(ref byte[] framePixelData) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 40; y++) {
					byte temp;
					temp = framePixelData[x + y * 32];
					framePixelData[x + y * 32] = framePixelData[(31 - x) + y * 32];
					framePixelData[(31 - x) + y * 32] = temp;
				}
			}
		}

		public void Run(DrawingRoutines.FrameMetadata mainWindowMetadata, SpriteSheet spriteSheet) {
			List<DrawingRoutines.FrameMetadata> metadataList = new List<DrawingRoutines.FrameMetadata>();
			int[] primaryList, secondaryList, tertiaryList;
			if (EntirePrimaryGroup) {
				primaryList = spriteSheet.drawingRoutine.PrimaryFrameMirroring();
			} else {
				primaryList = new int[] { mainWindowMetadata.PrimaryFrame };
			}
			if (EntireSecondaryGroup) {
				secondaryList = spriteSheet.drawingRoutine.SecondaryFrameMirroring();
			} else {
				secondaryList = new int[] { mainWindowMetadata.SecondaryFrame };
			}
			if (EntireTertiaryGroup) {
				tertiaryList = spriteSheet.drawingRoutine.TertiaryFrameMirroring();
			} else {
				tertiaryList = new int[] { mainWindowMetadata.TertiaryFrame };
			}
			for (int p = 0; p < primaryList.Length; p++) {
				for (int s = 0; s < secondaryList.Length; s++) {
					for (int t = 0; t < tertiaryList.Length; t++) {
						DrawingRoutines.FrameMetadata metadataForInfo = new DrawingRoutines.FrameMetadata();
						metadataForInfo.PrimaryFrame = primaryList[p];
						metadataForInfo.SecondaryFrame = secondaryList[s];
						metadataForInfo.TertiaryFrame = tertiaryList[t];
						int animationFrameCount = spriteSheet.drawingRoutine.GetAnimationFrameCount(metadataForInfo);
						int animationStart, animationEnd;
						if (EntireAnimation) {
							animationStart = 0;
							animationEnd = animationFrameCount;
						} else {
							animationStart = Math.Min(animationFrameCount-1, mainWindowMetadata.AnimationFrame);
							animationEnd = Math.Min(animationFrameCount, mainWindowMetadata.AnimationFrame+1);
						}
						for (int a = animationStart; a < animationEnd; a++) {
							DrawingRoutines.FrameMetadata metadataToProcess = new DrawingRoutines.FrameMetadata();
							metadataToProcess.PrimaryFrame = primaryList[p];
							metadataToProcess.SecondaryFrame = secondaryList[s];
							metadataToProcess.TertiaryFrame = tertiaryList[t];
							metadataToProcess.Direction = -1;
							metadataToProcess.AnimationFrame = a;
							metadataList.Add(metadataToProcess);
						}
					}
				}
			}
			SpriteSheet sourceSpriteSheet = new SpriteSheet(spriteSheet);
			ProcessComposite(metadataList,sourceSpriteSheet, spriteSheet);
		}
	}
}
