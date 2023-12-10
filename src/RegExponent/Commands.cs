namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Input;

	#endregion

	internal static class Commands
	{
		#region Public Fields

		public static readonly RoutedUICommand About = new(nameof(About), nameof(About), typeof(Commands));

		public static readonly RoutedUICommand Exit = new(nameof(Exit), nameof(Exit), typeof(Commands));

		public static readonly RoutedUICommand Font = new(nameof(Font), nameof(Font), typeof(Commands));

		public static readonly RoutedUICommand InsertInlineOptions
			= new(nameof(InsertInlineOptions), nameof(InsertInlineOptions), typeof(Commands));

		public static readonly RoutedUICommand GenerateCodeToClipboard
			= new(nameof(GenerateCodeToClipboard), nameof(GenerateCodeToClipboard), typeof(Commands));

		public static readonly RoutedUICommand RunBenchmark = new(nameof(RunBenchmark), nameof(RunBenchmark), typeof(Commands));
		public static readonly RoutedUICommand ViewBenchmarks = new(nameof(ViewBenchmarks), nameof(ViewBenchmarks), typeof(Commands));

		public static readonly RoutedUICommand ShellExecute = new(nameof(ShellExecute), nameof(ShellExecute), typeof(Commands));

		public static readonly RoutedUICommand InsertPattern = new(nameof(InsertPattern), nameof(InsertPattern), typeof(Commands));

		public static readonly RoutedUICommand Match = new(nameof(Match), nameof(Match), typeof(Commands));
		public static readonly RoutedUICommand Replace = new(nameof(Replace), nameof(Replace), typeof(Commands));
		public static readonly RoutedUICommand Split = new(nameof(Split), nameof(Split), typeof(Commands));
		public static readonly RoutedUICommand WindowsNewline = new(nameof(WindowsNewline), nameof(WindowsNewline), typeof(Commands));
		public static readonly RoutedUICommand UnixNewline = new(nameof(UnixNewline), nameof(UnixNewline), typeof(Commands));

		public static readonly RoutedUICommand Option1 = new(nameof(Option1), nameof(Option1), typeof(Commands));
		public static readonly RoutedUICommand Option2 = new(nameof(Option2), nameof(Option2), typeof(Commands));
		public static readonly RoutedUICommand Option3 = new(nameof(Option3), nameof(Option3), typeof(Commands));
		public static readonly RoutedUICommand Option4 = new(nameof(Option4), nameof(Option4), typeof(Commands));
		public static readonly RoutedUICommand Option5 = new(nameof(Option5), nameof(Option5), typeof(Commands));
		public static readonly RoutedUICommand Option6 = new(nameof(Option6), nameof(Option6), typeof(Commands));
		public static readonly RoutedUICommand Option7 = new(nameof(Option7), nameof(Option7), typeof(Commands));
		public static readonly RoutedUICommand Option8 = new(nameof(Option8), nameof(Option8), typeof(Commands));
		public static readonly RoutedUICommand Option9 = new(nameof(Option9), nameof(Option9), typeof(Commands));

		#endregion
	}
}
