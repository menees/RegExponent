namespace RegExponent
{
	#region Using Directives

	using System;
	using Menees.Windows.Forms;

	#endregion

	public partial class MainForm : Form
	{
		#region Constructors

		public MainForm()
		{
			this.InitializeComponent();
			this.pattern.SelectionIndent = 1;
			this.replacement.SelectionIndent = 1;
		}

		#endregion

		#region Private Methods

		private void About_Click(object sender, EventArgs e)
		{
			WindowsUtility.ShowAboutBox(this, this.GetType().Assembly);
		}

		private void HelpUrl_Click(object sender, EventArgs e)
		{
			if (sender is ToolStripMenuItem item && item.Tag is string text && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
			{
				WindowsUtility.ShellExecute(this, uri.ToString());
			}
		}

		private void Exit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Font_Click(object sender, EventArgs e)
		{
			this.fontDialog.Font = this.pattern.Font;
			if (this.fontDialog.ShowDialog(this) == DialogResult.OK)
			{
				Font font = this.fontDialog.Font;
				this.pattern.Font = font;
				this.replacement.Font = font;
				this.extendedRichTextBox3.Font = font;
			}
		}

		#endregion
	}
}