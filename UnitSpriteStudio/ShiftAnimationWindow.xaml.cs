using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UnitSpriteStudio.FrameProcessing;

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for ShiftAnimationWindow.xaml
	/// </summary>
	public partial class ShiftAnimationWindow : Window {
		MainWindow ApplicationWindow;
		List<InteractiveFrame> animationFrames;
		public ShiftAnimationWindow() {
			InitializeComponent();
			ApplicationWindow = (MainWindow)Application.Current.MainWindow;
			ApplicationWindow.OnMetadataChanged += ApplicationWindow_OnMetadataChanged;
		}

		private void ApplicationWindow_OnMetadataChanged() {
			foreach (InteractiveFrame frame in animationFrames) {
				RefreshCompositeImage(frame);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			BuildFrames();
		}

		private void Window_Closed(object sender, EventArgs e) {
		}

		private void BuildFrames() {
			var metadata = ApplicationWindow.GatherMetadata();
			var animationFrameCount = ApplicationWindow.spriteSheet.drawingRoutine.GetAnimationFrameCount(metadata);
			animationFrames = new List<InteractiveFrame>();

			var CompositeImageSize = ApplicationWindow.spriteSheet.drawingRoutine.CompositeImageSize();

			for (int f = 0; f < animationFrameCount; f++) {
				InteractiveFrame newFrame;
				BitmapSource frameSource = new RenderTargetBitmap(CompositeImageSize.Width, CompositeImageSize.Height, 96, 96, PixelFormats.Pbgra32);

				newFrame = new InteractiveFrame(4, frameSource);
				newFrame.EnableReticle(true);
				newFrame.EnableSecondaryReticle(true);
				newFrame.MoveReticle(-1, -1);
				newFrame.MoveSecondaryReticle(0, 0);

				newFrame.InteractionMouseDown += AnimationFrame_Interaction_MouseClick;
				newFrame.InteractionMouseUp += AnimationFrame_Interaction_MouseClick;
				newFrame.InteractionMouseMove += AnimationFrame_Interaction_MouseMove;

				newFrame.ID = f;
				AnimationFrames.Children.Add(newFrame);

				RefreshCompositeImage(newFrame);
				animationFrames.Add(newFrame);
			}
		}

		private void MoveSecondaryReticles((int x, int y) position) {
			foreach (var frame in animationFrames) {
				frame.MoveSecondaryReticle(position.x, position.y);
				if (frame.ReticlePosition.X == -1) {
					frame.MoveReticle(position.x, position.y);
				}
			}
		}

		private void AnimationFrame_Interaction_MouseMove(InteractiveFrame source, int x, int y, MouseEventArgs e) {
			if (e.RightButton == MouseButtonState.Pressed) MoveSecondaryReticles(source.SecondaryReticlePosition);
		}

		private void AnimationFrame_Interaction_MouseClick(InteractiveFrame source, int x, int y, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Right) MoveSecondaryReticles(source.SecondaryReticlePosition);
		}

		private const int CheckerboardSize = 6;
		private void DrawTransparentBackground(DrawingContext drawingContext, ImageSource imageSource, bool faded = false) {
			if (ApplicationWindow.ToggleBackground.IsChecked == true) {
				Brush darkSquare, lightSquare;
				if (faded) {
					darkSquare = new SolidColorBrush(Color.FromArgb((byte)ApplicationWindow.SliderHighlightPower.Value, 128, 128, 128));
					lightSquare = new SolidColorBrush(Color.FromArgb((byte)ApplicationWindow.SliderHighlightPower.Value, 192, 192, 192));
				} else {
					darkSquare = new SolidColorBrush(Color.FromRgb(128, 128, 128));
					lightSquare = new SolidColorBrush(Color.FromRgb(192, 192, 192));
				}
				int x, y;
				for (x = 0; x < imageSource.Width / CheckerboardSize + 1; x++) {
					for (y = 0; y < imageSource.Height / CheckerboardSize + 1; y++) {
						drawingContext.DrawRectangle(((x + y) % 2) == 0 ? darkSquare : lightSquare, null, new Rect(x * CheckerboardSize, y * CheckerboardSize, CheckerboardSize, CheckerboardSize));
					}
				}
			} else {
				Brush fillBrush;
				if (faded) {
					fillBrush = new SolidColorBrush(Color.FromArgb((byte)ApplicationWindow.SliderHighlightPower.Value, 0, 0, 0));
				} else {
					fillBrush = Brushes.Black;
				}
				drawingContext.DrawRectangle(fillBrush, null, new Rect(0, 0, imageSource.Width, imageSource.Height));
			}
		}
		private void RefreshCompositeImage(InteractiveFrame targetInteractiveFrame) {
			DrawingVisual drawingVisual = new DrawingVisual();
			var metadata = ApplicationWindow.GatherMetadata();
			metadata.AnimationFrame = targetInteractiveFrame.ID;
			BitmapSource targetBitmap = targetInteractiveFrame.GetBitmap();
			int HighlightedLayer = -1;
			if (ApplicationWindow.CheckBoxHighlightLayer.IsChecked == true) {
				HighlightedLayer = ApplicationWindow.ListBoxLayers.SelectedIndex;
			}
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				DrawTransparentBackground(drawingContext, targetBitmap);
				if (HighlightedLayer != -1) {
					ApplicationWindow.spriteSheet.DrawCompositeImage(metadata, drawingContext, -1);
					DrawTransparentBackground(drawingContext, targetBitmap, true);
				}
				ApplicationWindow.spriteSheet.DrawCompositeImage(metadata, drawingContext, HighlightedLayer);
			}
			((RenderTargetBitmap)targetBitmap).Render(drawingVisual);
		}

		private void ButtonGO_Click(object sender, RoutedEventArgs e) {
			MainWindow.undoSystem.BeginUndoBlock();
			var metadata = ApplicationWindow.GatherMetadata();
			var drawingRoutine = ApplicationWindow.spriteSheet.drawingRoutine;
			var frameSource = ApplicationWindow.spriteSheet.frameSource;
			foreach (InteractiveFrame frame in animationFrames) {
				int deltaX = frame.SecondaryReticlePosition.X - frame.ReticlePosition.X;
				int deltaY = frame.SecondaryReticlePosition.Y - frame.ReticlePosition.Y;
				metadata.AnimationFrame = frame.ID;
				for (int l = 0; l < drawingRoutine.LayerNames().Length; l++) {
					DrawingRoutines.DrawingRoutine.LayerFrameInfo frameInfo = drawingRoutine.GetLayerFrame(metadata, l);
					byte[] originalPixels = frameSource.GetFramePixelData(frameInfo.Index);
					byte[] shiftedPixels = new byte[originalPixels.Length];
					for (int x = 0; x < 32; x++) {
						for (int y = 0; y < 40; y++) {
							int newX = x + deltaX;
							int newY = y + deltaY;

							if (newX < 0 || newY < 0 || newX >= 32 || newY >= 40) continue;

							shiftedPixels[newX + newY * 32] = originalPixels[x + y * 32];
						}
					}
					frameSource.SetFramePixelData(frameInfo.Index, shiftedPixels);
				}
				frame.MoveReticle(frame.SecondaryReticlePosition.X, frame.SecondaryReticlePosition.Y);
				RefreshCompositeImage(frame);
			}
			ApplicationWindow.FrameMetadataChanged();
			MainWindow.undoSystem.EndUndoBlock();
		}

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			// TODO: This is a stupid solution, it needs some refactoring.
			// We let only Undo and Redo through.
			if (ApplicationWindow.IsRedoShortcut(e) || ApplicationWindow.IsUndoShortcut(e)) ApplicationWindow.Window_KeyUp(sender, e);
		}
	}
}
