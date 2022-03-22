namespace RegExponent
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.formSaver = new Menees.Windows.Forms.FormSaver(this.components);
			this.recentFiles = new Menees.Windows.Forms.RecentItemList(this.components);
			this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainMenu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.autoSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.matchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.windowsNewlinesCRLFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.unixNewlineLFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.fontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ignoreCaseiiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.multilinemmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.singlelinessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.explicitCapturennToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ignorePatternWhitespacexxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.rightToLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.eCMAScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cultureInvariantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.insertInlineOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nETRegexQuickReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.regexOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.about = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip = new Menees.Windows.Forms.ExtendedToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.patternPanel = new System.Windows.Forms.Panel();
			this.extendedPanel1 = new Menees.Windows.Forms.ExtendedPanel();
			this.pattern = new Menees.Windows.Forms.ExtendedRichTextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.extendedPanel2 = new Menees.Windows.Forms.ExtendedPanel();
			this.replacement = new Menees.Windows.Forms.ExtendedRichTextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.panel2 = new System.Windows.Forms.Panel();
			this.extendedPanel3 = new Menees.Windows.Forms.ExtendedPanel();
			this.extendedRichTextBox3 = new Menees.Windows.Forms.ExtendedRichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.openDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveDialog = new System.Windows.Forms.SaveFileDialog();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.mainMenu.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.patternPanel.SuspendLayout();
			this.extendedPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.extendedPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.extendedPanel3.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// formSaver
			// 
			this.formSaver.ContainerControl = this;
			// 
			// recentFiles
			// 
			this.recentFiles.FormSaver = this.formSaver;
			this.recentFiles.Items = new string[0];
			this.recentFiles.MenuItem = this.recentToolStripMenuItem;
			this.recentFiles.SettingsNodeName = "Recent Files";
			// 
			// recentToolStripMenuItem
			// 
			this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
			this.recentToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.recentToolStripMenuItem.Text = "&Recent";
			// 
			// mainMenu
			// 
			this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.modeToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.mainMenu.Location = new System.Drawing.Point(0, 0);
			this.mainMenu.Name = "mainMenu";
			this.mainMenu.Size = new System.Drawing.Size(800, 24);
			this.mainMenu.TabIndex = 0;
			this.mainMenu.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripMenuItem3,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.autoSaveToolStripMenuItem,
            this.toolStripMenuItem1,
            this.recentToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.newToolStripMenuItem.Text = "&New";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.openToolStripMenuItem.Text = "&Open...";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(126, 6);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.saveAsToolStripMenuItem.Text = "Save &As...";
			// 
			// autoSaveToolStripMenuItem
			// 
			this.autoSaveToolStripMenuItem.Name = "autoSaveToolStripMenuItem";
			this.autoSaveToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.autoSaveToolStripMenuItem.Text = "A&uto-Save";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(126, 6);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(126, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.Exit_Click);
			// 
			// modeToolStripMenuItem
			// 
			this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.matchToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.splitToolStripMenuItem,
            this.toolStripMenuItem6,
            this.windowsNewlinesCRLFToolStripMenuItem,
            this.unixNewlineLFToolStripMenuItem,
            this.toolStripMenuItem7,
            this.fontToolStripMenuItem});
			this.modeToolStripMenuItem.Name = "modeToolStripMenuItem";
			this.modeToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			this.modeToolStripMenuItem.Text = "&Mode";
			// 
			// matchToolStripMenuItem
			// 
			this.matchToolStripMenuItem.Name = "matchToolStripMenuItem";
			this.matchToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.matchToolStripMenuItem.Text = "&Match";
			// 
			// replaceToolStripMenuItem
			// 
			this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
			this.replaceToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.replaceToolStripMenuItem.Text = "&Replace";
			// 
			// splitToolStripMenuItem
			// 
			this.splitToolStripMenuItem.Name = "splitToolStripMenuItem";
			this.splitToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.splitToolStripMenuItem.Text = "&Split";
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(193, 6);
			// 
			// windowsNewlinesCRLFToolStripMenuItem
			// 
			this.windowsNewlinesCRLFToolStripMenuItem.Name = "windowsNewlinesCRLFToolStripMenuItem";
			this.windowsNewlinesCRLFToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.windowsNewlinesCRLFToolStripMenuItem.Text = "Windows Newline: \\r\\n";
			// 
			// unixNewlineLFToolStripMenuItem
			// 
			this.unixNewlineLFToolStripMenuItem.Name = "unixNewlineLFToolStripMenuItem";
			this.unixNewlineLFToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.unixNewlineLFToolStripMenuItem.Text = "Unix Newline: \\n";
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(193, 6);
			// 
			// fontToolStripMenuItem
			// 
			this.fontToolStripMenuItem.Name = "fontToolStripMenuItem";
			this.fontToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			this.fontToolStripMenuItem.Text = "Font...";
			this.fontToolStripMenuItem.Click += new System.EventHandler(this.Font_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoreCaseiiToolStripMenuItem,
            this.multilinemmToolStripMenuItem,
            this.singlelinessToolStripMenuItem,
            this.explicitCapturennToolStripMenuItem,
            this.ignorePatternWhitespacexxToolStripMenuItem,
            this.rightToLeftToolStripMenuItem,
            this.eCMAScriptToolStripMenuItem,
            this.cultureInvariantToolStripMenuItem,
            this.toolStripMenuItem5,
            this.insertInlineOptionsToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// ignoreCaseiiToolStripMenuItem
			// 
			this.ignoreCaseiiToolStripMenuItem.Name = "ignoreCaseiiToolStripMenuItem";
			this.ignoreCaseiiToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.ignoreCaseiiToolStripMenuItem.Text = "Ignore Case (?i-i)";
			this.ignoreCaseiiToolStripMenuItem.ToolTipText = "Use case-insensitive matching.";
			// 
			// multilinemmToolStripMenuItem
			// 
			this.multilinemmToolStripMenuItem.Name = "multilinemmToolStripMenuItem";
			this.multilinemmToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.multilinemmToolStripMenuItem.Text = "Multiline (?m-m)";
			this.multilinemmToolStripMenuItem.ToolTipText = "Use multiline mode, where ^ and $ match the beginning and end of each line (inste" +
    "ad of the beginning and end of the input string).";
			// 
			// singlelinessToolStripMenuItem
			// 
			this.singlelinessToolStripMenuItem.Name = "singlelinessToolStripMenuItem";
			this.singlelinessToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.singlelinessToolStripMenuItem.Text = "Singleline (?s-s)";
			this.singlelinessToolStripMenuItem.ToolTipText = "Use single-line mode, where the period (.) matches every character (instead of ev" +
    "ery character except \\n).";
			// 
			// explicitCapturennToolStripMenuItem
			// 
			this.explicitCapturennToolStripMenuItem.Name = "explicitCapturennToolStripMenuItem";
			this.explicitCapturennToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.explicitCapturennToolStripMenuItem.Text = "Explicit Capture (?n-n)";
			this.explicitCapturennToolStripMenuItem.ToolTipText = "Do not capture unnamed groups. The only valid captures are explicitly named or nu" +
    "mbered groups of the form (?<name> subexpression).";
			// 
			// ignorePatternWhitespacexxToolStripMenuItem
			// 
			this.ignorePatternWhitespacexxToolStripMenuItem.Name = "ignorePatternWhitespacexxToolStripMenuItem";
			this.ignorePatternWhitespacexxToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.ignorePatternWhitespacexxToolStripMenuItem.Text = "Ignore Pattern Whitespace (?x-x)";
			this.ignorePatternWhitespacexxToolStripMenuItem.ToolTipText = "Exclude unescaped white space from the pattern, and enable comments after a numbe" +
    "r sign (#).";
			// 
			// rightToLeftToolStripMenuItem
			// 
			this.rightToLeftToolStripMenuItem.Name = "rightToLeftToolStripMenuItem";
			this.rightToLeftToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.rightToLeftToolStripMenuItem.Text = "Right To Left";
			this.rightToLeftToolStripMenuItem.ToolTipText = "Change the search direction. Search moves from right to left instead of from left" +
    " to right.";
			// 
			// eCMAScriptToolStripMenuItem
			// 
			this.eCMAScriptToolStripMenuItem.Name = "eCMAScriptToolStripMenuItem";
			this.eCMAScriptToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.eCMAScriptToolStripMenuItem.Text = "ECMAScript";
			this.eCMAScriptToolStripMenuItem.ToolTipText = "Enable ECMAScript-compliant behavior for the expression.";
			// 
			// cultureInvariantToolStripMenuItem
			// 
			this.cultureInvariantToolStripMenuItem.Name = "cultureInvariantToolStripMenuItem";
			this.cultureInvariantToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.cultureInvariantToolStripMenuItem.Text = "Culture Invariant";
			this.cultureInvariantToolStripMenuItem.ToolTipText = "Ignore cultural differences in language.";
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(243, 6);
			// 
			// insertInlineOptionsToolStripMenuItem
			// 
			this.insertInlineOptionsToolStripMenuItem.Name = "insertInlineOptionsToolStripMenuItem";
			this.insertInlineOptionsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.insertInlineOptionsToolStripMenuItem.Text = "Insert Inline Options";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nETRegexQuickReferenceToolStripMenuItem,
            this.regexOptionsToolStripMenuItem,
            this.toolStripMenuItem4,
            this.about});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// nETRegexQuickReferenceToolStripMenuItem
			// 
			this.nETRegexQuickReferenceToolStripMenuItem.Name = "nETRegexQuickReferenceToolStripMenuItem";
			this.nETRegexQuickReferenceToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.nETRegexQuickReferenceToolStripMenuItem.Tag = "https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-la" +
    "nguage-quick-reference";
			this.nETRegexQuickReferenceToolStripMenuItem.Text = ".NET Regex &Quick Reference...";
			this.nETRegexQuickReferenceToolStripMenuItem.Click += new System.EventHandler(this.HelpUrl_Click);
			// 
			// regexOptionsToolStripMenuItem
			// 
			this.regexOptionsToolStripMenuItem.Name = "regexOptionsToolStripMenuItem";
			this.regexOptionsToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.regexOptionsToolStripMenuItem.Tag = "https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-op" +
    "tions";
			this.regexOptionsToolStripMenuItem.Text = ".NET Regex &Options...";
			this.regexOptionsToolStripMenuItem.Click += new System.EventHandler(this.HelpUrl_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(228, 6);
			// 
			// about
			// 
			this.about.Name = "about";
			this.about.Size = new System.Drawing.Size(231, 22);
			this.about.Text = "&About...";
			this.about.Click += new System.EventHandler(this.About_Click);
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(800, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip1";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(61, 22);
			this.toolStripButton1.Text = "Match";
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(68, 22);
			this.toolStripButton2.Text = "Replace";
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(50, 22);
			this.toolStripButton3.Text = "Split";
			// 
			// patternPanel
			// 
			this.patternPanel.Controls.Add(this.extendedPanel1);
			this.patternPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.patternPanel.Location = new System.Drawing.Point(0, 49);
			this.patternPanel.Name = "patternPanel";
			this.patternPanel.Padding = new System.Windows.Forms.Padding(4);
			this.patternPanel.Size = new System.Drawing.Size(800, 31);
			this.patternPanel.TabIndex = 2;
			// 
			// extendedPanel1
			// 
			this.extendedPanel1.BackColor = System.Drawing.SystemColors.Window;
			this.extendedPanel1.Controls.Add(this.pattern);
			this.extendedPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extendedPanel1.Location = new System.Drawing.Point(4, 4);
			this.extendedPanel1.Name = "extendedPanel1";
			this.extendedPanel1.Padding = new System.Windows.Forms.Padding(3);
			this.extendedPanel1.Size = new System.Drawing.Size(792, 23);
			this.extendedPanel1.TabIndex = 1;
			// 
			// pattern
			// 
			this.pattern.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.pattern.DetectUrls = false;
			this.pattern.Dock = System.Windows.Forms.DockStyle.Top;
			this.pattern.HideSelection = false;
			this.pattern.Location = new System.Drawing.Point(3, 3);
			this.pattern.Multiline = false;
			this.pattern.Name = "pattern";
			this.pattern.RichTextShortcutsEnabled = false;
			this.pattern.Size = new System.Drawing.Size(786, 16);
			this.pattern.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.extendedPanel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 80);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(4);
			this.panel1.Size = new System.Drawing.Size(800, 31);
			this.panel1.TabIndex = 3;
			// 
			// extendedPanel2
			// 
			this.extendedPanel2.BackColor = System.Drawing.SystemColors.Window;
			this.extendedPanel2.Controls.Add(this.replacement);
			this.extendedPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extendedPanel2.Location = new System.Drawing.Point(4, 4);
			this.extendedPanel2.Name = "extendedPanel2";
			this.extendedPanel2.Padding = new System.Windows.Forms.Padding(3);
			this.extendedPanel2.Size = new System.Drawing.Size(792, 23);
			this.extendedPanel2.TabIndex = 1;
			// 
			// replacement
			// 
			this.replacement.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.replacement.DetectUrls = false;
			this.replacement.Dock = System.Windows.Forms.DockStyle.Top;
			this.replacement.HideSelection = false;
			this.replacement.Location = new System.Drawing.Point(3, 3);
			this.replacement.Multiline = false;
			this.replacement.Name = "replacement";
			this.replacement.RichTextShortcutsEnabled = false;
			this.replacement.Size = new System.Drawing.Size(786, 16);
			this.replacement.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 111);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.panel2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
			this.splitContainer1.Size = new System.Drawing.Size(800, 339);
			this.splitContainer1.SplitterDistance = 142;
			this.splitContainer1.TabIndex = 4;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.extendedPanel3);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(800, 142);
			this.panel2.TabIndex = 0;
			// 
			// extendedPanel3
			// 
			this.extendedPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.extendedPanel3.BackColor = System.Drawing.SystemColors.Window;
			this.extendedPanel3.Controls.Add(this.extendedRichTextBox3);
			this.extendedPanel3.Location = new System.Drawing.Point(4, 20);
			this.extendedPanel3.Name = "extendedPanel3";
			this.extendedPanel3.Padding = new System.Windows.Forms.Padding(3);
			this.extendedPanel3.Size = new System.Drawing.Size(792, 120);
			this.extendedPanel3.TabIndex = 2;
			// 
			// extendedRichTextBox3
			// 
			this.extendedRichTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.extendedRichTextBox3.DetectUrls = false;
			this.extendedRichTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extendedRichTextBox3.HideSelection = false;
			this.extendedRichTextBox3.Location = new System.Drawing.Point(3, 3);
			this.extendedRichTextBox3.Name = "extendedRichTextBox3";
			this.extendedRichTextBox3.RichTextShortcutsEnabled = false;
			this.extendedRichTextBox3.Size = new System.Drawing.Size(786, 114);
			this.extendedRichTextBox3.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Input:";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(800, 193);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.dataGridView1);
			this.tabPage1.Location = new System.Drawing.Point(4, 24);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(792, 165);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Matches";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(3, 3);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowTemplate.Height = 25;
			this.dataGridView1.Size = new System.Drawing.Size(786, 159);
			this.dataGridView1.TabIndex = 0;
			// 
			// tabPage2
			// 
			this.tabPage2.Location = new System.Drawing.Point(4, 24);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(792, 165);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Output";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 24);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(792, 165);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Splits";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// openDialog
			// 
			this.openDialog.DefaultExt = "rxp";
			this.openDialog.FileName = "openFileDialog1";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.patternPanel);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.mainMenu);
			this.MainMenuStrip = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "RegExponent";
			this.mainMenu.ResumeLayout(false);
			this.mainMenu.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.patternPanel.ResumeLayout(false);
			this.extendedPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.extendedPanel2.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.extendedPanel3.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Menees.Windows.Forms.FormSaver formSaver;
		private MenuStrip mainMenu;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem newToolStripMenuItem;
		private ToolStripMenuItem openToolStripMenuItem;
		private ToolStripMenuItem saveToolStripMenuItem;
		private ToolStripMenuItem saveAsToolStripMenuItem;
		private ToolStripSeparator toolStripMenuItem1;
		private ToolStripMenuItem recentToolStripMenuItem;
		private ToolStripSeparator toolStripMenuItem2;
		private ToolStripMenuItem exitToolStripMenuItem;
		private ToolStripMenuItem optionsToolStripMenuItem;
		private ToolStripMenuItem helpToolStripMenuItem;
		private Menees.Windows.Forms.RecentItemList recentFiles;
		private ToolStripSeparator toolStripMenuItem3;
		private ToolStripMenuItem autoSaveToolStripMenuItem;
		private ToolStripMenuItem nETRegexQuickReferenceToolStripMenuItem;
		private ToolStripMenuItem regexOptionsToolStripMenuItem;
		private ToolStripSeparator toolStripMenuItem4;
		private ToolStripMenuItem about;
		private Menees.Windows.Forms.ExtendedToolStrip toolStrip;
		private ToolStripMenuItem ignoreCaseiiToolStripMenuItem;
		private ToolStripMenuItem multilinemmToolStripMenuItem;
		private ToolStripMenuItem singlelinessToolStripMenuItem;
		private ToolStripMenuItem explicitCapturennToolStripMenuItem;
		private ToolStripMenuItem ignorePatternWhitespacexxToolStripMenuItem;
		private ToolStripMenuItem rightToLeftToolStripMenuItem;
		private ToolStripMenuItem eCMAScriptToolStripMenuItem;
		private ToolStripMenuItem cultureInvariantToolStripMenuItem;
		private ToolStripSeparator toolStripMenuItem5;
		private ToolStripMenuItem insertInlineOptionsToolStripMenuItem;
		private ToolStripButton toolStripButton1;
		private ToolStripButton toolStripButton2;
		private ToolStripButton toolStripButton3;
		private ToolStripMenuItem modeToolStripMenuItem;
		private ToolStripMenuItem matchToolStripMenuItem;
		private ToolStripMenuItem replaceToolStripMenuItem;
		private ToolStripMenuItem splitToolStripMenuItem;
		private Panel patternPanel;
		private Menees.Windows.Forms.ExtendedRichTextBox pattern;
		private SplitContainer splitContainer1;
		private Panel panel1;
		private Menees.Windows.Forms.ExtendedRichTextBox replacement;
		private Panel panel2;
		private Menees.Windows.Forms.ExtendedRichTextBox extendedRichTextBox3;
		private Label label1;
		private TabControl tabControl1;
		private TabPage tabPage1;
		private TabPage tabPage2;
		private TabPage tabPage3;
		private ToolStripSeparator toolStripMenuItem6;
		private ToolStripMenuItem windowsNewlinesCRLFToolStripMenuItem;
		private ToolStripMenuItem unixNewlineLFToolStripMenuItem;
		private ToolStripSeparator toolStripMenuItem7;
		private ToolStripMenuItem fontToolStripMenuItem;
		private DataGridView dataGridView1;
		private OpenFileDialog openDialog;
		private SaveFileDialog saveDialog;
		private FontDialog fontDialog;
		private Menees.Windows.Forms.ExtendedPanel extendedPanel1;
		private Menees.Windows.Forms.ExtendedPanel extendedPanel2;
		private Menees.Windows.Forms.ExtendedPanel extendedPanel3;
	}
}