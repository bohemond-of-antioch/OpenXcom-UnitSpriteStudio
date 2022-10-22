using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio {
	class UndoSystem {
		class SavedFrame {
			byte[] PixelData;
			int FrameIndex;

			internal SavedFrame(int frameIndex, UnitSpriteSheet spriteSheet) {
				PixelData = spriteSheet.frameSource.GetFramePixelData(frameIndex);
				FrameIndex = frameIndex;
			}

			internal void Apply(UnitSpriteSheet spriteSheet) {
				spriteSheet.frameSource.SetFramePixelData(FrameIndex, PixelData);
			}
		}
		class PixelData {
			internal readonly int FrameIndex;
			internal readonly SavedFrame FrameData;
			internal readonly WriteableBitmap EntireSprite;
			public PixelData(int frameIndex, SavedFrame frameData, WriteableBitmap entireSprite) {
				FrameIndex = frameIndex;
				FrameData = frameData;
				EntireSprite = entireSprite;
			}
		}

		private class State {
			internal class FloatingSelectionState {
				internal readonly (int X, int Y) Position;
				internal readonly FloatingSelectionBitmap ImageData;

				public FloatingSelectionState((int X, int Y) position, FloatingSelectionBitmap floatingSelection) {
					Position = position;
					ImageData = floatingSelection;
				}
			}
			internal readonly Selection Selection;
			internal readonly FloatingSelectionState FloatingSelection;
			internal readonly MainWindow.EToolPhase ToolPhase;
			internal readonly PixelData UnitPixelData;
			internal readonly PixelData ItemPixelData;
			internal readonly string SelectedPalette;

			internal State(int unitFrameIndex, SavedFrame unitFrameData, Selection selection, FloatingSelectionState floatingSelection, MainWindow.EToolPhase toolPhase, string selectedPalette, ItemSpriteSheet itemSpriteSheet) {
				Selection = selection;
				FloatingSelection = floatingSelection;
				ToolPhase = toolPhase;
				SelectedPalette = selectedPalette;
				UnitPixelData = new PixelData(unitFrameIndex, unitFrameData, null);
				if (itemSpriteSheet != null) {
					ItemPixelData = new PixelData(-1, null, new WriteableBitmap(itemSpriteSheet.frameSource.sprite));
				} else {
					ItemPixelData = null;
				}
			}

			internal State(UnitSpriteSheet spriteSheet, Selection selection, FloatingSelectionState floatingSelection, MainWindow.EToolPhase toolPhase, string selectedPalette, ItemSpriteSheet itemSpriteSheet) {
				Selection = selection;
				FloatingSelection = floatingSelection;
				ToolPhase = toolPhase;
				SelectedPalette = selectedPalette;
				UnitPixelData = new PixelData(-1, null, new WriteableBitmap(spriteSheet.frameSource.sprite));
				if (itemSpriteSheet != null) {
					ItemPixelData = new PixelData(-1, null, new WriteableBitmap(itemSpriteSheet.frameSource.sprite));
				} else {
					ItemPixelData = null;
				}
			}
		}

		UnitSpriteSheet SpriteSheet;
		MainWindow ApplicationWindow;

		Stack<State> UndoBuffer;
		Stack<State> RedoBuffer;

		bool InsideUndoBlock = false;

		public UndoSystem(UnitSpriteSheet spriteSheet, MainWindow mainWindow) {
			SpriteSheet = spriteSheet;
			ApplicationWindow = mainWindow;
			UndoBuffer = new Stack<State>();
			RedoBuffer = new Stack<State>();
		}

		private void PushUndoState(State state) {
			UndoBuffer.Push(state);
			RedoBuffer.Clear();
		}

		private State PopUndoState() {
			State temp = UndoBuffer.Pop();
			if (temp.UnitPixelData.FrameIndex == -1) {
				RedoBuffer.Push(CaptureEntireSprite());
			} else {
				RedoBuffer.Push(CaptureCurrentState(temp.UnitPixelData.FrameIndex));
			}
			return temp;
		}
		private State PopRedoState() {
			State temp = RedoBuffer.Pop();
			if (temp.UnitPixelData.FrameIndex == -1) {
				UndoBuffer.Push(CaptureEntireSprite());
			} else {
				UndoBuffer.Push(CaptureCurrentState(temp.UnitPixelData.FrameIndex));
			}
			return temp;
		}

		private State CaptureEntireSprite(string previousPalette = null) {
			int floatingSelectionX = ((int)System.Windows.Controls.Canvas.GetLeft(ApplicationWindow.ImageFloatingSelection));
			int floatingSelectionY = ((int)System.Windows.Controls.Canvas.GetTop(ApplicationWindow.ImageFloatingSelection));

			State currentState;
			currentState = new State(SpriteSheet
				, new Selection(ApplicationWindow.selectedArea)
				, new State.FloatingSelectionState((floatingSelectionX, floatingSelectionY), ApplicationWindow.floatingSelection)
				, ApplicationWindow.toolPhase
				, previousPalette
				, ApplicationWindow.itemSpriteSheet);
			return currentState;
		}
		private State CaptureCurrentState(int frameIndex = -1) {
			if (frameIndex == -1) {
				DrawingRoutines.FrameMetadata frameMetadata = ApplicationWindow.GatherMetadata();
				int layer = ApplicationWindow.ListBoxLayers.SelectedIndex;
				var frameInfo = SpriteSheet.drawingRoutine.GetLayerFrame(frameMetadata, layer);
				frameIndex = frameInfo.Index;
			}

			int floatingSelectionX = ((int)System.Windows.Controls.Canvas.GetLeft(ApplicationWindow.ImageFloatingSelection));
			int floatingSelectionY = ((int)System.Windows.Controls.Canvas.GetTop(ApplicationWindow.ImageFloatingSelection));


			State currentState;
			currentState = new State(frameIndex
				, new SavedFrame(frameIndex, SpriteSheet)
				, new Selection(ApplicationWindow.selectedArea)
				, new State.FloatingSelectionState((floatingSelectionX, floatingSelectionY), ApplicationWindow.floatingSelection)
				, ApplicationWindow.toolPhase
				, null
				, ApplicationWindow.itemSpriteSheet);
			return currentState;
		}

		internal void BeginUndoBlock(string previousPalette = null) {
			if (!InsideUndoBlock) {
				InsideUndoBlock = true;
				PushUndoState(CaptureEntireSprite(previousPalette));
			}
		}

		internal void EndUndoBlock() {
			InsideUndoBlock = false;
		}

		internal void RegisterUndoState() {
			if (!InsideUndoBlock) PushUndoState(CaptureCurrentState());
		}

		public void Redo() {
			if (RedoBuffer.Count == 0) return;
			State nextState = PopRedoState();
			ApplyState(nextState);
		}

		public void Undo() {
			if (UndoBuffer.Count == 0) return;
			State previousState = PopUndoState();
			ApplyState(previousState);
		}

		private void ApplyState(State state) {
			if (state.UnitPixelData.FrameData != null) {
				state.UnitPixelData.FrameData.Apply(SpriteSheet);
			} else {
				SpriteSheet.frameSource.SetInternalSprite(state.UnitPixelData.EntireSprite);
			}
			if (state.ItemPixelData != null) ApplicationWindow.itemSpriteSheet.frameSource.SetInternalSprite(state.ItemPixelData.EntireSprite);
			ApplicationWindow.selectedArea = state.Selection;
			System.Windows.Controls.Canvas.SetLeft(ApplicationWindow.ImageFloatingSelection, state.FloatingSelection.Position.X);
			System.Windows.Controls.Canvas.SetTop(ApplicationWindow.ImageFloatingSelection, state.FloatingSelection.Position.Y);
			ApplicationWindow.floatingSelection = state.FloatingSelection.ImageData;
			ApplicationWindow.ImageFloatingSelection.Source = state.FloatingSelection.ImageData.bitmap;
			ApplicationWindow.toolPhase = state.ToolPhase;
			if (!(state.SelectedPalette is null)) {
				ApplicationWindow.SelectPalette(state.SelectedPalette);
				ApplicationWindow.ToolColorPalette.ApplyPalette(SpriteSheet.GetColorPalette());
			}
		}
	}
}
