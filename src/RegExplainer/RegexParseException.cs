namespace RegExplainer;

public sealed class RegexParseException : Exception
{
	public RegexParseException(string message, int? position = null, int? line = null, int? column = null)
		: base(position is not null ? $"{message} (pos {position} line {line} col {column})" : message)
	{
		this.Position = position;
		this.Line = line;
		this.Column = column;
	}

	public int? Position { get; }

	public int? Line { get; }

	public int? Column { get; }
}
