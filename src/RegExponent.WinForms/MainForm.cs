namespace RegExponent
{
	using Menees.Windows.Forms;
	#region Using Directives

	using System;

	#endregion

	public partial class MainForm : Form
	{
		#region Constructors

		public MainForm()
		{
			InitializeComponent();
			this.pattern.SelectionIndent = 1;
			this.replacement.SelectionIndent = 1;
		}

		#endregion

		#region Protected Methods

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			// TODO: Add paint method from https://stackoverflow.com/a/36534068/1882616. [Bill, 3/20/2022]
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

		#endregion
	}
}