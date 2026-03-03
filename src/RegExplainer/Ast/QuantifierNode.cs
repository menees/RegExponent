namespace RegExplainer.Ast;

public sealed class QuantifierNode : RegexNode
{
	public QuantifierNode(RegexNode target, int min, int? max, bool isLazy)
	{
		this.Target = target;
		this.Min = min;
		this.Max = max;
		this.IsLazy = isLazy;
	}

	public bool IsLazy { get; }

	public int? Max { get; }

	public int Min { get; }

	public RegexNode Target { get; }

	public override void Accept(Visitor.IRegexVisitor visitor, int indent) => visitor.VisitQuantifier(this, indent);
}
