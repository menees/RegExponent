namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Documents;

	#endregion

	internal class Highlighter
	{
		#region Constructors

		public Highlighter(string text, string newline)
		{
			this.Text = text;
			this.Newline = newline;
		}

		#endregion

		#region Public Properties

		public static Highlighter Empty { get; } = new(string.Empty, "\n");

		#endregion

		#region Protected Properties

		protected string Text { get; }

		protected string Newline { get; }

		#endregion

		#region Public Methods

		public virtual void Parse()
		{
		}

		/// <summary>
		/// Creates a new document from the parsed <see cref="Text"/> with thread affinity to the calling thread.
		/// </summary>
		public FlowDocument CreateDocument()
		{
			// TODO: Marshal FlowDocuments back to foreground thread. See https://stackoverflow.com/a/55614761/1882616. [Bill, 4/24/2022]
			string[] lines = this.Text.Split(this.Newline);
			FlowDocument document = new();
			foreach (string line in lines)
			{
				document.Blocks.Add(new Paragraph(new Run(line)));
			}

			return document;
		}

		#endregion
	}
}
