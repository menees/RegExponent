namespace RegExponent.Highlights;

internal class BracketMatch
{
	public required int OpeningOffset { get; init; }

	public int OpeningLength { get; init; } = 1;

	public required int ClosingOffset { get; init; }

	public int ClosingLength { get; init; } = 1;
}
