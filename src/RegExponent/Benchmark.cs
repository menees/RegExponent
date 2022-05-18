namespace RegExponent
{
	#region Using Directives

	using System;

	#endregion

	internal sealed class Benchmark
	{
		#region Constructors

		public Benchmark(TimeSpan? timeout = null)
		{
			this.Timeout = timeout ?? DefaultTimeout;
		}

		#endregion

		#region Public Properties

		public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(1);

		public TimeSpan Timeout { get; }

		public string FormattedTimeout
		{
			get
			{
				double seconds = this.Timeout.TotalSeconds;
				string result = $"{seconds} second{(seconds == 1 ? string.Empty : "s")}";
				return result;
			}
		}

		public int Index { get; set; }

		public int? IsMatchCount { get; set; }

		public int? MatchesCount { get; set; }

		public int? ReplaceCount { get; set; }

		public int? SplitCount { get; set; }

		public string? Comment { get; set; }

		#endregion
	}
}
