using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using UnitSpriteStudio.Shading;

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for ShadeWindow.xaml
	/// </summary>
	public partial class ShadeWindow : Window {
		MainWindow ApplicationWindow;

		NormalMap CurrentNormalMap;
		PhongShader CurrentShader;

		bool UndoDisabled = false;
		bool Initialization = true;


		public ShadeWindow() {
			InitializeComponent();
			ApplicationWindow = (MainWindow)Application.Current.MainWindow;

			CurrentNormalMap = new NormalMap(ApplicationWindow.selectedArea);
			CurrentShader = new PhongShader(ApplicationWindow.selectedArea, CurrentNormalMap, ApplicationWindow.spriteSheet);



			UpdateControls();
			Initialization = false;
		}

		private void Run() {
			if (Initialization) return;
			try {
				ApplySettingsFromControls();
			} catch (Exception) {
				// One of the fields is not valid, just return.
				return;
			}
			ApplicationWindow.MergeFloatingSelection();
			DrawingRoutines.FrameMetadata metadata = ApplicationWindow.GatherMetadata();
			int[] AffectedLayers;
			if (ActionApplyToAllLayers.IsChecked == true) {
				AffectedLayers = Enumerable.Range(0, ApplicationWindow.spriteSheet.drawingRoutine.LayerNames().Length).ToArray();
				if (!UndoDisabled) MainWindow.undoSystem.BeginUndoBlock();
			} else {
				AffectedLayers = new int[] { ApplicationWindow.ListBoxLayers.SelectedIndex };
			}
			for (int l = 0; l < AffectedLayers.Length; l++) {
				if (!UndoDisabled) MainWindow.undoSystem.RegisterUndoState();

				DrawingRoutines.DrawingRoutine.LayerFrameInfo frameInfo = ApplicationWindow.spriteSheet.drawingRoutine.GetLayerFrame(metadata, l);
				CurrentNormalMap.Generate();
				CurrentShader.Shade(frameInfo);
			}
			ApplicationWindow.FrameMetadataChanged();
			if (!UndoDisabled) MainWindow.undoSystem.EndUndoBlock();
		}



		private void UpdateControls() {
			SurfacePower.Text = CurrentNormalMap.SurfaceCurvePower.ToString(CultureInfo.InvariantCulture);
			if (CurrentNormalMap.HeightCalculation == NormalMap.EHeightCalculation.Rational) {
				SurfaceHeightRational.IsChecked = true;
			} else {
				SurfaceHeightFixed.IsChecked = true;
			}
			SurfaceHeightParameter.Text = CurrentNormalMap.HeightParameter.ToString(CultureInfo.InvariantCulture);

			MaterialDiffuse.Text = CurrentShader.DiffuseReflection.ToString(CultureInfo.InvariantCulture);
			MaterialSpecular.Text = CurrentShader.SpecularReflection.ToString(CultureInfo.InvariantCulture);
			MaterialShininess.Text = CurrentShader.Shininess.ToString(CultureInfo.InvariantCulture);
			LabelShadeRange.Content = CurrentShader.ShadeRange.ToString(CultureInfo.InvariantCulture);
			MaterialShadeRange.Value = CurrentShader.ShadeRange;

			LightDirection.SetValue(CurrentShader.LightDirection);
			EyeDirection.SetValue(CurrentShader.EyeDirection);
		}



		private void ApplySettingsFromControls() {
			CurrentNormalMap.SurfaceCurvePower = double.Parse(SurfacePower.Text, CultureInfo.InvariantCulture);
			if (SurfaceHeightRational.IsChecked == true) {
				CurrentNormalMap.HeightCalculation = NormalMap.EHeightCalculation.Rational;
			} else {
				CurrentNormalMap.HeightCalculation = NormalMap.EHeightCalculation.Fixed;
			}
			CurrentNormalMap.HeightParameter = (float)double.Parse(SurfaceHeightParameter.Text, CultureInfo.InvariantCulture);

			CurrentShader.DiffuseReflection = (float)double.Parse(MaterialDiffuse.Text, CultureInfo.InvariantCulture);
			CurrentShader.SpecularReflection = (float)double.Parse(MaterialSpecular.Text, CultureInfo.InvariantCulture);
			CurrentShader.Shininess = double.Parse(MaterialShininess.Text, CultureInfo.InvariantCulture);
			CurrentShader.ShadeRange = (float)MaterialShadeRange.Value;

			CurrentShader.LightDirection = LightDirection.GetValue();
			CurrentShader.EyeDirection = EyeDirection.GetValue();
		}

		private void ButtonApply_Click(object sender, RoutedEventArgs e) {
			Run();
		}


		private void MaterialShadeRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			LabelShadeRange.Content = MaterialShadeRange.Value.ToString(CultureInfo.InvariantCulture);
			if (ActionAutoApply!=null && ActionAutoApply.IsChecked == true) Run();
		}

		private void Direction_ValueChanged(System.Windows.Media.Media3D.Vector3D value) {
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
		}

		private void Direction_ValueChangeStart() {
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) {
				MainWindow.undoSystem.BeginUndoBlock();
				UndoDisabled = true;
			}
		}

		private void Direction_ValueChangeEnd() {
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) {
				MainWindow.undoSystem.EndUndoBlock();
				UndoDisabled = false;
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			// TODO: This is a stupid solution, it needs some refactoring.
			// We let only Undo and Redo through.
			if (ApplicationWindow.IsRedoShortcut(e) || ApplicationWindow.IsUndoShortcut(e)) ApplicationWindow.Window_KeyUp(sender, e);
		}

		private void SettingControl_Click(object sender, RoutedEventArgs e) {
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
		}

		private void SettingControl_TextChanged(object sender, TextChangedEventArgs e) {
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
		}
	}
}
