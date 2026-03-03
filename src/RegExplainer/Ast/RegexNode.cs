namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public abstract class RegexNode
{
	public int? Start { get; internal set; }

	public int? End { get; internal set; }

	public abstract void Accept(IRegexVisitor visitor, int indent);
}
