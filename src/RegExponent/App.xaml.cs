namespace RegExponent
{
	#region Using Directives

	using System;
	using System.IO;
	using System.Windows;
	using System.Xml;
	using ICSharpCode.AvalonEdit.Highlighting;
	using ICSharpCode.AvalonEdit.Highlighting.Xshd;
	using Menees;
	using Menees.Windows.Presentation;

	#endregion

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		#region Constructors

		public App()
		{
			WindowsUtility.InitializeApplication(nameof(RegExponent), null, this.GetType().Assembly);

			RegisterHighlighting("Pattern");
			RegisterHighlighting("Replacement");
		}

		#endregion

		#region Protected Methods

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			MainWindow mainWindow = new(e.Args);
			mainWindow.Show();
		}

		#endregion

		#region Private Methods

		private static void RegisterHighlighting(string highlightName)
		{
			IHighlightingDefinition definition = LoadHighlightingDefinition(highlightName + ".xshd");

			// Based on https://stackoverflow.com/a/46512591/1882616
			HighlightingManager.Instance.RegisterHighlighting(highlightName, Array.Empty<string>(), definition);
		}

		private static IHighlightingDefinition LoadHighlightingDefinition(string resourceName)
		{
			// Based on https://stackoverflow.com/a/5057464/1882616.
			string fullName = $"{nameof(RegExponent)}.Highlights.{resourceName}";
			using (Stream stream = typeof(App).Assembly.GetManifestResourceStream(fullName)
				?? throw Exceptions.NewInvalidOperationException($"Could not find resource {fullName}."))
			using (XmlReader reader = XmlReader.Create(stream))
			{
				IHighlightingDefinition result = HighlightingLoader.Load(reader, HighlightingManager.Instance);
				return result;
			}
		}

		#endregion
	}
}
