namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Documents;
	using System.Windows.Media;

	#endregion

	internal class Highlighter
	{
		#region Private Data Members

		private readonly List<Segment> segments = new();
		private readonly List<List<Part>> paragraphs = new();

		#endregion

		#region Constructors

		public Highlighter(string text, string newline)
		{
			this.Text = text;
			this.Newline = newline;
		}

		// This constructor should only be used by the Empty instance.
		private Highlighter()
		{
			this.Text = string.Empty;
			this.Newline = string.Empty;

			// Pre-parse everything since Highlight() can't do anything in the shared Empty instance.
			this.paragraphs.Add(new List<Part> { new Part(this.Text) });
		}

		#endregion

		#region Public Properties

		public static Highlighter Empty { get; } = new();

		#endregion

		#region Protected Properties

		protected string Text { get; }

		protected string Newline { get; }

		#endregion

		#region Public Methods

		public void Highlight()
		{
			// The Empty instance is a pre-parsed shared instance, so we mustn't mutate its state.
			if (this.paragraphs.Count == 0)
			{
				this.Parse();
				this.AddSegmentTo(this.Text.Length);
				this.SplitParagraphs();
			}
		}

		/// <summary>
		/// Creates a new document from the parsed <see cref="Text"/> with thread affinity to the calling thread.
		/// </summary>
		public FlowDocument CreateDocument()
		{
			// FlowDocuments have thread affinity, so they can only be created on the foreground thread,
			// unless we serialize them as discussed here: https://stackoverflow.com/a/55614761/1882616.
			FlowDocument document = new();
			foreach (List<Part> parts in this.paragraphs)
			{
				Paragraph paragraph = new();

				int decorationCount = 0;
				foreach (Part part in parts)
				{
					Run run = new(part.Text);
					if (part.Foreground != HighlightColor.None)
					{
						run.Foreground = GetBrush(part.Foreground);
					}

					if (part.Background != HighlightColor.None)
					{
						run.Background = GetBrush(part.Background);
					}

					if (part.Underline != HighlightColor.None)
					{
						TextDecorationLocation location = decorationCount % 2 == 0 ? TextDecorationLocation.Underline : TextDecorationLocation.Baseline;
						TextDecoration decoration = new() { Pen = new Pen(GetBrush(part.Underline), 1) };
						run.TextDecorations.Add(decoration);
						decorationCount++;
					}

					paragraph.Inlines.Add(run);
				}

				document.Blocks.Add(paragraph);
			}

			return document;
		}

		#endregion

		#region Protected Methods

		protected virtual void Parse()
		{
			this.AddSegment(0, this.Text.Length);
		}

		protected string[] GetLines() => this.Text.Split(this.Newline);

		protected void AddSegment(
			int index,
			int length,
			HighlightColor foreground = HighlightColor.None,
			HighlightColor background = HighlightColor.None,
			HighlightColor underline = HighlightColor.None)
		{
			if (length > 0)
			{
				int nextIndex = this.AddSegmentTo(index);
				if (index >= nextIndex)
				{
					this.segments.Add(new(index, length, foreground, background, underline));
				}
				else
				{
					// Note: This only supports adding an overlapping segment on the last segment!
					//
					// The new index is inside the previous segment, so we have to split that.
					// A group segment can be contained within an outer Match segment.
					// The outer Match segment can be replaced with 1, 2, or 3 segments.
					// 1: Group == Match
					// 2: Group is at the start or end of the match.
					// 3: Group is fully inside the match.
					Segment outer = this.segments[^1];
					this.segments.RemoveAt(this.segments.Count - 1);
					int seg1Index = outer.Index;
					int seg1Length = index - seg1Index;
					int seg2Index = index;
					int seg2Length = length;
					int seg3Index = index + length;
					int seg3Length = outer.Length - seg3Index;
					this.AddCombinedSegment(seg1Index, seg1Length, foreground, background, underline, outer);
					this.AddCombinedSegment(seg2Index, seg2Length, foreground, background, underline, outer);
					this.AddCombinedSegment(seg3Index, seg3Length, foreground, background, underline, outer);
				}
			}
		}

		#endregion

		#region Private Methods

		private static Brush? GetBrush(HighlightColor highlightColor)
			=> highlightColor switch
			{
				HighlightColor.None => null,
				HighlightColor.Blue => Brushes.Blue,
				HighlightColor.Green => Brushes.ForestGreen,
				HighlightColor.Purple => Brushes.DarkViolet,
				HighlightColor.Gray => Brushes.Gray,
				HighlightColor.Yellow => Brushes.Yellow,
				HighlightColor.Orange => Brushes.Orange,
				_ => Brushes.Black,
			};

		private int AddSegmentTo(int index)
		{
			int nextIndex = this.segments.Count == 0 ? 0 : this.segments[^1].NextIndex;
			if (index > nextIndex)
			{
				this.segments.Add(new(nextIndex, index - nextIndex));
			}

			return nextIndex;
		}

		private void AddCombinedSegment(
			int index,
			int length,
			HighlightColor foreground,
			HighlightColor background,
			HighlightColor underline,
			Segment outer)
		{
			if (length > 0)
			{
				static HighlightColor Max(HighlightColor color1, HighlightColor color2)
					=> color1 >= color2 ? color1 : color2;

				this.segments.Add(new(
					index,
					length,
					Max(foreground, outer.Foreground),
					Max(background, outer.Background),
					Max(underline, outer.Underline)));
			}
		}

		private void SplitParagraphs()
		{
			List<Part> parts = new();
			this.paragraphs.Add(parts);
			foreach (Segment segment in this.segments)
			{
				string text = this.Text.Substring(segment.Index, segment.Length);
				string[] lines = text.Split(this.Newline);
				int numLines = lines.Length;
				for (int i = 0; i < numLines; i++)
				{
					if (i > 0)
					{
						parts = new();
						this.paragraphs.Add(parts);
					}

					string line = lines[i];
					Part part = new(line, segment.Foreground, segment.Background, segment.Underline);
					parts.Add(part);
				}
			}

			this.segments.Clear();
		}

		#endregion

		#region Private Types

		private sealed class Segment
		{
			#region Constructors

			public Segment(
				int index,
				int length,
				HighlightColor foreground = HighlightColor.None,
				HighlightColor background = HighlightColor.None,
				HighlightColor underline = HighlightColor.None)
			{
				this.Index = index;
				this.Length = length;
				this.Foreground = foreground;
				this.Background = background;
				this.Underline = underline;
			}

			#endregion

			#region Public Properties

			public int Index { get; }

			public int Length { get; set; }

			public int NextIndex => this.Index + this.Length;

			public HighlightColor Foreground { get; }

			public HighlightColor Background { get; }

			public HighlightColor Underline { get; }

			#endregion
		}

		private sealed class Part
		{
			#region Constructors

			public Part(
				string text,
				HighlightColor foreground = HighlightColor.None,
				HighlightColor background = HighlightColor.None,
				HighlightColor underline = HighlightColor.None)
			{
				this.Text = text;
				this.Foreground = foreground;
				this.Background = background;
				this.Underline = underline;
			}

			#endregion

			#region Public Properties

			public string Text { get; }

			public HighlightColor Foreground { get; }

			public HighlightColor Background { get; }

			public HighlightColor Underline { get; }

			#endregion
		}

		#endregion
	}
}
