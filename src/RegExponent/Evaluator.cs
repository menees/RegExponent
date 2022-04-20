namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using Menees;

	#endregion

	internal sealed class Evaluator
	{
		#region Private Data Members

		private readonly string pattern;
		private readonly string replacement;
		private readonly string input;
		private readonly RegexOptions options;
		private readonly Mode mode;
		private readonly TimeSpan timeout;

		#endregion

		#region Constructors

		public Evaluator(Model model, TimeSpan timeout, int updateLevel)
		{
			// Copy all the model's values so another thread can't change them out from under us.
			this.pattern = model.Pattern;
			this.replacement = model.Replacement;
			this.input = model.Input;
			this.options = model.Options;
			this.mode = model.Mode;

			this.timeout = timeout;
			this.UpdateLevel = updateLevel;

			this.Matches = Array.Empty<Match>();
			this.Replaced = string.Empty;
			this.Splits = Array.Empty<string>();
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<Match> Matches { get; private set; }

		public string Replaced { get; private set; }

		public string[] Splits { get; private set; }

		public Exception? Exception { get; private set; }

		public TimeSpan Elapsed { get; private set; }

		public int UpdateLevel { get; }

		#endregion

		#region Public Methods

		public void Evaluate(Func<int> getLatestUpdateLevel)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ValidateOptions(this.options);

				if (this.pattern.IsNotEmpty()
					&& this.input.IsNotEmpty()
					&& this.UpdateLevel == getLatestUpdateLevel())
				{
					Regex expression = new(this.pattern, this.options, this.timeout);
					this.Matches = expression.Matches(this.input);

					if (this.mode == Mode.Replace)
					{
						if (this.UpdateLevel == getLatestUpdateLevel())
						{
							this.Replaced = expression.Replace(this.input, this.replacement);
						}
					}
					else if (this.mode == Mode.Split)
					{
						if (this.UpdateLevel == getLatestUpdateLevel())
						{
							this.Splits = expression.Split(this.input);
						}
					}
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
		}

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
