namespace RegExplainer.Visitor;

using RegExplainer.Ast;

public interface IRegexVisitor
{
	void VisitSequence(SequenceNode node, int indent);

	void VisitAlternation(AlternationNode node, int indent);

	void VisitLiteral(LiteralNode node, int indent);

	void VisitDot(DotNode node, int indent);

	void VisitAnchor(AnchorNode node, int indent);

	void VisitQuantifier(QuantifierNode node, int indent);

	void VisitCharacterClass(CharacterClassNode node, int indent);

	void VisitGroup(GroupNode node, int indent);

	void VisitEscape(EscapeNode node, int indent);

	void VisitLookaround(LookaroundNode node, int indent);

	void VisitBackreference(BackreferenceNode node, int indent);

	void VisitNamedBackreference(NamedBackreferenceNode node, int indent);

	void VisitComment(CommentNode node, int indent);

	void VisitConditional(ConditionalNode node, int indent);

	void VisitInlineOptions(InlineOptionsNode node, int indent);
}
