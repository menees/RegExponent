namespace RegExplainer;

using RegExplainer.Ast;
using RegExplainer.Parser;
using RegExplainer.Visitor;

/// <summary>
/// High-level façade for parsing and explaining regular expressions.
/// </summary>
public static class RegExplainerApi
{
	/// <summary>
	/// Parses a regex pattern and returns the AST root node.
	/// </summary>
	public static RegexNode Parse(string pattern)
	{
		RegexParser parser = new(pattern);
		return parser.Parse();
	}

	/// <summary>
	/// Attempts to parse a regex pattern. Returns <c>true</c> on success.
	/// </summary>
	public static bool TryParse(string pattern, out RegexNode? ast, out RegexParseException? error)
	{
		RegexParser parser = new(pattern);
		return parser.TryParse(out ast, out error);
	}

	/// <summary>
	/// Parses a regex pattern and returns a human-readable explanation.
	/// </summary>
	public static string Explain(string pattern)
	{
		RegexNode ast = Parse(pattern);
		ExplanationVisitor visitor = new();
		ast.Accept(visitor, 0);
		return visitor.Builder.ToString();
	}

	/// <summary>
	/// Parses a regex pattern and serializes the AST as JSON.
	/// </summary>
	public static string ToJson(string pattern)
	{
		RegexNode ast = Parse(pattern);
		return Json.JsonAstSerializer.Serialize(ast);
	}
}
