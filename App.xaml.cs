using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SC2Inspector {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		/// <summary>
		/// Initializes the statup of the window.
		/// </summary>
		/// <param name="e">Startup Arguments</param>
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			MainWindow MainWindow = new MainWindow();
			MainWindow.Show();
		}
	}
}
