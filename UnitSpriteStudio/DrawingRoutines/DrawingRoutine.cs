using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnitSpriteStudio.DrawingRoutines {
	abstract class DrawingRoutine {
		public static readonly Dictionary<int, Func<DrawingRoutine>> ClassConstructors = new Dictionary<int, Func<DrawingRoutine>> {
			{ 0,()=>new DrawingRoutineSoldier() },
			{ 1,()=>new DrawingRoutineFloater() },
			{ 2,()=>new DrawingRoutineTank() },
			{ 4,()=>new DrawingRoutineEthereal() },
			{ 5,()=>new DrawingRoutineSectopod() },
			{ 6,()=>new DrawingRoutineSnakeman() },
			{ 7,()=>new DrawingRoutineChryssalid() },
			{ 8,()=>new DrawingRoutineSilacoid() },
			{ 9,()=>new DrawingRoutineCelatid() },
			{ 10,()=>new DrawingRoutineMuton() }
		};
		internal struct LayerFrameInfo {
			internal enum ETarget {
				Unit,
				Item,
				None
			}
			internal readonly int Index;
			internal readonly int OffsetX;
			internal readonly int OffsetY;
			internal readonly ETarget Target;

			public LayerFrameInfo(int index, int offsetX, int offsetY, ETarget target = ETarget.Unit) {
				Index = index;
				OffsetX = offsetX;
				OffsetY = offsetY;
				Target = target;
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
		internal virtual (int Width, int Height) FrameImageSize() { return (32, 40); }
		internal abstract void DrawCompositeImage(FrameSource sprite, FrameMetadata metadata, ItemSpriteSheet itemSprite, DrawingContext drawingContext, int highlightedLayer);
		internal abstract Selection GetCompositeOutline(FrameSource sprite, FrameMetadata metadata);

		internal abstract LayerFrameInfo GetLayerFrame(FrameMetadata metadata, int layer);
		internal abstract int GetAnimationFrameCount(FrameMetadata metadata);

		internal abstract (int Width, int Height) DefaultSpriteSheetSize();
		internal abstract BitmapPalette DefaultSpriteSheetPalette();

		internal virtual SmartLayerType SmartLayerSupported() {
			return SmartLayerType.None;
		}
		internal virtual bool ItemSupported() {
			return false;
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
		internal virtual List<string> SupportedRuleValues() {
			return new List<string>();
		}
		internal virtual void SetRuleValue(string Name, string Value) {
		}
		internal virtual string GetRuleValue(string Name) {
			return "";
		}
	}
}
