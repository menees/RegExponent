namespace RegExponent
{
	#region Using Directives

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
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using Menees;
	using Menees.Windows.Presentation;

	#endregion

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Private Data Members

		private readonly WindowSaver saver;

		#endregion

		#region Constructors

		public MainWindow()
		{
			this.InitializeComponent();
			this.saver = new WindowSaver(this);
			this.saver.LoadSettings += this.Saver_LoadSettings;
			this.saver.SaveSettings += this.Saver_SaveSettings;
		}

		#endregion

		#region Private Event Handlers

		private void ExitExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void HelpExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string text && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
			{
				WindowsUtility.ShellExecute(this, uri.ToString());
			}
		}

		private void AboutExecuted(object sender, RoutedEventArgs e)
		{
			WindowsUtility.ShowAboutBox(this, this.GetType().Assembly, nameof(RegExponent));
		}

		private void Saver_LoadSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Finish Saver_LoadSettings. [Bill, 3/22/2022]
			Conditions.RequireReference(this, nameof(MainWindow));
		}

		private void Saver_SaveSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Finish Saver_SaveSettings. [Bill, 3/22/2022]
			Conditions.RequireReference(this, nameof(MainWindow));
		}

		#endregion
	}
}
