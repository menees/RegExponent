namespace RegExplainer.Visitor;

using RegExplainer.Ast;

public interface IRegexVisitor
{
	void VisitSequence(Ast.SequenceNode node, int indent);

	void VisitAlternation(AlternationNode node, int indent);

	void VisitLiteral(Ast.LiteralNode node, int indent);

	void VisitDot(Ast.DotNode node, int indent);

	void VisitAnchor(Ast.AnchorNode node, int indent);

	void VisitQuantifier(QuantifierNode node, int indent);

	void VisitCharacterClass(Ast.CharacterClassNode node, int indent);

	void VisitGroup(Ast.GroupNode node, int indent);

	void VisitEscape(Ast.EscapeNode node, int indent);

	void VisitLookaround(Ast.LookaroundNode node, int indent);

	void VisitBackreference(Ast.BackreferenceNode node, int indent);

	void VisitNamedBackreference(Ast.NamedBackreferenceNode node, int indent);

	void VisitComment(CommentNode node, int indent);

	void VisitConditional(ConditionalNode node, int indent);

	void VisitInlineOptions(InlineOptionsNode node, int indent);
}
