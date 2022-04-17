namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	#endregion

	internal sealed class Evaluation
	{
		#region Constructors

		public Evaluation(Model model, RegexOptions options, TimeSpan timeout)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ValidateOptions(options);

				Regex expression = new(model.Pattern, options, timeout);
				this.Matches = expression.Matches(model.Input);

				if (model.InReplaceMode)
				{
					this.Replaced = expression.Replace(model.Input, model.Replacement);
				}
				else if (model.InSplitMode)
				{
					this.Splits = expression.Split(model.Input);
				}
			}
			catch (ArgumentException ex)
			{
				this.Exception = ex;
			}
			catch (RegexMatchTimeoutException ex)
			{
				this.Exception = ex;
			}

			this.Elapsed = stopwatch.Elapsed;
			this.Matches ??= Array.Empty<Match>();
			this.Replaced ??= string.Empty;
			this.Splits ??= Array.Empty<string>();
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<Match> Matches { get; }

		public string Replaced { get; }

		public string[] Splits { get; }

		public Exception? Exception { get; }

		public TimeSpan Elapsed { get; }

		#endregion

		#region Private Methods

		private static void ValidateOptions(RegexOptions options)
		{
			// We can throw an exception with a much better message than .NET does for incompatible options.
			// The ECMAScript option is only compatible with IgnoreCase and Multiline per MSDN, but in modern
			// .NET it also works with CultureInvariant.
			// https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#ecmascript-matching-behavior
			if (options.HasFlag(RegexOptions.ECMAScript))
			{
				List<string> incompatible = new();
				void Check(RegexOptions flag, string? name = null)
				{
					if (options.HasFlag(flag))
					{
						incompatible.Add(name ?? flag.ToString());
					}
				}

				Check(RegexOptions.Singleline);
				Check(RegexOptions.ExplicitCapture, "Explicit Capture");
				Check(RegexOptions.IgnorePatternWhitespace, "Ignore Pattern Whitespace");
				Check(RegexOptions.RightToLeft, "Right To Left");

				if (incompatible.Count > 0)
				{
					string message = $"The ECMAScript option cannot be combined with {string.Join(", ", incompatible)}.";
					throw new ArgumentException(message);
				}
			}
		}

		#endregion
	}
}
