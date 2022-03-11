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
using System.Windows.Threading;

namespace UnitSpriteStudio.PixelOperations {
	/// <summary>
	/// Interaction logic for SelectColorGroup.xaml
	/// </summary>
	public partial class SelectColorGroup : UserControl {
		public byte SelectedColor;
		private BitmapPalette CurrentPalette;

		public event Action OnSelectedColorChanged;

		public SelectColorGroup() {
			InitializeComponent();

			int y;
			for (y = 0; y < 16; y++) {
				Rectangle colorSwatch = new Rectangle();
				colorSwatch.Fill = Brushes.White;
				colorSwatch.MouseUp += ColorSwatch_MouseUp;
				Grid.SetColumn(colorSwatch, 0);
				Grid.SetRow(colorSwatch, y);
				colorSwatch.Tag = 3+ y * 16;
				SwatchGrid.Children.Add(colorSwatch);
			}
			CurrentPalette = null;
		}
		private void UpdateMarkers() {
			Color selectedColor;
			Grid.SetRow(SelectedMarker, SelectedColor / 16);
			selectedColor = CurrentPalette.Colors[SelectedColor];
			if (System.Drawing.Color.FromArgb(selectedColor.R, selectedColor.G, selectedColor.B).GetBrightness() > 0.5) {
				LeftSelectedMarkerInner.Stroke = Brushes.Black;
			} else {
				LeftSelectedMarkerInner.Stroke = Brushes.White;
			}
		}

		public void SetSelectedColor(byte Color) {
			SelectedColor = Color;
			UpdateMarkers();
		}

		private void ColorSwatch_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) SelectedColor = (byte)(int)(((Rectangle)sender).Tag);
			UpdateMarkers();
			Dispatcher.Invoke(() => { OnSelectedColorChanged?.Invoke(); }, DispatcherPriority.ContextIdle);
		}
		internal void ApplyPalette(BitmapPalette palette) {
			CurrentPalette = palette;
			foreach (Rectangle swatch in SwatchGrid.Children.OfType<Rectangle>()) {
				if ((int)(swatch.Tag) == 0) {
					swatch.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
				} else {
					swatch.Fill = new SolidColorBrush(palette.Colors[(int)(swatch.Tag)]);
				}
			}

			UpdateMarkers();
		}
	}
}
