namespace RegExplainer.Ast;

public sealed class CategoryItem : CharacterClassItem
{
	public CategoryItem(string cat) => this.Category = cat;

	public string Category { get; }
}
