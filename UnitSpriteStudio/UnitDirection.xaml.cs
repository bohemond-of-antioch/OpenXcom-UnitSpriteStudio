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
	/// Interaction logic for UnitDirection.xaml
	/// </summary>
	public partial class UnitDirection : UserControl {
		public int SelectedDirection = 0;
		private Brush normalBackground;
		private Brush selectedBackground;

		public event Action OnChanged;
		public UnitDirection() {
			InitializeComponent();
			normalBackground = buttonDirection0.Background;
			selectedBackground = Brushes.AliceBlue;
			UpdateButtons();
		}
		private void UpdateButtons() {
			buttonDirection0.Background = SelectedDirection == 0 ? selectedBackground : normalBackground;
			buttonDirection1.Background = SelectedDirection == 1 ? selectedBackground : normalBackground;
			buttonDirection2.Background = SelectedDirection == 2 ? selectedBackground : normalBackground;
			buttonDirection3.Background = SelectedDirection == 3 ? selectedBackground : normalBackground;
			buttonDirection4.Background = SelectedDirection == 4 ? selectedBackground : normalBackground;
			buttonDirection5.Background = SelectedDirection == 5 ? selectedBackground : normalBackground;
			buttonDirection6.Background = SelectedDirection == 6 ? selectedBackground : normalBackground;
			buttonDirection7.Background = SelectedDirection == 7 ? selectedBackground : normalBackground;
			OnChanged?.Invoke();
		}

		public void SetDirection(int direction) {
			SelectedDirection = direction;
			UpdateButtons();
		}

		private void buttonDirection0_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 0;
			UpdateButtons();
		}

		private void buttonDirection1_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 1;
			UpdateButtons();
		}

		private void buttonDirection2_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 2;
			UpdateButtons();
		}

		private void buttonDirection3_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 3;
			UpdateButtons();
		}

		private void buttonDirection4_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 4;
			UpdateButtons();
		}

		private void buttonDirection5_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 5;
			UpdateButtons();
		}

		private void buttonDirection6_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 6;
			UpdateButtons();
		}

		private void buttonDirection7_Click(object sender, RoutedEventArgs e) {
			SelectedDirection = 7;
			UpdateButtons();
		}
	}
}
