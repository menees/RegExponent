namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class CharacterClassNode : RegexNode
{
	public CharacterClassNode(bool neg) => this.IsNegated = neg;

	public bool IsNegated { get; }

	public List<CharacterClassItem> Items { get; } = [];

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitCharacterClass(this, indent);
}
