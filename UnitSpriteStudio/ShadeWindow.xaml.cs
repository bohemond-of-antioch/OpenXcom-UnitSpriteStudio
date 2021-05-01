using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

			string[] presetFiles =System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory(), "*.ShaderPreset");
			foreach (var filePath in presetFiles) {
				ShaderPreset preset =ShaderPreset.ReadFromBinaryFile(filePath);
				ListPresets.Items.Add(preset);
			}

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

				DrawingRoutines.DrawingRoutine.LayerFrameInfo frameInfo = ApplicationWindow.spriteSheet.drawingRoutine.GetLayerFrame(metadata, AffectedLayers[l]);
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
			MaterialAmbientDarkness.Value = 16 - CurrentShader.AmbientDarkness;

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
			CurrentShader.AmbientDarkness = (float)(16 - MaterialAmbientDarkness.Value);

			CurrentShader.LightDirection = LightDirection.GetValue();
			CurrentShader.EyeDirection = EyeDirection.GetValue();
		}

		private void ButtonApply_Click(object sender, RoutedEventArgs e) {
			Run();
		}


		private void MaterialShadeRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			LabelShadeRange.Content = MaterialShadeRange.Value.ToString(CultureInfo.InvariantCulture);
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
		}
		private void MaterialAmbientDarkness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			LabelBrightness.Content = MaterialAmbientDarkness.Value.ToString(CultureInfo.InvariantCulture);
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
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

		private void ButtonPresetAdd_Click(object sender, RoutedEventArgs e) {
			ApplySettingsFromControls();
			var newPreset = new ShaderPreset(TextBoxPresetName.Text, CurrentNormalMap, CurrentShader);
			ListPresets.Items.Add(newPreset);
			ShaderPreset.WriteToBinaryFile(string.Format("{0}.ShaderPreset", TextBoxPresetName.Text), newPreset);
		}
		private void ButtonPresetSave_Click(object sender, RoutedEventArgs e) {
			if (ListPresets.SelectedIndex == -1) return;
			ApplySettingsFromControls();
			var newPreset = new ShaderPreset(TextBoxPresetName.Text, CurrentNormalMap, CurrentShader);
			ListPresets.Items[ListPresets.SelectedIndex] = newPreset;
			ShaderPreset.WriteToBinaryFile(string.Format("{0}.ShaderPreset", TextBoxPresetName.Text), newPreset);
		}

		private void ButtonPresetLoad_Click(object sender, RoutedEventArgs e) {
			if (ListPresets.SelectedIndex == -1) return;
			ShaderPreset preset = (ShaderPreset)ListPresets.SelectedItem;
			TextBoxPresetName.Text = preset.Name;
			preset.Apply(CurrentNormalMap, CurrentShader);
			Initialization = true;
			UpdateControls();
			Initialization = false;
			if (ActionAutoApply != null && ActionAutoApply.IsChecked == true) Run();
		}

		private void ButtonPresetDelete_Click(object sender, RoutedEventArgs e) {
			if (ListPresets.SelectedIndex == -1) return;
			System.IO.File.Delete(string.Format("{0}.ShaderPreset", ListPresets.SelectedItem));
			ListPresets.Items.RemoveAt(ListPresets.SelectedIndex);
		}

	}
}
