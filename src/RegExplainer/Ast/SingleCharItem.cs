namespace RegExplainer.Ast;

public sealed class SingleCharItem : CharacterClassItem
{
	public SingleCharItem(char c) => this.C = c;

	public char C { get; }
}
