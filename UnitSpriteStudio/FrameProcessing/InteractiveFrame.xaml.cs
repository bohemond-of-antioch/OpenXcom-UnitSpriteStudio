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

namespace UnitSpriteStudio.FrameProcessing {
	/// <summary>
	/// Interaction logic for InteractiveFrame.xaml
	/// </summary>
	public partial class InteractiveFrame : UserControl {
		public event Action<InteractiveFrame, int, int, MouseButtonEventArgs> InteractionMouseDown;
		public event Action<InteractiveFrame, int, int, MouseEventArgs> InteractionMouseMove;
		public event Action<InteractiveFrame, int, int, MouseButtonEventArgs> InteractionMouseUp;

		public int ID;

		private int Scale;
		private BitmapSource Bitmap;

		public (int X, int Y) ReticlePosition { get; private set; }
		public (int X, int Y) SecondaryReticlePosition { get; private set; }

		public InteractiveFrame(int scale, BitmapSource bitmap) {
			InitializeComponent();

			Bitmap = bitmap;
			Scale = scale;

			ReticleHorizontal.Visibility = Visibility.Hidden;
			ReticleVertical.Visibility = Visibility.Hidden;

			SecondaryReticleHorizontal.Visibility = Visibility.Hidden;
			SecondaryReticleVertical.Visibility = Visibility.Hidden;

			ReticlePosition = (-1, -1);
			SecondaryReticlePosition = (-1, -1);
			SetBitmap(bitmap);
		}

		public void SetBitmap(BitmapSource bitmap) {
			ImageFrame.Width = bitmap.PixelWidth * Scale;
			ImageFrame.Height = bitmap.PixelHeight * Scale;
			ImageFrame.Source = bitmap;
		}

		public BitmapSource GetBitmap() {
			return (BitmapSource)ImageFrame.Source;
		}

		private void ImageFrame_MouseDown(object sender, MouseButtonEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / Scale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / Scale);

			if (e.ChangedButton == MouseButton.Left) MoveReticle(PositionX, PositionY);
			if (e.ChangedButton == MouseButton.Right) MoveSecondaryReticle(PositionX, PositionY);
			InteractionMouseDown?.Invoke(this,PositionX, PositionY, e);
		}
		private void ImageFrame_MouseMove(object sender, MouseEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / Scale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / Scale);

			if (e.LeftButton == MouseButtonState.Pressed) MoveReticle(PositionX, PositionY);
			if (e.RightButton == MouseButtonState.Pressed) MoveSecondaryReticle(PositionX, PositionY);
			InteractionMouseMove?.Invoke(this,PositionX, PositionY, e);
		}
		private void ImageFrame_MouseUp(object sender, MouseButtonEventArgs e) {
			int PositionX, PositionY;
			PositionX = (int)(e.GetPosition((IInputElement)sender).X / Scale);
			PositionY = (int)(e.GetPosition((IInputElement)sender).Y / Scale);

			if (e.ChangedButton == MouseButton.Left) MoveReticle(PositionX, PositionY);
			if (e.ChangedButton == MouseButton.Right) MoveSecondaryReticle(PositionX, PositionY);
			InteractionMouseUp?.Invoke(this, PositionX, PositionY, e);
		}

		public void MoveReticle(int x, int y) {
			ReticlePosition = (x, y);

			ReticleHorizontal.X1 = 0;
			ReticleHorizontal.Y1 = y * Scale + Scale / 2+2;
			ReticleHorizontal.X2 = MainCanvas.ActualWidth;
			ReticleHorizontal.Y2 = y * Scale + Scale / 2+2;

			ReticleVertical.X1 = x * Scale + Scale / 2+2;
			ReticleVertical.Y1 = 0;
			ReticleVertical.X2 = x * Scale + Scale / 2+2;
			ReticleVertical.Y2 = MainCanvas.ActualHeight;
		}
		public void EnableReticle(bool setting) {
			if (setting) {
				ReticleHorizontal.Visibility = Visibility.Visible;
				ReticleVertical.Visibility = Visibility.Visible;
			} else {
				ReticleHorizontal.Visibility = Visibility.Hidden;
				ReticleVertical.Visibility = Visibility.Hidden;
			}
		}
		public void MoveSecondaryReticle(int x, int y) {
			SecondaryReticlePosition = (x, y);

			double Length = Math.Max(ImageFrame.ActualWidth, ImageFrame.ActualHeight);

			SecondaryReticleHorizontal.X1 = x * Scale + Scale / 2+2 - Length;
			SecondaryReticleHorizontal.Y1 = y * Scale + Scale / 2+2 - Length;
			SecondaryReticleHorizontal.X2 = x * Scale + Scale / 2+2 + Length;
			SecondaryReticleHorizontal.Y2 = y * Scale + Scale / 2+2 + Length;

			SecondaryReticleVertical.X1 = x * Scale + Scale / 2+2 - Length;
			SecondaryReticleVertical.Y1 = y * Scale + Scale / 2+2 + Length;
			SecondaryReticleVertical.X2 = x * Scale + Scale / 2+2 + Length;
			SecondaryReticleVertical.Y2 = y * Scale + Scale / 2+2 - Length;
		}
		public void EnableSecondaryReticle(bool setting) {
			if (setting) {
				SecondaryReticleHorizontal.Visibility = Visibility.Visible;
				SecondaryReticleVertical.Visibility = Visibility.Visible;
			} else {
				SecondaryReticleHorizontal.Visibility = Visibility.Hidden;
				SecondaryReticleVertical.Visibility = Visibility.Hidden;
			}
		}

	}
}
