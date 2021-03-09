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
			Paint,
			Paste
		}

		public enum EToolPhase {
			None,
			Started,
			InProgress,
			MoveFloatingSelection
		}
		private const int EditImageScale = 10;
		private const int AnimationImageScale = 3;
		private const int LayerImageScale = 2;

		internal SpriteSheet spriteSheet;
		private RenderTargetBitmap compositeImageSource;
		private RenderTargetBitmap overlayImageSource;
		internal FloatingSelectionBitmap floatingSelection;

		private RenderTargetBitmap animationImageSource;
		private int animationFrame = 0;

		private System.Windows.Threading.DispatcherTimer animationTimer;
		private List<Image> LayerImages;
		private bool SpriteSheetInitialized;

		private ECursorTool CursorTool=ECursorTool.Paint;

		internal Selection selectedArea, futureSelectedArea;
		private int selectionStartX, selectionStartY;
		internal EToolPhase toolPhase;
		private int lastCursorPositionX, lastCursorPositionY;
		//private FloatingSelectionBitmap copyBuffer;

		private UndoSystem undoSystem;

		public event Action OnMetadataChanged;
		public MainWindow() {
			InitializeComponent();
			LayerImages = new List<Image>();
			UnitDirectionControl.OnChanged += UnitDirectionControl_OnChanged;
			ToolColorPalette.OnSelectedColorChanged += ToolColorPalette_OnSelectedColorChanged;
			animationTimer = new System.Windows.Threading.DispatcherTimer();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();
		}

		private void AnimationTimer_Tick(object sender, EventArgs e) {
			animationFrame = (animationFrame + 1) % spriteSheet.drawingRoutine.GetAnimationFrameCount(GatherMetadata());
			RefreshAnimationImage();
		}

		internal DrawingRoutines.FrameMetadata GatherMetadata() {
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
			if (!animationTimer.IsEnabled) {
				animationFrame = (int)SliderFrame.Value;
				RefreshAnimationImage();
			}
			RefreshLayerImages();
		}

		internal void DrawSelectionBorder(Selection selection,Pen selectionPenWhite,Pen selectionPenBlack, DrawingContext drawingContext,int shiftX,int shiftY) {
			int x, y;
			for (x = 0; x < selection.SizeX; x++) {
				for (y = 0; y < selection.SizeY; y++) {
					if (selection.GetPoint((x, y))) {
						int originX = x * EditImageScale+shiftX;
						int originY = y * EditImageScale+shiftY;
						
						if (!selection.GetPoint((x - 1, y))) {
							drawingContext.DrawLine(selectionPenWhite, new Point(originX, originY), new Point(originX, originY + EditImageScale));
							drawingContext.DrawLine(selectionPenBlack, new Point(originX, originY), new Point(originX, originY + EditImageScale));
						}
						if (!selection.GetPoint((x + 1, y))) {
							drawingContext.DrawLine(selectionPenWhite, new Point(originX + EditImageScale - 1, originY), new Point(originX + EditImageScale - 1, originY + EditImageScale));
							drawingContext.DrawLine(selectionPenBlack, new Point(originX + EditImageScale - 1, originY), new Point(originX + EditImageScale - 1, originY + EditImageScale));
						}
						if (!selection.GetPoint((x, y - 1))) {
							drawingContext.DrawLine(selectionPenWhite, new Point(originX, originY), new Point(originX + EditImageScale, originY));
							drawingContext.DrawLine(selectionPenBlack, new Point(originX, originY), new Point(originX + EditImageScale, originY));
						}
						if (!selection.GetPoint((x, y + 1))) {
							drawingContext.DrawLine(selectionPenWhite, new Point(originX, originY + EditImageScale - 1), new Point(originX + EditImageScale, originY + EditImageScale - 1));
							drawingContext.DrawLine(selectionPenBlack, new Point(originX, originY + EditImageScale - 1), new Point(originX + EditImageScale, originY + EditImageScale - 1));
						}
					}
				}
			}
		}
		internal void RefreshOverlayImage() {
			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				if (ToggleGrid.IsChecked==true) {
					int x, y;
					Pen gridPen = new Pen(Brushes.WhiteSmoke,0.5);
					for (x = 0; x < overlayImageSource.PixelWidth; x+=EditImageScale) {
						drawingContext.DrawLine(gridPen, new Point(x, 0), new Point(x, overlayImageSource.PixelHeight));
					}
					for (y = 0; y < overlayImageSource.PixelHeight; y+= EditImageScale) {
						drawingContext.DrawLine(gridPen, new Point(0, y), new Point(overlayImageSource.PixelWidth, y));
					}
				}

				Pen selectionPenWhite;
				Pen selectionPenBlack;

				int shiftX=0, shiftY=0;
				if (toolPhase == EToolPhase.MoveFloatingSelection) {
					shiftX = ((int)Canvas.GetLeft(ImageFloatingSelection));
					shiftY = ((int)Canvas.GetTop(ImageFloatingSelection));
				}

				selectionPenWhite = new Pen(Brushes.White, 1);
				selectionPenWhite.DashStyle = new DashStyle(new double[] { 3,2 }, 0);
				selectionPenBlack = new Pen(Brushes.Black, 1);
				selectionPenBlack.DashStyle = new DashStyle(new double[] { 2, 3 }, 3);
				DrawSelectionBorder(selectedArea, selectionPenWhite, selectionPenBlack, drawingContext, shiftX, shiftY);

				selectionPenWhite = new Pen(Brushes.Yellow, 1);
				selectionPenWhite.DashStyle = new DashStyle(new double[] { 3, 2 }, 0);
				selectionPenBlack = new Pen(Brushes.Red, 1);
				selectionPenBlack.DashStyle = new DashStyle(new double[] { 2, 3 }, 3);
				DrawSelectionBorder(futureSelectedArea, selectionPenWhite, selectionPenBlack, drawingContext, shiftX, shiftY);
			}
			overlayImageSource.Clear();
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

			floatingSelection = new FloatingSelectionBitmap(spriteSheet);
			ImageFloatingSelection.Width = CompositeImageSize.Width * EditImageScale;
			ImageFloatingSelection.Height = CompositeImageSize.Height * EditImageScale;
			ImageFloatingSelection.Source = floatingSelection.bitmap;

			selectedArea = new Selection(CompositeImageSize.Width, CompositeImageSize.Height);
			futureSelectedArea = new Selection(CompositeImageSize.Width, CompositeImageSize.Height);

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
			undoSystem = new UndoSystem(spriteSheet, this);
			FrameMetadataChanged();
		}

		private void ToolColorPalette_OnSelectedColorChanged() {
			SelectedColorLeft.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedLeftColor]);
			SelectedColorMiddle.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedMiddleColor]);
			SelectedColorRight.Fill = new SolidColorBrush(spriteSheet.GetColorPalette().Colors[ToolColorPalette.SelectedRightColor]);
		}

		private void CutSelection() {
			floatingSelection = new FloatingSelectionBitmap(spriteSheet);
			int cutPixels = floatingSelection.CopyPixels(selectedArea, spriteSheet, GatherMetadata(), ListBoxLayers.SelectedIndex, true);
			if (cutPixels==0) {
				toolPhase = EToolPhase.None;
			} else {
				ImageFloatingSelection.Source = floatingSelection.bitmap;
			}
		}
		private void CopySelection() {
			FloatingSelectionBitmap copyBuffer = new FloatingSelectionBitmap(spriteSheet);
			int copiedPixels = copyBuffer.CopyPixels(selectedArea, spriteSheet, GatherMetadata(), ListBoxLayers.SelectedIndex);
			if (copiedPixels > 0) {
				Clipboard.Clear();
				DataObject data;
				data = new DataObject();
				data.SetData(DataFormats.Bitmap, copyBuffer.bitmap, true);
				UnitSpriteStudioClipboardFormat clipboardObject = new UnitSpriteStudioClipboardFormat(copyBuffer);
				data.SetData("UnitSpriteStudioClipboardFormat", clipboardObject,false);
				
				Clipboard.SetDataObject(data, true);
			}
		}
		private void PasteBuffer() {
			MergeFloatingSelection();
			IDataObject data = Clipboard.GetDataObject();
			string[] formats =data.GetFormats();
			FloatingSelectionBitmap newFloatingSelection = null;

			if (data.GetDataPresent("UnitSpriteStudioClipboardFormat",false)) {
				UnitSpriteStudioClipboardFormat clipboardObject =(UnitSpriteStudioClipboardFormat) data.GetData("UnitSpriteStudioClipboardFormat", false);
				newFloatingSelection = clipboardObject.GetFloatingSelection(spriteSheet);
			} else if (data.GetDataPresent(DataFormats.Bitmap, true)) {
				try {
					object BitmapData = data.GetData(DataFormats.Bitmap, true);
					System.Windows.Interop.InteropBitmap interopBitmap = (System.Windows.Interop.InteropBitmap)BitmapData;

					int CopyWidth = Math.Min(interopBitmap.PixelWidth, spriteSheet.drawingRoutine.CompositeImageSize().Width);
					int CopyHeight = Math.Min(interopBitmap.PixelHeight, spriteSheet.drawingRoutine.CompositeImageSize().Height);
					byte[] originalPixels = new byte[CopyWidth * CopyHeight * 4];
					interopBitmap.CopyPixels(new Int32Rect(0, 0, CopyWidth, CopyHeight), originalPixels, 4 * CopyWidth, 0);
					IList<Color> paletteWPF = spriteSheet.frameSource.GetUFOBattlescapePalette().Colors;
					List<System.Drawing.Color> palette = new List<System.Drawing.Color>();
					foreach (var c in paletteWPF) {
						palette.Add(System.Drawing.Color.FromArgb(c.A,c.R,c.G,c.B));
					}
					byte[] transformedPixels = new byte[CopyWidth * CopyHeight];
					int f;
					for (f = 0; f < transformedPixels.Length; f++) {
						byte a, r, g, b;
						a = originalPixels[f * 4 + 3];
						r = originalPixels[f * 4 + 2];
						g = originalPixels[f * 4 + 1];
						b = originalPixels[f * 4 + 0];
						var originalColor = System.Drawing.Color.FromArgb(a, r, g, b);

						int BestMatchIndex = -1;
						float BestMatchCloseness = 1000;
						for (int i = 0; i < palette.Count; i++) {
							float closeness = 0;
							float hueDelta = Math.Abs(originalColor.GetHue() - palette[i].GetHue());
							float saturationDelta = Math.Abs(originalColor.GetSaturation() - palette[i].GetSaturation());
							float brightnessDelta = Math.Abs(originalColor.GetBrightness() - palette[i].GetBrightness());
							closeness = hueDelta * 0.48f + saturationDelta * 0.25f + brightnessDelta * 0.25f;
							if (BestMatchIndex == -1 || BestMatchCloseness > closeness) {
								BestMatchCloseness = closeness;
								BestMatchIndex = i;
							}
						}
						transformedPixels[f] = (byte)BestMatchIndex;

					}


					newFloatingSelection = new FloatingSelectionBitmap(spriteSheet);
					newFloatingSelection.bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, CopyWidth, CopyHeight), transformedPixels, CopyWidth, 0);

				} catch (Exception exception) {
					MessageBox.Show("Something went wrong with the Paste: " + exception.Message + "\nThis content can't be pasted in.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			if (newFloatingSelection!=null) {
				undoSystem.RegisterUndoState();
				ResetFloatingSection();
				ImageFloatingSelection.Source = newFloatingSelection.bitmap;
				floatingSelection = newFloatingSelection;
				toolPhase = EToolPhase.MoveFloatingSelection;
				SelectFloatingSelection();
				RefreshOverlayImage();
			}
		}

		private void SelectFloatingSelection() {
			selectedArea.SetAll(false);
			var pixels = floatingSelection.GetAllPixels();
			for (int f=0;f<pixels.Length;f++) {
				if (pixels[f] != 0) selectedArea.AddPoint((f % floatingSelection.bitmap.PixelWidth, f / floatingSelection.bitmap.PixelWidth));
			}
		}

		private void Undo() {
			undoSystem.Undo();
			FrameMetadataChanged();
			RefreshOverlayImage();
		}
		private void Redo() {
			undoSystem.Redo();
			FrameMetadataChanged();
			RefreshOverlayImage();
		}
		private void MoveFloatingSelection(int deltaX,int deltaY) {
			Canvas.SetLeft(ImageFloatingSelection, Canvas.GetLeft(ImageFloatingSelection) + deltaX * EditImageScale);
			Canvas.SetTop(ImageFloatingSelection, Canvas.GetTop(ImageFloatingSelection)+deltaY*EditImageScale);
			RefreshOverlayImage();
		}
		private void ResetFloatingSection() {
			floatingSelection = new FloatingSelectionBitmap(spriteSheet);
			ImageFloatingSelection.Source = floatingSelection.bitmap;
			Canvas.SetLeft(ImageFloatingSelection, 0);
			Canvas.SetTop(ImageFloatingSelection, 0);
			FrameMetadataChanged();
		}
		private void Cut() {
			undoSystem.RegisterUndoState();
			toolPhase = EToolPhase.MoveFloatingSelection;
			CutSelection();
			RefreshCompositeImage();
		}
		private void ImageOverlay_MouseDown(object sender, MouseButtonEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / EditImageScale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / EditImageScale);

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
				if (toolPhase == EToolPhase.None) {
					Cut();
				} else if (toolPhase==EToolPhase.MoveFloatingSelection) {
					undoSystem.RegisterUndoState();
				}
			} else {
				MergeFloatingSelection();
				switch (CursorTool) {
					case ECursorTool.SelectPixels:
						if (e.ChangedButton == MouseButton.Left) {
							futureSelectedArea.AddPoint((PositionX, PositionY));
							RefreshOverlayImage();
						}
						break;
					case ECursorTool.SelectBox:
						if (e.ChangedButton == MouseButton.Left) {
							toolPhase = EToolPhase.Started;
							selectionStartX = PositionX;
							selectionStartY = PositionY;
							RefreshOverlayImage();
						}
						break;
					case ECursorTool.SelectLasso:
						break;
					case ECursorTool.SelectColors:
						break;
					case ECursorTool.SelectArea:
						break;
					case ECursorTool.Paint:
						if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) {
							byte ColorUnderCursor = spriteSheet.GetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY);
							if (e.ChangedButton == MouseButton.Left) ToolColorPalette.SelectedLeftColor = ColorUnderCursor;
							if (e.ChangedButton == MouseButton.Middle) ToolColorPalette.SelectedMiddleColor = ColorUnderCursor;
							if (e.ChangedButton == MouseButton.Right) ToolColorPalette.SelectedRightColor = ColorUnderCursor;
							ToolColorPalette.UpdateMarkers();
							ToolColorPalette_OnSelectedColorChanged();
						} else {
							byte ApplyColor = 0;
							if (toolPhase == EToolPhase.None) {
								undoSystem.RegisterUndoState();
							}
							toolPhase = EToolPhase.InProgress;
							if (e.ChangedButton == MouseButton.Left) ApplyColor = ToolColorPalette.SelectedLeftColor;
							if (e.ChangedButton == MouseButton.Middle) ApplyColor = ToolColorPalette.SelectedMiddleColor;
							if (e.ChangedButton == MouseButton.Right) ApplyColor = ToolColorPalette.SelectedRightColor;
							spriteSheet.SetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY, ApplyColor);
							RefreshCompositeImage();
						}
						break;
				}
			}
			lastCursorPositionX = PositionX;
			lastCursorPositionY = PositionY;
		}

		private void MergeFloatingSelection() {
			if (toolPhase == EToolPhase.MoveFloatingSelection) {
				undoSystem.RegisterUndoState();

				int shiftX, shiftY;
				shiftX = (int)Canvas.GetLeft(ImageFloatingSelection) / EditImageScale;
				shiftY = (int)Canvas.GetTop(ImageFloatingSelection) / EditImageScale;
				floatingSelection.PastePixels(spriteSheet, GatherMetadata(), ListBoxLayers.SelectedIndex, shiftX, shiftY);
				selectedArea.SetAll(false);
				ResetFloatingSection();
				RefreshOverlayImage();
				toolPhase = EToolPhase.None;
			}
		}

		private void ImageOverlay_MouseMove(object sender, MouseEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / EditImageScale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / EditImageScale);

			if (e.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && toolPhase == EToolPhase.MoveFloatingSelection) {
				if (lastCursorPositionY != PositionY || lastCursorPositionX != PositionX) {
					MoveFloatingSelection(PositionX - lastCursorPositionX, PositionY - lastCursorPositionY);
				}
			} else {
				switch (CursorTool) {
					case ECursorTool.SelectPixels:
						if (e.LeftButton == MouseButtonState.Pressed) {
							futureSelectedArea.AddPoint((PositionX, PositionY));
							RefreshOverlayImage();
						}
						break;
					case ECursorTool.SelectBox:
						if (toolPhase == EToolPhase.Started || toolPhase == EToolPhase.InProgress) {
							toolPhase = EToolPhase.InProgress;
							futureSelectedArea.SetAll(false);
							futureSelectedArea.AddRectangle(new Int32Rect(selectionStartX, selectionStartY, PositionX - selectionStartX + 1, PositionY - selectionStartY + 1));
							RefreshOverlayImage();
						}
						break;
					case ECursorTool.SelectLasso:
						break;
					case ECursorTool.SelectColors:
						break;
					case ECursorTool.SelectArea:
						break;
					case ECursorTool.Paint:
						if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) {
							byte ColorUnderCursor = spriteSheet.GetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY);
							if (e.LeftButton == MouseButtonState.Pressed) {
								ToolColorPalette.SelectedLeftColor = ColorUnderCursor;
							} else if (e.MiddleButton == MouseButtonState.Pressed) {
								ToolColorPalette.SelectedMiddleColor = ColorUnderCursor;
							} else if (e.RightButton == MouseButtonState.Pressed) {
								ToolColorPalette.SelectedRightColor = ColorUnderCursor;
							} else {
								break;
							}
							ToolColorPalette.UpdateMarkers();
							ToolColorPalette_OnSelectedColorChanged();
						} else {
							byte ApplyColor;
							if (e.LeftButton == MouseButtonState.Pressed) {
								ApplyColor = ToolColorPalette.SelectedLeftColor;
							} else if (e.MiddleButton == MouseButtonState.Pressed) {
								ApplyColor = ToolColorPalette.SelectedMiddleColor;
							} else if (e.RightButton == MouseButtonState.Pressed) {
								ApplyColor = ToolColorPalette.SelectedRightColor;
							} else {
								break;
							}
							spriteSheet.SetPixel(GatherMetadata(), ListBoxLayers.SelectedIndex, PositionX, PositionY, ApplyColor);
							RefreshCompositeImage();
						}
						break;
				}
			}
			lastCursorPositionX = PositionX;
			lastCursorPositionY = PositionY;
		}

		private void ImageOverlay_MouseUp(object sender, MouseButtonEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / EditImageScale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / EditImageScale);
			if (toolPhase == EToolPhase.MoveFloatingSelection) return;
			switch (CursorTool) {
				case ECursorTool.SelectPixels:
					goto default;
				case ECursorTool.SelectBox:
					if (toolPhase==EToolPhase.InProgress || toolPhase==EToolPhase.Started) {
						toolPhase = EToolPhase.None;
						goto default;
					}
					break;
				case ECursorTool.SelectLasso:
					break;
				case ECursorTool.SelectColors:
					break;
				case ECursorTool.SelectArea:
					break;
				case ECursorTool.Paint:
					if (e.LeftButton==MouseButtonState.Released && e.MiddleButton==MouseButtonState.Released && e.RightButton==MouseButtonState.Released) {
						toolPhase = EToolPhase.None;
					}
					break;
				case ECursorTool.Paste:
					break;
				default:
					undoSystem.RegisterUndoState();
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
						selectedArea.Add(futureSelectedArea);
					} else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) {
						selectedArea.Subtract(futureSelectedArea);
					} else {
						selectedArea.Copy(futureSelectedArea);
					}
					futureSelectedArea.SetAll(false);
					RefreshOverlayImage();
					break;
			}
		}

		private void UpdateControls() {
			SliderFrame.Maximum = spriteSheet.drawingRoutine.GetAnimationFrameCount(GatherMetadata()) - 1;
			ToolColorPalette_OnSelectedColorChanged();
		}
		private void FrameMetadataChanged() {
			if (SpriteSheetInitialized) {
				RefreshCompositeImage();
				UpdateControls();

				OnMetadataChanged?.Invoke();
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
			FrameMetadataChanged();
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
				try {
					openedSpriteSheet.LoadSprite(openFileDialog.FileName);
				} catch (Exception exception) {
					MessageBox.Show("An error has occured during loading the file: " + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
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

		private bool KeyModifier(ModifierKeys key) {
			if (Keyboard.Modifiers.HasFlag(key)) {
				switch (key) {
					case ModifierKeys.None:
						return !(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) || Keyboard.Modifiers.HasFlag(ModifierKeys.Windows));
					case ModifierKeys.Alt:
						return !(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) || Keyboard.Modifiers.HasFlag(ModifierKeys.Windows));
					case ModifierKeys.Control:
						return !(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) || Keyboard.Modifiers.HasFlag(ModifierKeys.Windows));
					case ModifierKeys.Shift:
						return !(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Windows));
					case ModifierKeys.Windows:
						return !(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
					default:
						return false;
				}
			} else {
				return false;
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if ((e.Key == Key.C && KeyModifier(ModifierKeys.Control)) || (e.Key == Key.Insert && KeyModifier(ModifierKeys.Control))) {
				CopySelection();
			}
			if ((e.Key == Key.V && KeyModifier(ModifierKeys.Control)) || (e.Key == Key.Insert && KeyModifier(ModifierKeys.Shift))) {
				PasteBuffer();
			}
			if (KeyModifier(ModifierKeys.Control)) {
				switch (e.Key) {
					case Key.Z:
						Undo();
						break;
					case Key.Y:
						Redo();
						break;
					case Key.D:
						ClearSelection();
						break;
					case Key.B:
						ToggleBackground.IsChecked = !ToggleBackground.IsChecked;
						FrameMetadataChanged();
						break;
					case Key.G:
						ToggleGrid.IsChecked = !ToggleGrid.IsChecked;
						RefreshOverlayImage();
						break;
					case Key.A:
						selectedArea.SetAll(true);
						RefreshOverlayImage();
						break;
				}
			}
			if (KeyModifier(ModifierKeys.None)) {
				switch (e.Key) {
					case Key.Delete:
						DeleteSelection();
						break;
					case Key.Insert:
						FillSelection();
						break;
					case Key.A:
						SetTool(ECursorTool.SelectPixels);
						break;
					case Key.S:
						SetTool(ECursorTool.SelectBox);
						break;
					case Key.D:
						SetTool(ECursorTool.SelectLasso);
						break;
					case Key.F:
						SetTool(ECursorTool.SelectColors);
						break;
					case Key.G:
						SetTool(ECursorTool.SelectArea);
						break;
					case Key.P:
					case Key.B:
						SetTool(ECursorTool.Paint);
						break;
					case Key.NumPad9:
						UnitDirectionControl.SetDirection(0);
						break;
					case Key.NumPad6:
						UnitDirectionControl.SetDirection(1);
						break;
					case Key.NumPad3:
						UnitDirectionControl.SetDirection(2);
						break;
					case Key.NumPad2:
						UnitDirectionControl.SetDirection(3);
						break;
					case Key.NumPad1:
						UnitDirectionControl.SetDirection(4);
						break;
					case Key.NumPad4:
						UnitDirectionControl.SetDirection(5);
						break;
					case Key.NumPad7:
						UnitDirectionControl.SetDirection(6);
						break;
					case Key.NumPad8:
						UnitDirectionControl.SetDirection(7);
						break;
					case Key.D1:
						ListBoxLayers.SelectedIndex = 0;
						break;
					case Key.D2:
						ListBoxLayers.SelectedIndex = 1;
						break;
					case Key.D3:
						ListBoxLayers.SelectedIndex = 2;
						break;
					case Key.D4:
						ListBoxLayers.SelectedIndex = 3;
						break;
					case Key.D5:
						ListBoxLayers.SelectedIndex = 4;
						break;
					case Key.D6:
						ListBoxLayers.SelectedIndex = 5;
						break;
				}
			}
		}

		private void DeleteSelection() {
			undoSystem.RegisterUndoState();
			if (toolPhase == EToolPhase.MoveFloatingSelection) {
				ResetFloatingSection();
				selectedArea.SetAll(false);
			} else {
				TransmogrifySelection((oldColor) => 0);
			}
		}
		private void FillSelection() {
			TransmogrifySelection((oldColor) => ToolColorPalette.SelectedLeftColor);
		}
		private void ClearSelection() {
			MergeFloatingSelection();
			selectedArea.SetAll(false);
			RefreshOverlayImage();
		}

		private void MenuItemCopy_Click(object sender, RoutedEventArgs e) {
			CopySelection();
		}

		private void MenuItemPaste_Click(object sender, RoutedEventArgs e) {
			PasteBuffer();
		}

		private void MenuItemUndo_Click(object sender, RoutedEventArgs e) {
			Undo();
		}

		private void MenuItemRedo_Click(object sender, RoutedEventArgs e) {
			Redo();
		}

		private void MenuItemCut_Click(object sender, RoutedEventArgs e) {
			Cut();
		}

		private void MenuItemGenerateMirrored_Click(object sender, RoutedEventArgs e) {
			MirroredSpritesGenerator generatorWindow = new MirroredSpritesGenerator();
			generatorWindow.Show();
		}

		private void MenuItemDelete_Click(object sender, RoutedEventArgs e) {
			DeleteSelection();
		}

		private void MenuItemFill_Click(object sender, RoutedEventArgs e) {
			FillSelection();
		}

		private void MenuItemCopyRulSnippet_Click(object sender, RoutedEventArgs e) {
			Clipboard.SetText(spriteSheet.GenerateRulSnippet());
		}

		private void TransmogrifySelection(Func<byte,byte> transmogrification) {
			MergeFloatingSelection();
			undoSystem.RegisterUndoState();
			DrawingRoutines.FrameMetadata metadata = GatherMetadata();
			int layer = ListBoxLayers.SelectedIndex;
			DrawingRoutines.DrawingRoutine.LayerFrameInfo frameInfo = spriteSheet.drawingRoutine.GetLayerFrame(metadata, layer);
			byte[] pixels = spriteSheet.frameSource.GetFramePixelData(frameInfo.Index);
			for (int x = 0; x < selectedArea.SizeX; x++) {
				for (int y = 0; y < selectedArea.SizeY; y++) {
					if (selectedArea.GetPoint((x, y))) {
						int FrameX = x - frameInfo.OffsetX;
						int FrameY = y - frameInfo.OffsetY;
						if (FrameX < 0 || FrameY < 0 || FrameX >= 32 || FrameY >= 40) return;

						pixels[FrameX + FrameY * 32] = transmogrification(pixels[FrameX + FrameY * 32]);
					}
				}
			}
			spriteSheet.frameSource.SetFramePixelData(frameInfo.Index, pixels);
			FrameMetadataChanged();
		}

		private void PaletteShiftLighter(object sender, RoutedEventArgs e) {
			TransmogrifySelection((oldColor) => {
				if (oldColor == 0) return 0;
				int newColor;
				newColor = oldColor;
				newColor = Math.Max(1, Math.Max(newColor - 1, (newColor / 16) * 16));
				return (byte)newColor;
			});
		}

		private void PaletteShiftDarker(object sender, RoutedEventArgs e) {
			TransmogrifySelection((oldColor) => {
				if (oldColor == 0) return 0;
				int newColor;
				newColor = oldColor;
				newColor = Math.Min(newColor + 1, ((newColor / 16) + 1) * 16 - 1);
				return (byte)newColor;
			});
		}

		private void PaletteShiftUp(object sender, RoutedEventArgs e) {
			TransmogrifySelection((oldColor) => {
				if (oldColor < 16) return oldColor;
				int newColor;
				newColor = oldColor;
				newColor = newColor - 16;
				if (newColor == 0) newColor = 1;
				return (byte)newColor;
			});
		}

		private void PaletteShiftDown(object sender, RoutedEventArgs e) {
			TransmogrifySelection((oldColor) => {
				if (oldColor == 0 || oldColor>=240) return oldColor;
				int newColor;
				newColor = oldColor;
				newColor = newColor + 16;
				return (byte)newColor;
			});
		}

		private void ToggleGrid_Click(object sender, RoutedEventArgs e) {
			RefreshOverlayImage();
		}

		private void SetTool(ECursorTool tool) {
			CursorTool = tool;
			foreach (var control in ToolBar.Children.OfType<System.Windows.Controls.Primitives.ToggleButton>()) {
				if (control.Tag != null) {
					control.IsChecked = (tool == (ECursorTool)int.Parse((string)control.Tag));
				}
			}
		}

		private void ToggleCursorToolClicked(object sender, RoutedEventArgs e) {
			var control = (Control)sender;
			if (control.Tag != null) {
				SetTool((ECursorTool)int.Parse((string)control.Tag));
			}
		}

		private void ToggleBackground_Click(object sender, RoutedEventArgs e) {
			FrameMetadataChanged();
		}
	}
}
