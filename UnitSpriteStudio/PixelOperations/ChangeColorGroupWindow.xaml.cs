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
	public partial class ChangeColorGroupWindow : Window {
		ChangeColorGroupOperation boundOperation = null;
		public ChangeColorGroupWindow() {
			InitializeComponent();

			SourceColorGroup.ApplyPalette(((MainWindow)Application.Current.MainWindow).spriteSheet.GetColorPalette());
			DestinationColorGroup.ApplyPalette(((MainWindow)Application.Current.MainWindow).spriteSheet.GetColorPalette());

			SourceColorGroup.OnSelectedColorChanged += SourceColorGroup_OnSelectedColorChanged;
			DestinationColorGroup.OnSelectedColorChanged += DestinationColorGroup_OnSelectedColorChanged;

			((MainWindow)Application.Current.MainWindow).OnPippeteUsed += ApplicationWindow_OnPippeteUsed;

			SourceColorGroup.SetSelectedColor(1);
			DestinationColorGroup.SetSelectedColor(1);
		}


		protected override void OnClosed(EventArgs e) {
			((MainWindow)Application.Current.MainWindow).OnPippeteUsed -= ApplicationWindow_OnPippeteUsed;
		}

		private void ApplicationWindow_OnPippeteUsed(byte color) {
			SourceColorGroup.SetSelectedColor(color);
			Dispatcher.Invoke(Refresh, System.Windows.Threading.DispatcherPriority.ContextIdle);
		}

		internal void BindOperation(ChangeColorGroupOperation operation) {
			ButtonAdd.Visibility = Visibility.Hidden;
			ButtonUpdate.Visibility = Visibility.Visible;

			SourceColorGroup.SetSelectedColor((byte)(operation.SourceGroup * 16));
			DestinationColorGroup.SetSelectedColor((byte)(operation.DestinationGroup * 16));
			boundOperation = operation;

			Dispatcher.Invoke(Refresh, System.Windows.Threading.DispatcherPriority.ContextIdle);
		}

		private void Refresh() {
			Point markerPosition = SourceColorGroup.SelectedMarker.TranslatePoint(new Point(SourceColorGroup.SelectedMarker.ActualWidth / 2, SourceColorGroup.SelectedMarker.ActualHeight/2), CanvasLinks);
			Canvas.SetLeft(Link, markerPosition.X);
			Canvas.SetTop(Link, markerPosition.Y);

			Point targetPosition = DestinationColorGroup.SelectedMarker.TranslatePoint(new Point(DestinationColorGroup.SelectedMarker.ActualWidth / 2, DestinationColorGroup.SelectedMarker.ActualHeight / 2), CanvasLinks);
			double deltaX, deltaY;
			deltaX = targetPosition.X - markerPosition.X;
			deltaY = targetPosition.Y - markerPosition.Y;
			LinkBezier.Point1 = new Point(deltaX / 3, 30);
			LinkBezier.Point2 = new Point(deltaX / 3 * 2, deltaY - 30);
			LinkBezier.Point3 = new Point(deltaX, deltaY);
		}

		private void DestinationColorGroup_OnSelectedColorChanged() {
			Refresh();
		}

		private void SourceColorGroup_OnSelectedColorChanged() {
			Refresh();
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			ChangeColorGroupOperation operation = new ChangeColorGroupOperation((byte)(SourceColorGroup.SelectedColor/16),(byte)(DestinationColorGroup.SelectedColor/16));
			((PixelOperationsWindow)Owner).AddOperation(operation);
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			((MainWindow)Application.Current.MainWindow).Window_KeyUp(sender, e);
		}

		private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
			boundOperation.SourceGroup = (byte)(SourceColorGroup.SelectedColor / 16);
			boundOperation.DestinationGroup = (byte)(DestinationColorGroup.SelectedColor / 16);
			((PixelOperationsWindow)Owner).RefreshList();
		}

		private void Window_Activated(object sender, EventArgs e) {
			Refresh();
		}
	}
}
