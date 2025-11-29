namespace RegExponent.Highlights;

#region Using Directives

using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

#endregion

internal class BracketHighlighter : IBackgroundRenderer
{
	#region Private Data Members

	private static readonly Color BracketHighlightForeground = Colors.Silver;
	private static readonly Color BracketHighlightBackground = Colors.LightYellow;

	private readonly TextView textView;
	private readonly Pen? foregroundPen;
	private readonly Brush? backgroundBrush;

	private BracketMatch? match;

	#endregion

	#region Constructors

	public BracketHighlighter(TextView textView)
	{
		ArgumentNullException.ThrowIfNull(textView);
		this.textView = textView;
		this.textView.BackgroundRenderers.Add(this);

		this.foregroundPen = new Pen(new SolidColorBrush(BracketHighlightForeground), 1);
		this.foregroundPen.Freeze();

		this.backgroundBrush = new SolidColorBrush(BracketHighlightBackground);
		this.backgroundBrush.Freeze();
	}

	#endregion

	#region Public Properties

	public KnownLayer Layer => KnownLayer.Selection;

	#endregion

	#region Public Methods

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		if (this.match != null && this.foregroundPen != null && this.backgroundBrush != null)
		{
			BackgroundGeometryBuilder builder = new()
			{
				CornerRadius = 1,
				AlignToWholePixels = false,
			};

			// Add a box around the opening bracket
			builder.AddSegment(textView, new TextSegment
			{
				StartOffset = this.match.OpeningOffset,
				Length = this.match.OpeningLength,
			});
			builder.CloseFigure();

			// Add a box around the closing bracket
			builder.AddSegment(textView, new TextSegment
			{
				StartOffset = this.match.ClosingOffset,
				Length = this.match.ClosingLength,
			});

			Geometry geometry = builder.CreateGeometry();
			if (geometry != null)
			{
				drawingContext.DrawGeometry(this.backgroundBrush, this.foregroundPen, geometry);
			}
		}
	}

	public void SetMatch(BracketMatch? match)
	{
		if (this.match != match)
		{
			this.match = match;
			this.textView.InvalidateLayer(this.Layer);
		}
	}

	#endregion
}
