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
		private void Application_Startup(object sender, StartupEventArgs e) {
			MainWindow mainWindow = new MainWindow();
			SpriteSheet initialSheet = new SpriteSheet(new DrawingRoutines.DrawingRoutineSectopod(), "D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Sectopod_Halved.png");
			//initialSheet.LoadSprite("D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Units\\FLIGHT_SUIT.png");
			//initialSheet.LoadSprite("D:\\!Honza!\\OpenXCom - Anabasis\\Assets\\Units\\zane_armor2.png");
			mainWindow.InitializeSpriteSheet(initialSheet);
			mainWindow.Show();

		}
	}
}
