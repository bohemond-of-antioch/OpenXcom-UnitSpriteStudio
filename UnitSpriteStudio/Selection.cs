using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UnitSpriteStudio {
	class Selection {
		private BitArray selectedPixels;
		internal int SizeX, SizeY;

		public Selection(Selection other) {
			this.SizeX = other.SizeX;
			this.SizeY = other.SizeY;
			this.selectedPixels = new BitArray(other.selectedPixels);
		}

		public Selection(int SizeX, int SizeY) {
			this.SizeX = SizeX;
			this.SizeY = SizeY;
			this.selectedPixels = new BitArray(SizeX * SizeY);
		}

		public void SetAll(bool value) {
			this.selectedPixels.SetAll(value);
		}

		public void Copy(Selection other) {
			this.selectedPixels = new BitArray(other.selectedPixels);
		}

		public void Add(Selection other) {
			this.selectedPixels.Or(other.selectedPixels);
		}
		public void Subtract(Selection other) {
			this.selectedPixels.Not().Or(other.selectedPixels).Not();
		}

		public void AddRectangle(Int32Rect rectangle) {
			int x, y;
			if (rectangle.Width <= 0) {
				rectangle.X = rectangle.X + rectangle.Width - 1;
				rectangle.Width = -rectangle.Width + 2;
			}
			if (rectangle.Height <= 0) {
				rectangle.Y = rectangle.Y + rectangle.Height - 1;
				rectangle.Height = -rectangle.Height + 2;
			}
			for (x = rectangle.X; x < rectangle.X + rectangle.Width; x++) {
				for (y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++) {
					this.selectedPixels[x + y * SizeX] = true;
				}
			}
		}
		public void AddPoint((int X, int Y) point) {
			int index = point.X + point.Y * SizeX;
			if (index < 0 || index >= selectedPixels.Length) return;
			this.selectedPixels[index] = true;
		}

		public bool GetPoint(int X, int Y) {
			if (X < 0 || X >= SizeX || Y < 0 || Y >= SizeY) return false;
			return this.selectedPixels[X + Y * SizeX];
		}

	}
}
