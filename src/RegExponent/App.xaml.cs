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
	}
}
