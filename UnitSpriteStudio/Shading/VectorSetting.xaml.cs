using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UnitSpriteStudio.Shading {
	/// <summary>
	/// Interaction logic for VectorSetting.xaml
	/// </summary>
	public partial class VectorSetting : UserControl {
		public event Action ValueChangeStart;
		public event Action<Vector3D> ValueChanged;
		public event Action ValueChangeEnd;
		public VectorSetting() {
			InitializeComponent();
		}

		

		public Vector3D GetValue() {
			double PositionX = (DirectionLine.X2 - 50) / 50;
			double PositionY = (DirectionLine.Y2 - 50) / 50;

			double PositionZ = Math.Sqrt(1 - PositionX * PositionX - PositionY * PositionY);
			if (double.IsNaN(PositionZ)) PositionZ = 0;
			Vector3D NormalizedResult = new Vector3D(PositionX, PositionY, PositionZ);
			NormalizedResult.Normalize();
			return NormalizedResult;
		}


		public void SetValue(Vector3D value) {
			value.Normalize();
			DirectionLine.X2 = value.X * 50 + 50;
			DirectionLine.Y2 = value.Y * 50 + 50;
			UpdateReadouts();
		}

		private void SetRawValue(Point position) {
			position.X = (position.X - 50) / 50;
			position.Y = (position.Y - 50) / 50;
			double length =Math.Sqrt( position.X * position.X + position.Y * position.Y);
			if (length > 1) {
				position.X = position.X / length;
				position.Y = position.Y / length;
			}
			DirectionLine.X2 = position.X*50+50;
			DirectionLine.Y2 = position.Y*50+50;
			UpdateReadouts();
		}

		private void UpdateReadouts() {
			Vector3D currentValue = GetValue();
			TextBoxVectorX.Text = string.Format(CultureInfo.InvariantCulture,"{0:F3}", currentValue.X);
			TextBoxVectorY.Text = string.Format(CultureInfo.InvariantCulture, "{0:F3}", currentValue.Y);
			LabelVectorZ.Content = string.Format(CultureInfo.InvariantCulture, "{0:F3}", currentValue.Z);
		}

		private void Circle_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				SetRawValue(e.GetPosition(this));
				((UIElement)sender).CaptureMouse();
				ValueChanged?.Invoke(GetValue());
			}
		}

		private void Circle_MouseMove(object sender, MouseEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				SetRawValue(e.GetPosition(this));
				ValueChanged?.Invoke(GetValue());
			}
		}
		private void Circle_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				((UIElement)sender).ReleaseMouseCapture();
			}
		}

		private void Ellipse_GotMouseCapture(object sender, MouseEventArgs e) {
			ValueChangeStart?.Invoke();
		}

		private void Ellipse_LostMouseCapture(object sender, MouseEventArgs e) {
			ValueChangeEnd?.Invoke();
		}
	}
}
