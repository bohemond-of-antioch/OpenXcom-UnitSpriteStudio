using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UnitSpriteStudio.FrameProcessing;

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for MirroredSpritesGenerator.xaml
	/// </summary>
	public partial class MirroredSpritesGenerator : Window {
		MainWindow ApplicationWindow;

		Image[] SourceImages, DestinationImages;
		Path[] Links;
		BezierSegment[] LinkBeziers;

		private const int PreviewImageScale = 1;
		private MirroredDirectionsOperation Operation;
		private int ActiveDirection = 0;

		public MirroredSpritesGenerator() {
			InitializeComponent();
			ApplicationWindow = (MainWindow)Application.Current.MainWindow;
			#region Nothing to see here... move along.
			SourceImages = new Image[8];
			DestinationImages = new Image[8];
			Links = new Path[8];
			LinkBeziers = new BezierSegment[8];
			for (int f = 0; f < 8; f++) {
				SourceImages[f] = (Image)BaseGrid.FindName(string.Format("SourceImage{0}", f));
				DestinationImages[f] = (Image)BaseGrid.FindName(string.Format("DestinationImage{0}", f));
				Links[f] = (Path)BaseGrid.FindName(string.Format("Link{0}", f));
				LinkBeziers[f] = (BezierSegment)BaseGrid.FindName(string.Format("Link{0}Bezier", f));
			}
			#endregion

			var CompositeImageSize = ApplicationWindow.spriteSheet.drawingRoutine.CompositeImageSize();
			Operation = new MirroredDirectionsOperation();

			foreach (var image in SourceImages) {
				image.Source = new RenderTargetBitmap(CompositeImageSize.Width, CompositeImageSize.Height, 96, 96, PixelFormats.Pbgra32);
				image.Width = CompositeImageSize.Width * PreviewImageScale;
				image.Height = CompositeImageSize.Height * PreviewImageScale;
			}
			foreach (var image in DestinationImages) {
				image.Source = new RenderTargetBitmap(CompositeImageSize.Width, CompositeImageSize.Height, 96, 96, PixelFormats.Pbgra32);
				image.Width = CompositeImageSize.Width * PreviewImageScale;
				image.Height = CompositeImageSize.Height * PreviewImageScale;
			}
		}


		private const int CheckerboardSize = 6;
		internal void DrawTransparentBackground(DrawingContext drawingContext, ImageSource imageSource, bool faded = false) {
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
		internal void RefreshCompositeImage(Image targetImage) {
			int HighlightedLayer = -1;
			if (ApplicationWindow.CheckBoxHighlightLayer.IsChecked == true) {
				HighlightedLayer = ApplicationWindow.ListBoxLayers.SelectedIndex;
			}
			DrawingVisual drawingVisual = new DrawingVisual();
			var metadata = ApplicationWindow.GatherMetadata();
			metadata.Direction = int.Parse((string)targetImage.Tag);
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				DrawTransparentBackground(drawingContext, targetImage.Source);
				if (HighlightedLayer != -1) {
					ApplicationWindow.spriteSheet.DrawCompositeImage(metadata, drawingContext, -1);
					DrawTransparentBackground(drawingContext, targetImage.Source, true);
				}
				ApplicationWindow.spriteSheet.DrawCompositeImage(metadata, drawingContext, HighlightedLayer);
			}
			((RenderTargetBitmap)targetImage.Source).Render(drawingVisual);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			ApplicationWindow.OnMetadataChanged += Refresh;
			Refresh();
		}

		private void Refresh() {
			RefreshImages();
			UpdateUIForOperation();
		}

		private void Window_Closed(object sender, EventArgs e) {
			ApplicationWindow.OnMetadataChanged -= Refresh;
		}

		private void SourceButtonClick(object sender, RoutedEventArgs e) {
			foreach (var button in GridSource.Children.OfType<ToggleButton>()) {
				button.IsChecked = (button == sender);
				if (button == sender) {
					ActiveDirection = int.Parse((string)button.Tag);
					UpdateUIForOperation();
				}
			}
		}

		private void UpdateUIForOperation() {
			var options = Operation.Directions[ActiveDirection];
			foreach (var button in GridDestination.Children.OfType<ToggleButton>()) {
				button.IsChecked = (int.Parse((string)button.Tag) == options.Destination);
			}
			CheckBoxActive.IsChecked = options.Active;
			CheckBoxFlip.IsChecked = options.Flip;
			CheckBoxMirror.IsChecked = options.Mirror;
			CheckBoxChangeArms.IsChecked = options.ChangeArms;

			CheckBoxEntireAnimation.IsChecked = Operation.EntireAnimation;
			CheckBoxPrimaryGroup.IsChecked = Operation.EntirePrimaryGroup;
			CheckBoxSecondaryGroup.IsChecked = Operation.EntireSecondaryGroup;
			CheckBoxTertiaryGroup.IsChecked = Operation.EntireTertiaryGroup;

			for (int d = 0; d < 8; d++) {
				if (Operation.Directions[d].Active) {
					Links[d].Visibility = Visibility.Visible;
					Point imagePosition = SourceImages[d].TranslatePoint(new Point(SourceImages[d].ActualWidth / 2, SourceImages[d].ActualHeight / 2), CanvasLinks);
					Canvas.SetLeft(Links[d], imagePosition.X);
					Canvas.SetTop(Links[d], imagePosition.Y);

					int targetDirection = Operation.Directions[d].Destination;
					Point targetPosition = DestinationImages[targetDirection].TranslatePoint(new Point(DestinationImages[targetDirection].ActualWidth / 2, DestinationImages[targetDirection].ActualHeight / 2), CanvasLinks);
					double deltaX, deltaY;
					deltaX = targetPosition.X - imagePosition.X;
					deltaY = targetPosition.Y - imagePosition.Y;
					LinkBeziers[d].Point1 = new Point(deltaX / 3, 30);
					LinkBeziers[d].Point2 = new Point(deltaX / 3 * 2, deltaY - 30);
					LinkBeziers[d].Point3 = new Point(deltaX, deltaY);
				} else {
					Links[d].Visibility = Visibility.Hidden;
				}
			}


		}
		private void DestinationButtonClick(object sender, RoutedEventArgs e) {
			Operation.Directions[ActiveDirection].Destination = int.Parse((string)((Control)sender).Tag);
			UpdateUIForOperation();
		}
		private void CheckBoxActive_Click(object sender, RoutedEventArgs e) {
			Operation.Directions[ActiveDirection].Active = CheckBoxActive.IsChecked == true;
			UpdateUIForOperation();
		}
		private void CheckBoxMirror_Click(object sender, RoutedEventArgs e) {
			Operation.Directions[ActiveDirection].Mirror = CheckBoxMirror.IsChecked == true;
		}
		private void CheckBoxFlip_Click(object sender, RoutedEventArgs e) {
			Operation.Directions[ActiveDirection].Flip = CheckBoxFlip.IsChecked == true;
		}
		private void CheckBoxChangeArms_Click(object sender, RoutedEventArgs e) {
			Operation.Directions[ActiveDirection].ChangeArms = CheckBoxChangeArms.IsChecked == true;
		}
		private void CheckBoxEntireAnimation_Click(object sender, RoutedEventArgs e) {
			Operation.EntireAnimation = CheckBoxEntireAnimation.IsChecked == true;
		}
		private void CheckBoxPrimaryGroup_Click(object sender, RoutedEventArgs e) {
			Operation.EntirePrimaryGroup = CheckBoxPrimaryGroup.IsChecked == true;
		}
		private void CheckBoxSecondaryGroup_Click(object sender, RoutedEventArgs e) {
			Operation.EntireSecondaryGroup = CheckBoxSecondaryGroup.IsChecked == true;
		}
		private void CheckBoxTertiaryGroup_Click(object sender, RoutedEventArgs e) {
			Operation.EntireTertiaryGroup = CheckBoxTertiaryGroup.IsChecked == true;
		}

		private void ButtonGO_Click(object sender, RoutedEventArgs e) {
			MainWindow.undoSystem.BeginUndoBlock();
			Operation.Run(ApplicationWindow.GatherMetadata(), ApplicationWindow.spriteSheet);
			MainWindow.undoSystem.EndUndoBlock();
			ApplicationWindow.FrameMetadataChanged();
		}

		private void ListBoxSelectedPreset_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			switch (ListBoxSelectedPreset.SelectedIndex) {
				case 1:
					Operation.ApplyPresetLeftToRight();
					UpdateUIForOperation();
					break;
				case 2:
					Operation.ApplyPresetRightToLeft();
					UpdateUIForOperation();
					break;
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			ApplicationWindow.Window_KeyUp(sender, e);
		}

		private void RefreshImages() {
			for (int f = 0; f < 8; f++) {
				RefreshCompositeImage(SourceImages[f]);
				RefreshCompositeImage(DestinationImages[f]);
			}
		}
	}
}
