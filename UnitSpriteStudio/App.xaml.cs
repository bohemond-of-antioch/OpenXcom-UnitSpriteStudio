using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	partial class App : Application {
		private MainWindow mainWindow;
		private System.Collections.ObjectModel.ObservableCollection<AlternateFrame> BackingAlternateFrames;
		public System.Collections.ObjectModel.ObservableCollection<AlternateFrame> AlternateFrames {
			get {
				if (BackingAlternateFrames == null) {
					BackingAlternateFrames = new System.Collections.ObjectModel.ObservableCollection<AlternateFrame>();
				}
				return BackingAlternateFrames;
			}
		}
		private void Application_Startup(object sender, StartupEventArgs e) {
			mainWindow = new MainWindow();
			UnitSpriteSheet initialSheet = new UnitSpriteSheet(new DrawingRoutines.DrawingRoutineSoldier());
			//SpriteSheet initialSheet = new SpriteSheet(new DrawingRoutines.DrawingRoutineSectopod(), "D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Sectopod_Halved.png");
			//initialSheet.LoadSprite("D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Units\\FLIGHT_SUIT.png");
			//initialSheet.LoadSprite("D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Units\\zane_armor2.png");
			mainWindow.InitializeSpriteSheet(initialSheet);
			mainWindow.Show();
		}
	}
}
