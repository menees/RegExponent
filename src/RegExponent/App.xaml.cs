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
			ApplicationConfiguration.Initialize();
			WindowsUtility.InitializeApplication(nameof(RegExponent), null, this.GetType().Assembly);

			RegisterHighlighting("Pattern");
			RegisterHighlighting("PatternXMode");
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
			// Based on https://stackoverflow.com/a/5057464/1882616.
			string resourceName = $"{nameof(RegExponent)}.Highlights.{highlightName}.xshd";

			using (Stream stream = typeof(App).Assembly.GetManifestResourceStream(resourceName)
				?? throw Exceptions.NewInvalidOperationException($"Could not find resource {resourceName}."))
			using (XmlReader reader = XmlReader.Create(stream))
			{
				// Based on https://stackoverflow.com/a/46512591/1882616
				HighlightingManager highlighters = HighlightingManager.Instance;
				IHighlightingDefinition definition = HighlightingLoader.Load(reader, highlighters);
				highlighters.RegisterHighlighting(highlightName, [], definition);
			}
		}

		#endregion
	}
}
