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
			internal readonly int Index;
			internal readonly int OffsetX;
			internal readonly int OffsetY;

			public LayerFrameInfo(int index, int offsetX, int offsetY) {
				Index = index;
				OffsetX = offsetX;
				OffsetY = offsetY;
			}
		}
		internal abstract string[] PrimaryFrameNames();
		internal abstract string[] SecondaryFrameNames();
		internal abstract string[] TertiaryFrameNames();
		internal abstract string[] LayerNames();

		internal abstract bool CanHoldItems();

		internal abstract (int Width, int Height) CompositeImageSize();
		internal abstract void DrawCompositeImage(SpriteSheet.FrameSource sprite,FrameMetadata metadata, DrawingContext drawingContext, int highlightedLayer);

		internal abstract LayerFrameInfo GetLayerFrame(FrameMetadata metadata,int layer);
		internal abstract int GetAnimationFrameCount(FrameMetadata metadata);

	}
}
