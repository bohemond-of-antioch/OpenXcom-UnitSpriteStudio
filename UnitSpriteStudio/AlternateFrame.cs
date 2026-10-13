using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitSpriteStudio {
	public class AlternateFrame {
		public string Name;
		public int ReplacedFrame;
		public int TargetFrame;
		public bool Enabled { get; set; }

		public AlternateFrame(string name, int replacedFrame, int targetFrame) {
			Name = name;
			ReplacedFrame = replacedFrame;
			TargetFrame = targetFrame;
		}

		public string Label {
			get {
				return ToString();
			}
		}
		public override string ToString() {
			if (Name.Equals("")) {
				return ReplacedFrame.ToString() + " -> " + TargetFrame.ToString();
			} else {
				return Name + " (" + TargetFrame.ToString() + ")";
			}
		}

		public bool Replaces(int frame) {
			return frame >= ReplacedFrame && frame < ReplacedFrame + 8;
		}

		public int GetTargetIndex(int frame) {
			return TargetFrame + frame - ReplacedFrame;
		}

		public static int CalculateTargetIndex(int frame) {
			foreach (AlternateFrame alternateFrame in ((App)App.Current).AlternateFrames) {
				if (alternateFrame.Enabled && alternateFrame.Replaces(frame)) {
					return alternateFrame.GetTargetIndex(frame);
				}
			}
			return frame;
		}
	}
}
