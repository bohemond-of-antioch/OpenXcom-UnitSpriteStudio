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

namespace UnitSpriteStudio.PixelOperations {
	public partial class ColorGroupBrightnessWindow : Window {
		ColorGroupBrightnessOperation boundOperation=null;
		public ColorGroupBrightnessWindow() {
			InitializeComponent();

			SourceColorGroup.ApplyPalette(((MainWindow)Application.Current.MainWindow).spriteSheet.GetColorPalette());

			SourceColorGroup.OnSelectedColorChanged += SourceColorGroup_OnSelectedColorChanged;

			((MainWindow)Application.Current.MainWindow).OnPippeteUsed += ApplicationWindow_OnPippeteUsed;
		}

		internal void BindOperation(ColorGroupBrightnessOperation operation) {
			ButtonAdd.Visibility = Visibility.Hidden;
			ButtonUpdate.Visibility = Visibility.Visible;

			SourceColorGroup.SetSelectedColor((byte)(operation.SourceGroup * 16));
			SliderChangeValue.Value = -operation.BrightnessAdjustment;
			boundOperation = operation;
		}

		protected override void OnClosed(EventArgs e) {
			((MainWindow)Application.Current.MainWindow).OnPippeteUsed -= ApplicationWindow_OnPippeteUsed;
		}

		private void ApplicationWindow_OnPippeteUsed(byte color) {
			SourceColorGroup.SetSelectedColor(color);
		}

		private void Refresh() {
			string Sign = "";
			if (SliderChangeValue.Value > 0) {
				Sign = "+";
			}
			LabelChangeValue.Content = string.Format("{0}{1}", Sign, (int)SliderChangeValue.Value);
		}

		private void SourceColorGroup_OnSelectedColorChanged() {
			Refresh();
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			ColorGroupBrightnessOperation operation = new ColorGroupBrightnessOperation((byte)(SourceColorGroup.SelectedColor / 16), -(int)SliderChangeValue.Value);
			((PixelOperationsWindow)Owner).AddOperation(operation);
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			((MainWindow)Application.Current.MainWindow).Window_KeyUp(sender, e);
		}

		private void SliderChangeValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			Refresh();
		}

		private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
			boundOperation.SourceGroup = (byte)(SourceColorGroup.SelectedColor / 16);
			boundOperation.BrightnessAdjustment = -(int)SliderChangeValue.Value;
			((PixelOperationsWindow)Owner).RefreshList();
		}
	}
}
