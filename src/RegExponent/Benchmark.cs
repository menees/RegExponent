namespace RegExponent
{
	#region Using Directives

	using System;

	#endregion

	internal sealed class Benchmark
	{
		#region Constructors

		public Benchmark(TimeSpan timeout)
		{
			this.Timeout = timeout;
		}

		#endregion

		#region Public Properties

		public TimeSpan Timeout { get; }

		public int IsMatchCount { get; set; }

		public int MatchesCount { get; set; }

		public int? ReplaceCount { get; set; }

		public int? SplitCount { get; set; }

		#endregion
	}
}
