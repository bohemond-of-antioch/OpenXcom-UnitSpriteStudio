using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	abstract class DrawingRoutine {
		internal struct LayerFrameInfo {
			internal readonly bool Enabled;
			internal readonly int Index;
			internal readonly int OffsetX;
			internal readonly int OffsetY;

			public LayerFrameInfo(int index, int offsetX, int offsetY) {
				Index = index;
				OffsetX = offsetX;
				OffsetY = offsetY;
				Enabled = true;
			}

			public LayerFrameInfo(bool enabled) {
				Index = -1;
				OffsetX = 0;
				OffsetY = 0;
				Enabled = enabled;
			}
		}

		internal enum SmartLayerType {
			None,
			HWP
		}
		internal abstract string[] PrimaryFrameNames();
		internal abstract int[] PrimaryFrameMirroring();
		internal abstract string[] SecondaryFrameNames();
		internal abstract int[] SecondaryFrameMirroring();
		internal abstract string[] TertiaryFrameNames();
		internal abstract int[] TertiaryFrameMirroring();
		internal abstract string[] LayerNames();
		internal abstract int ChangeArmsLayer(int layer);
		internal abstract bool CanHoldItems();

		internal abstract (int Width, int Height) CompositeImageSize();
		internal virtual (int Width,int Height) FrameImageSize() { return (32,40); }
		internal abstract void DrawCompositeImage(FrameSource sprite,FrameMetadata metadata, DrawingContext drawingContext, int highlightedLayer);
		internal abstract Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata);

		internal abstract LayerFrameInfo GetLayerFrame(FrameMetadata metadata,int layer);
		internal abstract int GetAnimationFrameCount(FrameMetadata metadata);

		internal abstract (int Width, int Height) DefaultSpriteSheetSize();
		internal abstract BitmapPalette DefaultSpriteSheetPalette();

		internal virtual SmartLayerType SmartLayerSupported() {
			return SmartLayerType.None;
		}
		internal virtual bool ShowLayerThumbnails() {
			return true;
		}
		internal virtual int InitialEditImageScale() {
			return 10;
		}
		internal virtual FrameSource CreateFrameSource(BitmapSource sourceBitmap) {
			return new FrameSource(sourceBitmap);
		}

	}
}
