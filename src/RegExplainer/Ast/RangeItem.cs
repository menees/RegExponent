namespace RegExplainer.Ast;

public sealed class RangeItem : CharacterClassItem
{
	public RangeItem(char f, char t)
	{
		this.From = f;
		this.To = t;
	}

	public char From { get; }

	public char To { get; }
}
