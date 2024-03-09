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
			this.Replaced = string.Empty;
			this.Splits = [];
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<Match> Matches { get; private set; }

		public string Replaced { get; private set; }

		public string[] Splits { get; private set; }

		public Exception? Exception { get; private set; }

		public TimeSpan Elapsed { get; private set; }

		public int UpdateLevel { get; }

		public Benchmark? Benchmark { get; private set; }

		#endregion

		#region Public Methods

		public void Evaluate(Func<int> getLatestUpdateLevel)
		{
			this.Execute(getLatestUpdateLevel, expression =>
			{
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
			});
		}

		public void RunBenchmark(Func<int> getLatestUpdateLevel)
		{
			Benchmark benchmark = new();

			bool executed = this.Execute(getLatestUpdateLevel, expression =>
			{
				static Task Run(Action action) => Task.Factory.StartNew(
					action, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);

				List<Task> tasks =
				[
					Run(() => benchmark.IsMatchCount = RunIterations(benchmark, () => expression.IsMatch(this.input))),

					// Matches is lazily evaluated, so we'll pull the Count to force the collection to be fully populated.
					Run(() => benchmark.MatchesCount = RunIterations(benchmark, () => expression.Matches(this.input).Count.GetHashCode())),
				];

				if (this.mode == Mode.Replace)
				{
					if (this.UpdateLevel == getLatestUpdateLevel())
					{
						tasks.Add(Run(() => benchmark.ReplaceCount = RunIterations(benchmark, () => expression.Replace(this.input, this.replacement))));
					}
				}
				else if (this.mode == Mode.Split)
				{
					if (this.UpdateLevel == getLatestUpdateLevel())
					{
						tasks.Add(Run(() => benchmark.SplitCount = RunIterations(benchmark, () => expression.Split(this.input))));
					}
				}

				Task.WaitAll(tasks.ToArray());
			});

			if (executed)
			{
				benchmark.Comment = "Iter/sec for: " + this.pattern;
				this.Benchmark = benchmark;
			}
		}

		#endregion

		#region Private Methods

		private static void ValidateOptions(RegexOptions options)
		{
			List<string> incompatible = [];
			void Check(RegexOptions flag, string? name = null)
			{
				if (options.HasFlag(flag))
				{
					incompatible.Add(name ?? flag.ToString());
				}
			}

			void Validate(string optionName)
			{
				if (incompatible.Count > 0)
				{
					string message = $"The {optionName} option cannot be combined with {string.Join(", ", incompatible)}.";
					throw new ArgumentException(message);
				}
			}

			// We can throw exceptions with much better messages than .NET does for incompatible options.
			if (options.HasFlag(RegexOptions.ECMAScript))
			{
				// The ECMAScript option is only compatible with IgnoreCase and Multiline per MSDN, but in modern
				// .NET it also works with CultureInvariant.
				// https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#ecmascript-matching-behavior
				Check(RegexOptions.Singleline);
				Check(RegexOptions.ExplicitCapture, "Explicit Capture");
				Check(RegexOptions.IgnorePatternWhitespace, "Ignore Pattern Whitespace");
				Check(RegexOptions.RightToLeft, "Right To Left");
				Check(RegexOptions.NonBacktracking, "Non-Backtracking");
				Validate("ECMAScript");
			}
			else if (options.HasFlag(RegexOptions.NonBacktracking))
			{
				// NonBacktracking can't be used with RightToLeft or ECMAScript per
				// https://devblogs.microsoft.com/dotnet/regular-expression-improvements-in-dotnet-7/#backtracking-and-regexoptions-nonbacktracking
				Check(RegexOptions.RightToLeft, "Right To Left");
				Check(RegexOptions.ECMAScript, "ECMAScript"); // Redundant because it's checked above.
				Validate("Non-Backtracking");
			}
		}

		private static int RunIterations(Benchmark benchmark, Action runOneIteration)
		{
			int iterations = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (stopwatch.ElapsedTicks < benchmark.Timeout.Ticks)
			{
				runOneIteration();
				iterations++;
			}

			return iterations;
		}

		private bool Execute(Func<int> getLatestUpdateLevel, Action<Regex> applyRegex)
		{
			bool result = false;
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				ValidateOptions(this.options);

				// Evaluate even if input is empty so the pattern will be validated by the Regex constructor.
				if (this.pattern.IsNotEmpty()
					&& this.UpdateLevel == getLatestUpdateLevel())
				{
					Regex expression = new(this.pattern, this.options, this.timeout);
					applyRegex(expression);
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

		#endregion
	}
}
