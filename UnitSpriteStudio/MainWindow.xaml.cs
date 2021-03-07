using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private enum ECursorTool {
			SelectPixels,
			SelectBox,
			SelectLasso,
			SelectColors,
			SelectArea,
			Paint
		}
		private const int EditImageScale = 10;
		private const int AnimationImageScale = 3;
		private const int LayerImageScale = 2;

		private SpriteSheet spriteSheet;
		private RenderTargetBitmap compositeImageSource;
		private RenderTargetBitmap overlayImageSource;

		private RenderTargetBitmap animationImageSource;
		private int animationFrame = 0;

		private System.Windows.Threading.DispatcherTimer animationTimer;
		private List<Image> LayerImages;
		private bool SpriteSheetInitialized;

		private ECursorTool CursorTool=ECursorTool.Paint;

		public MainWindow() {
			InitializeComponent();
			LayerImages = new List<Image>();
			UnitDirectionControl.OnChanged += UnitDirectionControl_OnChanged;
			ToolColorPalette.OnSelectedColorChanged += ToolColorPalette_OnSelectedColorChanged;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			animationTimer = new System.Windows.Threading.DispatcherTimer();
			animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();
		}

		private void AnimationTimer_Tick(object sender, EventArgs e) {
			animationFrame = (animationFrame + 1) % spriteSheet.drawingRoutine.GetAnimationFrameCount(GatherMetadata());
			RefreshAnimationImage();
		}

		private DrawingRoutines.FrameMetadata GatherMetadata() {
			DrawingRoutines.FrameMetadata metadata;
			metadata = new DrawingRoutines.FrameMetadata();
			metadata.Direction = UnitDirectionControl.SelectedDirection;
			metadata.PrimaryFrame = ListBoxPrimaryFrames.SelectedIndex;
			metadata.SecondaryFrame = ListBoxSecondaryFrames.SelectedIndex;
			metadata.TertiaryFrame = ListBoxTertiaryFrames.SelectedIndex;
			metadata.AnimationFrame = (int)SliderFrame.Value;
			return metadata;
		}

		private const int CheckerboardSize = 6;
		internal void DrawTransparentBackground(DrawingContext drawingContext, ImageSource imageSource, bool faded = false) {
			if (ToggleBackground.IsChecked==true) {
				Brush darkSquare,lightSquare;
				if (faded) {
					darkSquare = new SolidColorBrush(Color.FromArgb((byte)SliderHighlightPower.Value, 128, 128, 128));
					lightSquare = new SolidColorBrush(Color.FromArgb((byte)SliderHighlightPower.Value, 192, 192, 192));
				} else {
					darkSquare = new SolidColorBrush(Color.FromRgb(128, 128, 128));
					lightSquare = new SolidColorBrush(Color.FromRgb(192, 192, 192));
				}
				int x, y;
				for (x=0;x<imageSource.Width/CheckerboardSize+1;x++) {
					for (y = 0; y < imageSource.Height/ CheckerboardSize + 1; y++) {
						drawingContext.DrawRectangle(((x + y) % 2) == 0 ? darkSquare : lightSquare, null, new Rect(x * CheckerboardSize, y * CheckerboardSize, CheckerboardSize, CheckerboardSize));
					}
				}
			} else {
				Brush fillBrush;
				if (faded) {
					fillBrush = new SolidColorBrush(Color.FromArgb((byte)SliderHighlightPower.Value, 0, 0, 0));
				} else {
					fillBrush = Brushes.Black;
				}
				drawingContext.DrawRectangle(fillBrush, null, new Rect(0, 0, imageSource.Width, imageSource.Height));
			}
		}
		internal void RefreshCompositeImage() {
			if (compositeImageSource == null) return;
			int HighlightedLayer = -1;
			if (CheckBoxHighlightLayer.IsChecked == true) {
				HighlightedLayer = ListBoxLayers.SelectedIndex;
			}
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				DrawTransparentBackground(drawingContext, compositeImageSource);
				if (HighlightedLayer != -1) {
					spriteSheet.DrawCompositeImage(GatherMetadata(), drawingContext, -1);
					DrawTransparentBackground(drawingContext, compositeImageSource, true);
				}
				spriteSheet.DrawCompositeImage(GatherMetadata(), drawingContext, HighlightedLayer);
			}
			compositeImageSource.Render(drawingVisual);
		}

		internal void RefreshOverlayImage() {
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				//drawingContext.DrawLine(new Pen(Brushes.Black,1), new Point(10, 10), new Point(100, 100));
			}
			overlayImageSource.Render(drawingVisual);
		}

		internal void RefreshLayerImages() {
			DrawingRoutines.FrameMetadata layerMetadata;
			layerMetadata = GatherMetadata();
			for (int l = 0; l < LayerImages.Count; l++) {
				try {
					LayerImages[l].Source = spriteSheet.GetLayerImage(layerMetadata, l);
					LayerImages[l].Visibility = Visibility.Visible;
				} catch (Exception) {
					LayerImages[l].Source = null;
					LayerImages[l].Visibility = Visibility.Hidden;
				}
			}
		}

		private void RefreshAnimationImage() {
			if (animationImageSource == null) return;
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				DrawingRoutines.FrameMetadata metadata = GatherMetadata();
				metadata.AnimationFrame = animationFrame;
				DrawTransparentBackground(drawingContext, animationImageSource);
				spriteSheet.DrawCompositeImage(metadata, drawingContext);
			}
			animationImageSource.Render(drawingVisual);
		}

		internal void InitializeSpriteSheet(SpriteSheet spriteSheet) {
			SpriteSheetInitialized = false;
			this.spriteSheet = spriteSheet;
			ListBoxPrimaryFrames.Items.Clear();
			foreach (string frameName in spriteSheet.drawingRoutine.PrimaryFrameNames()) {
				ListBoxPrimaryFrames.Items.Add(frameName);
			}
			ListBoxPrimaryFrames.SelectedIndex = 0;
			ListBoxSecondaryFrames.Items.Clear();
			foreach (string frameName in spriteSheet.drawingRoutine.SecondaryFrameNames()) {
				ListBoxSecondaryFrames.Items.Add(frameName);
			}
			ListBoxSecondaryFrames.SelectedIndex = 0;
			ListBoxTertiaryFrames.Items.Clear();
			foreach (string frameName in spriteSheet.drawingRoutine.TertiaryFrameNames()) {
				ListBoxTertiaryFrames.Items.Add(frameName);
			}
			ListBoxTertiaryFrames.SelectedIndex = 0;
			var CompositeImageSize = spriteSheet.drawingRoutine.CompositeImageSize();
			compositeImageSource = new RenderTargetBitmap(CompositeImageSize.Width, CompositeImageSize.Height, 96, 96, PixelFormats.Pbgra32);
			ImageComposite.Width = CompositeImageSize.Width * EditImageScale;
			ImageComposite.Height = CompositeImageSize.Height * EditImageScale;
			ImageComposite.Source = compositeImageSource;

			animationImageSource = new RenderTargetBitmap(CompositeImageSize.Width, CompositeImageSize.Height, 96, 96, PixelFormats.Pbgra32);
			ImageAnimation.Width = CompositeImageSize.Width * AnimationImageScale;
			ImageAnimation.Height = CompositeImageSize.Height * AnimationImageScale;
			ImageAnimation.Source = animationImageSource;

			overlayImageSource = new RenderTargetBitmap(CompositeImageSize.Width * EditImageScale, CompositeImageSize.Height * EditImageScale, 96, 96, PixelFormats.Pbgra32);
			ImageOverlay.Width = CompositeImageSize.Width * EditImageScale;
			ImageOverlay.Height = CompositeImageSize.Height * EditImageScale;
			ImageOverlay.Source = overlayImageSource;

			foreach (Image element in LayerImages) {
				LayerPanel.Children.Remove(element);
			}

			ListBoxLayers.Items.Clear();
			DrawingRoutines.FrameMetadata layerMetadata;
			layerMetadata = GatherMetadata();
			int f = 0;
			foreach (string layerName in spriteSheet.drawingRoutine.LayerNames()) {
				ListBoxLayers.Items.Add(layerName);
				Image newLayerImage = new Image();
				RenderOptions.SetBitmapScalingMode(newLayerImage, BitmapScalingMode.NearestNeighbor);

				var LayerImageSize = spriteSheet.drawingRoutine.CompositeImageSize();
				newLayerImage.Width = LayerImageSize.Width * LayerImageScale;
				newLayerImage.Height = LayerImageSize.Height * LayerImageScale;
				try {
					newLayerImage.Source = spriteSheet.GetLayerImage(layerMetadata, f);
				} catch (Exception) {

				}

				LayerImages.Add(newLayerImage);
				LayerPanel.Children.Add(newLayerImage);
				f++;
			}
			ListBoxLayers.SelectedIndex = 0;

			SliderFrame.Minimum = 0;
			SliderFrame.Maximum = spriteSheet.drawingRoutine.GetAnimationFrameCount(GatherMetadata()) - 1;
			SliderFrame.Value = 0;

			ToolColorPalette.ApplyPalette(spriteSheet.GetColorPalette());
			SpriteSheetInitialized = true;
			FrameMetadataChanged();
		}

		private void ToolColorPalette_OnSelectedColorChanged() {
			SelectedColorLeft.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedLeftColor]);
			SelectedColorMiddle.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedMiddleColor]);
			SelectedColorRight.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedRightColor]);
		}
		private void ImageOverlay_MouseDown(object sender, MouseButtonEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / EditImageScale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / EditImageScale);

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
				byte ColorUnderCursor = spriteSheet.GetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY);
				if (e.ChangedButton == MouseButton.Left) ToolColorPalette.SelectedLeftColor= ColorUnderCursor;
				if (e.ChangedButton == MouseButton.Middle)  ToolColorPalette.SelectedMiddleColor= ColorUnderCursor;
				if (e.ChangedButton == MouseButton.Right)  ToolColorPalette.SelectedRightColor= ColorUnderCursor;
				ToolColorPalette.UpdateMarkers();
				ToolColorPalette_OnSelectedColorChanged();
				return;
			}

			byte ApplyColor = 0;
			if (e.ChangedButton == MouseButton.Left) ApplyColor = ToolColorPalette.SelectedLeftColor;
			if (e.ChangedButton == MouseButton.Middle) ApplyColor = ToolColorPalette.SelectedMiddleColor;
			if (e.ChangedButton == MouseButton.Right) ApplyColor = ToolColorPalette.SelectedRightColor;
			spriteSheet.SetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY, ApplyColor);
			RefreshCompositeImage();
		}
		private void ImageOverlay_MouseMove(object sender, MouseEventArgs e) {
			byte ApplyColor;
			if (e.LeftButton == MouseButtonState.Pressed) {
				ApplyColor = ToolColorPalette.SelectedLeftColor;
			} else if (e.MiddleButton == MouseButtonState.Pressed) {
				ApplyColor = ToolColorPalette.SelectedMiddleColor;
			} else if (e.RightButton == MouseButtonState.Pressed) {
				ApplyColor = ToolColorPalette.SelectedRightColor;
			} else {
				return;
			}
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / EditImageScale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / EditImageScale);
			spriteSheet.SetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY, ApplyColor);
			RefreshCompositeImage();
		}

		private void UpdateControls() {
			SliderFrame.Maximum = spriteSheet.drawingRoutine.GetAnimationFrameCount(GatherMetadata()) - 1;
			ToolColorPalette_OnSelectedColorChanged();
		}
		private void FrameMetadataChanged() {
			if (SpriteSheetInitialized) {
				RefreshCompositeImage();
				RefreshLayerImages();
				UpdateControls();
			}
		}

		private void FrameSelectionChanged(object sender, SelectionChangedEventArgs e) {
			FrameMetadataChanged();
		}

		private void UnitDirectionControl_OnChanged() {
			FrameMetadataChanged();
		}

		private void SliderFrame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			FrameMetadataChanged();
		}

		private void CheckBoxHighlightLayer_Click(object sender, RoutedEventArgs e) {
			FrameMetadataChanged();
		}

		private void ListBoxLayers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			FrameMetadataChanged();
		}

		private void SliderHighlightPower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			FrameMetadataChanged();
		}

		private void AnimationControlPlay(object sender, RoutedEventArgs e) {
			animationTimer.Start();
		}

		private void AnimationControlStop(object sender, RoutedEventArgs e) {
			animationTimer.Stop();
		}

		private void MenuItemNew_Click(object sender, RoutedEventArgs e) {

		}

		private void MenuItemOpen_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "PNG Files|*.png|All Files|*.*";
			if (openFileDialog.ShowDialog() == true) {
				SpriteSheet openedSpriteSheet;
				int DrawingRoutine = int.Parse((string)((MenuItem)sender).Tag);
				switch (DrawingRoutine) {
					case 0:
						openedSpriteSheet = new SpriteSheet(new DrawingRoutines.DrawingRoutineSoldier());
						break;
					default:
						throw new Exception("Unsupported drawing routine.");
				}
				openedSpriteSheet.LoadSprite(openFileDialog.FileName);
				InitializeSpriteSheet(openedSpriteSheet);
			}
		}

		private void MenuItemSave_Click(object sender, RoutedEventArgs e) {
			spriteSheet.Save();
		}
		private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "PNG Files|*.png|All Files|*.*";
			if (saveFileDialog.ShowDialog() == true) {
				spriteSheet.Save(saveFileDialog.FileName);
			}
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void MenuItemCopyRulSnippet_Click(object sender, RoutedEventArgs e) {
			Clipboard.SetText(spriteSheet.GenerateRulSnippet());
		}

		private void ToggleCursorToolClicked(object sender, RoutedEventArgs e) {
			foreach (var control in ToolBar.Children.OfType<System.Windows.Controls.Primitives.ToggleButton>()) {
				if (control.Tag != null) {
					control.IsChecked = (control == sender);
					if (control==sender) {
						CursorTool = (ECursorTool)int.Parse((string)control.Tag);
					}
				}
			}
		}

		private void ToggleBackground_Click(object sender, RoutedEventArgs e) {
			FrameMetadataChanged();
		}
	}
}
