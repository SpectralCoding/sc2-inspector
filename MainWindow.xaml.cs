using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SC2Inspector.ViewModel;

namespace SC2Inspector {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private InspectorViewModel MasterIVM;

		public MainWindow(InspectorViewModel i_MasterIVM) {
			MasterIVM = i_MasterIVM;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			MasterIVM.LoadReplay(@"E:\Programming\C#\111228 SC2Inspector\{Support\Replays\(Z)LiquidSheth_vs_(T)EGPuMa__2011-12-30_11217.sc2replay");
			//MasterIVM.LoadReplay(@"E:\Programming\C#\111228 SC2Inspector\{Support\Replays\(T)mouzThorZaiN_vs_(T)EGPuMa__2011-12-30_11194.sc2replay");
			//MasterIVM.LoadReplay(@"E:\Programming\C#\111228 SC2Inspector\{Support\Replays\(Z)EGIdrA_vs_(T)EGPuMa__2011-12-30_11201.sc2replay");
			//MasterIVM.LoadReplay(@"E:\Programming\C#\111228 SC2Inspector\{Support\Replays\(P)LiquidHerO_vs_(P)MouzHasuObs__2011-12-30_11213.sc2replay");
			//MasterIVM.LoadReplay(@"E:\Programming\C#\111228 SC2Inspector\{Support\Replays\Callahan\110629 Semi-Fail Drop TvP Double Expand Rape.SC2Replay");
		}

		private void OpenReplayBtn_Click(object sender, RoutedEventArgs e) {
			Microsoft.Win32.OpenFileDialog OpenDiag = new Microsoft.Win32.OpenFileDialog();
			OpenDiag.FileName = "Document";
			OpenDiag.DefaultExt = ".SC2Replay";
			OpenDiag.Filter = "StarCraft II Replays (.SC2Replay)|*.SC2Replay";
			Nullable<bool> OpenResult = OpenDiag.ShowDialog();
			if (OpenResult == true) {
				string Filename = OpenDiag.FileName;
				MasterIVM.LoadReplay(Filename);
			}
		}
	}
}
