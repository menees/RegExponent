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

		public static readonly RoutedUICommand Exit = new(
			nameof(Exit),
			nameof(Exit),
			typeof(Commands),
			new InputGestureCollection { new KeyGesture(Key.F4, ModifierKeys.Alt), });

		#endregion
	}
}
