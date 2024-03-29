﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio {
	class ItemSpriteSheet:SpriteSheet {
		protected const int ItemBitmapWidth = 256;
		protected const int ItemBitmapHeight = 40;

		internal void ReloadFromSource() {
			if (sourceFileName != null) frameSource = new FrameSource(new BitmapImage(new Uri(sourceFileName, UriKind.Absolute)));
		}
		internal void Save(string FileName = "") {
			if (FileName.Equals("")) FileName = sourceFileName;
			sourceFileName = FileName;
			BitmapEncoder encoder;
			if (Path.GetExtension(FileName).Equals(".png", StringComparison.OrdinalIgnoreCase)) {
				encoder = frameSource.GetPNGSpriteEncoder();
			} else if (Path.GetExtension(FileName).Equals(".gif", StringComparison.OrdinalIgnoreCase)) {
				encoder = frameSource.GetGIFSpriteEncoder();
			} else {
				throw new Exception("Unsupported file type!");
			}
			using (var stream = new FileStream(FileName, FileMode.Create)) {
				encoder.Save(stream);
			}
		}

		internal ItemSpriteSheet(string FileName) {
			BitmapImage sourceImage = new BitmapImage(new Uri(FileName, UriKind.Absolute));
			if (sourceImage.PixelWidth != 256 || sourceImage.PixelHeight != 40) {
				throw new Exception("Item image must be 256x40 pixels.");
			}
			frameSource = new FrameSource(sourceImage);
			sourceFileName = FileName;
		}
		internal ImageSource GetFrame(int direction) {
			return frameSource.GetFrame(direction);
		}
		internal ItemSpriteSheet(System.Drawing.Bitmap SourceImage, BitmapPalette Palette) {
			IList<Color> PaletteWPF = Palette.Colors;
			List<System.Drawing.Color> PaletteList = new List<System.Drawing.Color>();
			foreach (var c in PaletteWPF) {
				PaletteList.Add(System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B));
			}
			var BitmapData = SourceImage.LockBits(new System.Drawing.Rectangle(0, 0, ItemBitmapWidth, ItemBitmapHeight), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			IntPtr BitmapDataScan0 = BitmapData.Scan0;
			int BitmapDataByteSize = Math.Abs(BitmapData.Stride) * SourceImage.Height;
			byte[] SourcePixels = new byte[BitmapDataByteSize];
			System.Runtime.InteropServices.Marshal.Copy(BitmapDataScan0, SourcePixels, 0, BitmapDataByteSize);

			byte[] transformedPixels = new byte[SourceImage.Width * SourceImage.Height];
			int f;
			for (f = 0; f < transformedPixels.Length; f++) {
				byte a, r, g, b;
				a = SourcePixels[f * 4 + 3];
				r = SourcePixels[f * 4 + 2];
				g = SourcePixels[f * 4 + 1];
				b = SourcePixels[f * 4 + 0];
				if (a == 0) {
					transformedPixels[f] = (byte)0;
				} else {
					var originalColor = System.Drawing.Color.FromArgb(a, r, g, b);
					int BestMatchIndex = Palettes.FindNearestColorRGB(originalColor, PaletteList);
					transformedPixels[f] = (byte)BestMatchIndex;
				}
			}
			SourceImage.UnlockBits(BitmapData);
			var destinationBitmap = new WriteableBitmap(ItemBitmapWidth, ItemBitmapHeight, 96, 96, PixelFormats.Indexed8, Palette);
			destinationBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, ItemBitmapWidth, ItemBitmapHeight), transformedPixels, ItemBitmapWidth, 0);
			frameSource = new FrameSource(destinationBitmap);
			sourceFileName = null;
		}
		internal ItemSpriteSheet() {
			var emptyBitmap = new WriteableBitmap(ItemBitmapWidth, ItemBitmapHeight, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256);
			frameSource = new FrameSource(emptyBitmap);
			sourceFileName = null;
		}

	}
}
