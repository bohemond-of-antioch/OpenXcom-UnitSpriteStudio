using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitSpriteStudio.MirroredFramesGenerator {
	class MirroredDirectionsOperation {
		internal struct Direction {
			internal int Source;
			internal int Destination;
			internal bool Active;
			internal bool Mirror;
			internal bool Flip;
			internal bool ChangeArms;
		}

		internal bool EntireAnimation;
		internal bool EntireSprite;
		internal Direction[] Directions = new Direction[8];

		public MirroredDirectionsOperation() {
			ApplyDefaultSettings();
		}

		public void ApplyDefaultSettings() {
			for (int f = 0; f < 8; f++) {
				Directions[f].Source = f;
				Directions[f].Destination = 0;
				Directions[f].Active = false;
				Directions[f].Mirror = true;
				Directions[f].Flip = false;
				Directions[f].ChangeArms = true;
			}
		}

		public void ApplyPresetRightToLeft() {
			ApplyDefaultSettings();
			Directions[0].Destination = 6;
			Directions[0].Active = true;
			Directions[1].Destination = 5;
			Directions[1].Active = true;
			Directions[2].Destination = 4;
			Directions[2].Active = true;
		}
		public void ApplyPresetLeftToRight() {
			ApplyDefaultSettings();
			Directions[6].Destination = 0;
			Directions[6].Active = true;
			Directions[5].Destination = 1;
			Directions[5].Active = true;
			Directions[4].Destination = 2;
			Directions[4].Active = true;
		}
	}
}
