namespace RegExplainer.Ast;

public sealed class ConditionalNode : RegexNode
{
	public ConditionalNode(string condition, RegexNode trueBranch, RegexNode? falseBranch)
	{
		this.Condition = condition;
		this.TrueBranch = trueBranch;
		this.FalseBranch = falseBranch;
	}

	public string Condition { get; }

	public RegexNode TrueBranch { get; }

	public RegexNode? FalseBranch { get; }

	public override void Accept(Visitor.IRegexVisitor visitor, int indent) => visitor.VisitConditional(this, indent);
}
