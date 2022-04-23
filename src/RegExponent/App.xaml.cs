namespace RegExponent
{
	using System.Windows;
	using Menees.Windows.Presentation;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			WindowsUtility.InitializeApplication(nameof(RegExponent), null, this.GetType().Assembly);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			MainWindow mainWindow = new();

			if (e.Args.Length == 1)
			{
				mainWindow.CurrentFileName = e.Args[0];
			}

			mainWindow.Show();
		}
	}
}
