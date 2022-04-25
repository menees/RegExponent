namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Menees;

	#endregion

	internal sealed class Evaluator
	{
		#region Private Data Members

		private readonly string pattern;
		private readonly string replacement;
		private readonly string input;
		private readonly string newline;
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
			this.newline = model.Newline;

			this.timeout = timeout;
			this.UpdateLevel = updateLevel;

			this.Matches = Array.Empty<Match>();
			this.Replaced = Highlighter.Empty;
			this.Splits = Array.Empty<string>();
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<Match> Matches { get; private set; }

		public Highlighter Replaced { get; private set; }

		public string[] Splits { get; private set; }

		public Exception? Exception { get; private set; }

		public TimeSpan Elapsed { get; private set; }

		public int UpdateLevel { get; }

		public PatternHighlighter? Pattern { get; private set; }

		public InputHighlighter? Input { get; private set; }

		public ReplacementHighlighter? Replacement { get; private set; }

		#endregion

		#region Public Methods

		public bool Evaluate(Func<int> getLatestUpdateLevel)
		{
			bool result = false;
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
							this.Replaced = new Highlighter(expression.Replace(this.input, this.replacement), this.newline);
						}
					}
					else if (this.mode == Mode.Split)
					{
						if (this.UpdateLevel == getLatestUpdateLevel())
						{
							this.Splits = expression.Split(this.input);
						}
					}

					result = true;
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
			return result;
		}

		public void Highlight()
		{
			this.Pattern = new(this.pattern, this.newline, this.options.HasFlag(RegexOptions.IgnorePatternWhitespace));
			this.Input = new(this.input, this.newline, this.Matches);
			List<Highlighter> highlighters = new() { this.Pattern, this.Input };

			if (this.mode == Mode.Replace)
			{
				highlighters.Add(this.Replaced);
				this.Replacement = new(this.replacement, this.newline);
				highlighters.Add(this.Replacement);
			}

			Task[] tasks = highlighters.Select(h => Task.Run(h.Highlight)).ToArray();
			Task.WaitAll(tasks);
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
