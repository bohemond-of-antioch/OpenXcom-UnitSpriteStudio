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
	/// Interaction logic for ColorPalette.xaml
	/// </summary>
	public partial class ColorPalette : UserControl {
		public byte SelectedLeftColor;
		public byte SelectedMiddleColor;
		public byte SelectedRightColor;
		private BitmapPalette CurrentPalette;

		public event Action OnSelectedColorChanged;

		public ColorPalette() {
			InitializeComponent();
			int x, y;
			for (x=0;x<16;x++) {
				for (y=0;y<16;y++) {
					Rectangle colorSwatch = new Rectangle();
					colorSwatch.Fill = Brushes.White;
					colorSwatch.MouseUp += ColorSwatch_MouseUp;
					Grid.SetColumn(colorSwatch, x);
					Grid.SetRow(colorSwatch, y);
					colorSwatch.Tag = x + y * 16;
					SwatchGrid.Children.Add(colorSwatch);
				}
			}
			CurrentPalette =null;
		}

		public void UpdateMarkers() {
			Color selectedColor;
			Grid.SetColumn(LeftSelectedMarker, SelectedLeftColor % 16);
			Grid.SetRow(LeftSelectedMarker, SelectedLeftColor / 16);
			 selectedColor = CurrentPalette.Colors[SelectedLeftColor];
			if (System.Drawing.Color.FromArgb(selectedColor.R, selectedColor.G, selectedColor.B).GetBrightness() > 0.5) {
				LeftSelectedMarkerInner.Fill = Brushes.Black;
			} else {
				LeftSelectedMarkerInner.Fill = Brushes.White;
			}

			Grid.SetColumn(MiddleSelectedMarker, SelectedMiddleColor % 16);
			Grid.SetRow(MiddleSelectedMarker, SelectedMiddleColor / 16);
			 selectedColor = CurrentPalette.Colors[SelectedMiddleColor];
			if (System.Drawing.Color.FromArgb(selectedColor.R, selectedColor.G, selectedColor.B).GetBrightness() > 0.5) {
				MiddleSelectedMarkerInner.Fill = Brushes.Black;
			} else {
				MiddleSelectedMarkerInner.Fill = Brushes.White;
			}

			Grid.SetColumn(RightSelectedMarker, SelectedRightColor % 16);
			Grid.SetRow(RightSelectedMarker, SelectedRightColor / 16);
			 selectedColor = CurrentPalette.Colors[SelectedRightColor];
			if (System.Drawing.Color.FromArgb(selectedColor.R, selectedColor.G, selectedColor.B).GetBrightness() > 0.5) {
				RightSelectedMarkerInner.Fill = Brushes.Black;
			} else {
				RightSelectedMarkerInner.Fill = Brushes.White;
			}

		}

		private void ColorSwatch_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton==MouseButton.Left) SelectedLeftColor = (byte)(int)(((Rectangle)sender).Tag);
			if (e.ChangedButton == MouseButton.Middle) SelectedMiddleColor = (byte)(int)(((Rectangle)sender).Tag);
			if (e.ChangedButton == MouseButton.Right) SelectedRightColor = (byte)(int)(((Rectangle)sender).Tag);
			OnSelectedColorChanged?.Invoke();
			UpdateMarkers();
		}


		internal void ApplyPalette(BitmapPalette palette) {
			CurrentPalette = palette;
			foreach (Rectangle swatch in SwatchGrid.Children.OfType<Rectangle>()) {
				if ((int)(swatch.Tag)==0) {
					swatch.Fill = new SolidColorBrush(Color.FromRgb(0,255,0));
				} else {
					swatch.Fill = new SolidColorBrush(palette.Colors[(int)(swatch.Tag)]);
				}
			}
			
			UpdateMarkers();
		}
	}
}
