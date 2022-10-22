using System;
using System.Collections.Generic;
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

namespace UnitSpriteStudio {
	/// <summary>
	/// Interaction logic for PixelOperationsWindow.xaml
	/// </summary>
	public partial class PixelOperationsWindow : Window {
		MainWindow ApplicationWindow;

		private List<CheckBox> TargetPrimaryFrames, TargetSecondaryFrames, TargetTertiaryFrames, TargetLayers;

		internal struct STargetFrames {
			internal List<int> Primary, Secondary, Tertiary;
			internal List<int> Layers;
		}

		public PixelOperationsWindow() {
			InitializeComponent();

			ApplicationWindow = (MainWindow)Application.Current.MainWindow;

			string[] frameNames;
			frameNames = ApplicationWindow.spriteSheet.drawingRoutine.PrimaryFrameNames();
			TargetPrimaryFrames = new List<CheckBox>();
			foreach (string name in frameNames) {
				AddTargetCheckBox(PrimaryFramesPanel, TargetPrimaryFrames, name);
				if (name.Equals("Death")) TargetPrimaryFrames[TargetPrimaryFrames.Count - 1].Foreground=Brushes.Yellow;
			}
			frameNames = ApplicationWindow.spriteSheet.drawingRoutine.SecondaryFrameNames();
			TargetSecondaryFrames = new List<CheckBox>();
			foreach (string name in frameNames) {
				AddTargetCheckBox(SecondaryFramesPanel, TargetSecondaryFrames, name);
			}
			frameNames = ApplicationWindow.spriteSheet.drawingRoutine.TertiaryFrameNames();
			TargetTertiaryFrames = new List<CheckBox>();
			foreach (string name in frameNames) {
				AddTargetCheckBox(TertiaryFramesPanel, TargetTertiaryFrames, name);
			}
			var layerNames = ApplicationWindow.spriteSheet.drawingRoutine.LayerNames();
			TargetLayers = new List<CheckBox>();
			foreach (string name in layerNames) {
				AddTargetCheckBox(LayersPanel, TargetLayers, name);
			}
		}
		private STargetFrames GatherTargets() {
			STargetFrames targets = new STargetFrames();
			targets.Primary = new List<int>();
			targets.Secondary = new List<int>();
			targets.Tertiary = new List<int>();
			targets.Layers = new List<int>();

			for (int f = 0; f < TargetPrimaryFrames.Count; f++) {
				if (TargetPrimaryFrames[f].IsChecked == true) targets.Primary.Add(f);
			}
			for (int f = 0; f < TargetSecondaryFrames.Count; f++) {
				if (TargetSecondaryFrames[f].IsChecked == true) targets.Secondary.Add(f);
			}
			for (int f = 0; f < TargetTertiaryFrames.Count; f++) {
				if (TargetTertiaryFrames[f].IsChecked == true) targets.Tertiary.Add(f);
			}
			for (int f = 0; f < TargetLayers.Count; f++) {
				if (TargetLayers[f].IsChecked == true) targets.Layers.Add(f);
			}

			return targets;
		}
		private void ButtonColorGroup_Click(object sender, RoutedEventArgs e) {
			PixelOperations.ChangeColorGroupWindow toolWindow = new PixelOperations.ChangeColorGroupWindow();
			toolWindow.Owner = this;
			toolWindow.Show();
		}

		private void ButtonColorGroupBrightness_Click(object sender, RoutedEventArgs e) {
			PixelOperations.ColorGroupBrightnessWindow toolWindow = new PixelOperations.ColorGroupBrightnessWindow();
			toolWindow.Owner = this;
			toolWindow.Show();
		}

		private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
			if (ListOperations.SelectedIndex == -1) return;
			ListOperations.Items.RemoveAt(ListOperations.SelectedIndex);
		}
		private void ButtonClear_Click(object sender, RoutedEventArgs e) {
			ListOperations.Items.Clear();
		}

		private void ButtonMoveUp_Click(object sender, RoutedEventArgs e) {
			if (ListOperations.SelectedIndex == -1) return;
			int movedIndex = ListOperations.SelectedIndex;
			if (movedIndex == 0) return;
			PixelOperations.PixelOperation movedOperation = (PixelOperations.PixelOperation)ListOperations.SelectedItem;
			ListOperations.Items.RemoveAt(ListOperations.SelectedIndex);
			ListOperations.Items.Insert(movedIndex - 1, movedOperation);
			ListOperations.SelectedIndex = movedIndex - 1;
		}

		private void ButtonMoveDown_Click(object sender, RoutedEventArgs e) {
			if (ListOperations.SelectedIndex == -1) return;
			int movedIndex = ListOperations.SelectedIndex;
			if (movedIndex == ListOperations.Items.Count - 1) return;
			PixelOperations.PixelOperation movedOperation = (PixelOperations.PixelOperation)ListOperations.SelectedItem;
			ListOperations.Items.RemoveAt(ListOperations.SelectedIndex);
			ListOperations.Items.Insert(movedIndex + 1, movedOperation);
			ListOperations.SelectedIndex = movedIndex + 1;
		}

		private void ButtonRun_Click(object sender, RoutedEventArgs e) {
			STargetFrames targets = GatherTargets();
			UnitSpriteSheet spriteSheet = ApplicationWindow.spriteSheet;
			DrawingRoutines.DrawingRoutine drawingRoutine = spriteSheet.drawingRoutine;
			HashSet<int> targetFrames = new HashSet<int>();
			HashSet<int> targetItemFrames = new HashSet<int>();

			DrawingRoutines.FrameMetadata frameMetadata = new DrawingRoutines.FrameMetadata();
			foreach (int primary in targets.Primary) {
				frameMetadata.PrimaryFrame = primary;
				foreach (int secondary in targets.Secondary) {
					frameMetadata.SecondaryFrame = secondary;
					foreach (int tertiary in targets.Tertiary) {
						frameMetadata.TertiaryFrame = tertiary;
						for (int d = 0; d < 8; d++) {
							frameMetadata.Direction = d;
							for (int a = 0; a < drawingRoutine.GetAnimationFrameCount(frameMetadata); a++) {
								frameMetadata.AnimationFrame = a;
								foreach (int l in targets.Layers) {
									var frameInfo = ApplicationWindow.spriteSheet.drawingRoutine.GetLayerFrame(frameMetadata, l);
									switch (frameInfo.Target) {
										case DrawingRoutines.DrawingRoutine.LayerFrameInfo.ETarget.Unit:
											targetFrames.Add(frameInfo.Index);
											break;
										case DrawingRoutines.DrawingRoutine.LayerFrameInfo.ETarget.Item:
											targetItemFrames.Add(frameInfo.Index);
											break;
									}
								}
							}
						}
					}
				}
			}

			MainWindow.undoSystem.BeginUndoBlock();
			foreach (PixelOperations.PixelOperation operation in ListOperations.Items) {
				operation.Run(ApplicationWindow.spriteSheet, targetFrames.ToList<int>());
				if (ApplicationWindow.itemSpriteSheet!=null) operation.Run(ApplicationWindow.itemSpriteSheet, targetItemFrames.ToList<int>());
			}
			MainWindow.undoSystem.EndUndoBlock();
			ApplicationWindow.FrameMetadataChanged();
		}

		private void ListOperations_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (ListOperations.SelectedIndex == -1) return;
			((PixelOperations.PixelOperation)ListOperations.SelectedItem).Edit(this);
		}

		public void RefreshList() {
			ListOperations.Items.Refresh();
		}

		private void AddTargetCheckBox(Panel Parent, List<CheckBox> InternalList, string Label) {
			CheckBox newCheckBox = new CheckBox();
			newCheckBox.IsChecked = true;
			newCheckBox.Content = Label;
			Parent.Children.Add(newCheckBox);
			InternalList.Add(newCheckBox);
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			ApplicationWindow.Window_KeyUp(sender, e);
		}

		internal void AddOperation(PixelOperations.PixelOperation operation) {
			ListOperations.Items.Add(operation);
		}
	}
}
