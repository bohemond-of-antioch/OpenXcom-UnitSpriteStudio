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

			internal SavedFrame(int frameIndex, SpriteSheet spriteSheet) {
				PixelData = spriteSheet.frameSource.GetFramePixelData(frameIndex);
				FrameIndex = frameIndex;
			}

			internal void Apply(SpriteSheet spriteSheet) {
				spriteSheet.frameSource.SetFramePixelData(FrameIndex, PixelData);
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
			internal readonly int FrameIndex;
			internal readonly SavedFrame FrameData;
			internal readonly Selection Selection;
			internal readonly FloatingSelectionState FloatingSelection;
			internal readonly MainWindow.EToolPhase ToolPhase;
			internal readonly WriteableBitmap EntireSprite;

			internal State(int frameIndex, SavedFrame frameData, Selection selection, FloatingSelectionState floatingSelection, MainWindow.EToolPhase toolPhase) {
				FrameIndex = frameIndex;
				FrameData = frameData;
				Selection = selection;
				FloatingSelection = floatingSelection;
				ToolPhase = toolPhase;
				EntireSprite = null;
			}

			internal State(SpriteSheet spriteSheet,Selection selection, FloatingSelectionState floatingSelection, MainWindow.EToolPhase toolPhase) {
				FrameIndex = -1;
				FrameData = null;
				Selection = selection;
				FloatingSelection = floatingSelection;
				ToolPhase = toolPhase;
				EntireSprite = new WriteableBitmap(spriteSheet.frameSource.sprite);
			}
		}

		SpriteSheet SpriteSheet;
		MainWindow ApplicationWindow;

		Stack<State> UndoBuffer;
		Stack<State> RedoBuffer;

		bool InsideUndoBlock = false;

		public UndoSystem(SpriteSheet spriteSheet,MainWindow mainWindow) {
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
			if (temp.FrameIndex==-1) {
				RedoBuffer.Push(CaptureEntireSprite());
			} else {
				RedoBuffer.Push(CaptureCurrentState(temp.FrameIndex));
			}
			return temp;
		}
		private State PopRedoState() {
			State temp = RedoBuffer.Pop();
			if (temp.FrameIndex == -1) {
				UndoBuffer.Push(CaptureEntireSprite());
			} else {
				UndoBuffer.Push(CaptureCurrentState(temp.FrameIndex));
			}
			return temp;
		}

		private State CaptureEntireSprite() {
			int floatingSelectionX = ((int)System.Windows.Controls.Canvas.GetLeft(ApplicationWindow.ImageFloatingSelection));
			int floatingSelectionY = ((int)System.Windows.Controls.Canvas.GetTop(ApplicationWindow.ImageFloatingSelection));

			State currentState;
			currentState = new State(SpriteSheet
				, new Selection(ApplicationWindow.selectedArea)
				, new State.FloatingSelectionState((floatingSelectionX, floatingSelectionY), ApplicationWindow.floatingSelection)
				, ApplicationWindow.toolPhase);
			return currentState;
		}
		private State CaptureCurrentState(int frameIndex=-1) {
			if (frameIndex==-1) {
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
				, new Selection( ApplicationWindow.selectedArea)
				, new State.FloatingSelectionState((floatingSelectionX, floatingSelectionY), ApplicationWindow.floatingSelection)
				, ApplicationWindow.toolPhase);
			return currentState;
		}

		internal void BeginUndoBlock() {
			if (!InsideUndoBlock) {
				InsideUndoBlock = true;
				PushUndoState(CaptureEntireSprite());
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
			if (state.FrameData!=null) {
				state.FrameData.Apply(SpriteSheet);
			} else {
				SpriteSheet.frameSource.SetInternalSprite(state.EntireSprite);
			}
			ApplicationWindow.selectedArea = state.Selection;
			System.Windows.Controls.Canvas.SetLeft(ApplicationWindow.ImageFloatingSelection, state.FloatingSelection.Position.X);
			System.Windows.Controls.Canvas.SetTop(ApplicationWindow.ImageFloatingSelection, state.FloatingSelection.Position.Y);
			ApplicationWindow.floatingSelection = state.FloatingSelection.ImageData;
			ApplicationWindow.ImageFloatingSelection.Source = state.FloatingSelection.ImageData.bitmap;
			ApplicationWindow.toolPhase = state.ToolPhase;
		}
	}
}
