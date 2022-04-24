namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Text.Json;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows;
	using Menees;
	using Menees.Windows.Presentation;

	#endregion

	internal sealed class Model : PropertyChangeNotifier
	{
		#region Private Data Members

		private const short CurrentVersion = 1;

		private static readonly HashSet<string> UpdateSkipIsModified = new()
		{
			nameof(IsModified),
			nameof(InMatchMode),
			nameof(InReplaceMode),
			nameof(InSplitMode),
		};

		private readonly bool inDesignMode;
		private bool isModified;
		private string pattern = string.Empty;
		private string replacement = string.Empty;
		private string input = string.Empty;
		private RegexOptions regexOptions;
		private Mode mode;
		private bool unixNewline;

		#endregion

		#region Constructors

		public Model()
		{
			this.inDesignMode = WindowsUtility.IsInDesignMode(new DependencyObject());
		}

		#endregion

		#region Public Properties

		public bool IsModified { get => this.isModified; set => this.Update(ref this.isModified, value); }

		public string Pattern { get => this.pattern; set => this.Update(ref this.pattern, value); }

		public string Replacement { get => this.replacement; set => this.Update(ref this.replacement, value); }

		public string Input { get => this.input; set => this.Update(ref this.input, value); }

		public bool UseIgnoreCase { get => this.GetOption(RegexOptions.IgnoreCase); set => this.SetOption(RegexOptions.IgnoreCase, value); }

		public bool UseMultiline { get => this.GetOption(RegexOptions.Multiline); set => this.SetOption(RegexOptions.Multiline, value); }

		public bool UseSingleline { get => this.GetOption(RegexOptions.Singleline); set => this.SetOption(RegexOptions.Singleline, value); }

		public bool UseExplicitCapture { get => this.GetOption(RegexOptions.ExplicitCapture); set => this.SetOption(RegexOptions.ExplicitCapture, value); }

		public bool UseIgnorePatternWhitespace
		{
			get => this.GetOption(RegexOptions.IgnorePatternWhitespace);
			set => this.SetOption(RegexOptions.IgnorePatternWhitespace, value);
		}

		public bool UseRightToLeft { get => this.GetOption(RegexOptions.RightToLeft); set => this.SetOption(RegexOptions.RightToLeft, value); }

		public bool UseECMAScript { get => this.GetOption(RegexOptions.ECMAScript); set => this.SetOption(RegexOptions.ECMAScript, value); }

		public bool UseCultureInvariant { get => this.GetOption(RegexOptions.CultureInvariant); set => this.SetOption(RegexOptions.CultureInvariant, value); }

		public bool InMatchMode
		{
			get => this.mode == Mode.Match && !this.inDesignMode;
			set
			{
				if (value && this.Update(ref this.mode, Mode.Match))
				{
					this.OnPropertyChanged(nameof(this.InReplaceMode));
					this.OnPropertyChanged(nameof(this.InSplitMode));
				}
			}
		}

		public bool InReplaceMode
		{
			get => this.mode == Mode.Replace || this.inDesignMode;
			set
			{
				if (value && this.Update(ref this.mode, Mode.Replace))
				{
					this.OnPropertyChanged(nameof(this.InMatchMode));
					this.OnPropertyChanged(nameof(this.InSplitMode));
				}
			}
		}

		public bool InSplitMode
		{
			get => this.mode == Mode.Split || this.inDesignMode;
			set
			{
				if (value && this.Update(ref this.mode, Mode.Split))
				{
					this.OnPropertyChanged(nameof(this.InMatchMode));
					this.OnPropertyChanged(nameof(this.InReplaceMode));
				}
			}
		}

		public bool UnixNewline { get => this.unixNewline; set => this.Update(ref this.unixNewline, value); }

		#endregion

		#region Internal Properties

		/* Note: These internal properties don't send IPropertyChanged notifications. */

		internal Mode Mode => this.mode;

		internal string Newline => this.UnixNewline ? "\n" : "\r\n";

		internal RegexOptions Options => this.regexOptions;

		#endregion

		#region Public Methods

		public void Clear()
		{
			// Set the public properties rather than the backing members to ensure change notifications are sent.
			this.Input = string.Empty;
			this.Replacement = string.Empty;
			this.Pattern = string.Empty;

			this.UseIgnoreCase = default;
			this.UseMultiline = default;
			this.UseSingleline = default;
			this.UseExplicitCapture = default;
			this.UseIgnorePatternWhitespace = default;
			this.UseRightToLeft = default;
			this.UseECMAScript = default;
			this.UseCultureInvariant = default;

			this.InMatchMode = true;
			this.InReplaceMode = default;
			this.InSplitMode = default;

			this.UnixNewline = default;

			this.IsModified = false;
		}

		public void Load(string fileName)
		{
			// Reset everything first to minimize UI change processing (or at least make it deterministic).
			this.Clear();

			// Be permissive when reading in a file in case a human has manually edited it.
			JsonDocumentOptions options = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
			using Stream file = File.OpenRead(fileName);
			using JsonDocument document = JsonDocument.Parse(file, options);
			JsonElement root = document.RootElement;

			if (!root.TryGetProperty(nameof(Version), out JsonElement version) || version.GetInt16() != CurrentVersion)
			{
				throw Exceptions.NewArgumentException($"Unsupported file version: {version.GetInt16()}");
			}

			// Set the public properties rather than the backing members to ensure change notifications are sent.
			if (root.TryGetProperty(nameof(this.UnixNewline), out JsonElement newline))
			{
				this.UnixNewline = newline.GetBoolean();
			}

			if (root.TryGetProperty(nameof(this.Mode), out JsonElement modeElement)
				&& Enum.TryParse(modeElement.GetString(), out Mode mode))
			{
				this.InMatchMode = mode == Mode.Match;
				this.InReplaceMode = mode == Mode.Replace;
				this.InSplitMode = mode == Mode.Split;
			}

			if (root.TryGetProperty(nameof(RegexOptions), out JsonElement optionsElement)
				&& Enum.TryParse(optionsElement.GetString(), out RegexOptions regexOptions))
			{
				this.UseIgnoreCase = regexOptions.HasFlag(RegexOptions.IgnoreCase);
				this.UseMultiline = regexOptions.HasFlag(RegexOptions.Multiline);
				this.UseSingleline = regexOptions.HasFlag(RegexOptions.Singleline);
				this.UseExplicitCapture = regexOptions.HasFlag(RegexOptions.ExplicitCapture);
				this.UseIgnorePatternWhitespace = regexOptions.HasFlag(RegexOptions.IgnorePatternWhitespace);
				this.UseRightToLeft = regexOptions.HasFlag(RegexOptions.RightToLeft);
				this.UseECMAScript = regexOptions.HasFlag(RegexOptions.ECMAScript);
				this.UseCultureInvariant = regexOptions.HasFlag(RegexOptions.CultureInvariant);
			}

			// Load the text properties last so all the desired settings are configured first.
			// This should minimize churn in the UI as change notifications are processed.
			if (root.TryGetProperty(nameof(this.Pattern), out JsonElement pattern))
			{
				this.Pattern = pattern.GetString() ?? string.Empty;
			}

			if (root.TryGetProperty(nameof(this.Replacement), out JsonElement replacement))
			{
				this.Replacement = replacement.GetString() ?? string.Empty;
			}

			if (root.TryGetProperty(nameof(this.Input), out JsonElement input))
			{
				this.Input = input.GetString() ?? string.Empty;
			}

			this.IsModified = false;
		}

		public void Save(string fileName, bool isTempFile = false)
		{
			using MemoryStream memory = new();

			JsonWriterOptions options = new() { Indented = true };
			using Utf8JsonWriter writer = new(memory, options);
			writer.WriteStartObject();
			writer.WriteNumber(nameof(Version), 1);

			// Track writeCount so we don't write out temp files with just a Version number.
			int writeCount = 0;
			if (this.UnixNewline)
			{
				writeCount++;
				writer.WriteBoolean(nameof(this.UnixNewline), this.UnixNewline);
			}

			if (this.mode != default)
			{
				writeCount++;
				writer.WriteString(nameof(this.Mode), this.mode.ToString());
			}

			if (this.regexOptions != default)
			{
				writeCount++;
				writer.WriteString(nameof(RegexOptions), this.regexOptions.ToString());
			}

			if (this.Pattern.IsNotEmpty())
			{
				writeCount++;
				writer.WriteString(nameof(this.Pattern), this.Pattern);
			}

			if (this.Replacement.IsNotEmpty())
			{
				writeCount++;
				writer.WriteString(nameof(this.Replacement), this.Replacement);
			}

			if (this.Input.IsNotEmpty())
			{
				writeCount++;
				writer.WriteString(nameof(this.Input), this.Input);
			}

			if (!isTempFile || writeCount > 0)
			{
				writer.WriteEndObject();
				writer.Flush();
				File.WriteAllBytes(fileName, memory.ToArray());
			}

			this.IsModified = false;
		}

		#endregion

		#region Protected Methods

		protected override bool Update<T>(ref T member, T value, [CallerMemberName] string? callerMemberName = null)
		{
			bool result = base.Update(ref member, value, callerMemberName);
			if (result && callerMemberName != null && !UpdateSkipIsModified.Contains(callerMemberName))
			{
				this.IsModified = true;
			}

			return result;
		}

		#endregion

		#region Private Methods

		private bool GetOption(RegexOptions flag) => this.regexOptions.HasFlag(flag);

		private void SetOption(RegexOptions flag, bool include, [CallerMemberName] string? callerMemberName = null)
		{
			RegexOptions newOptions = include ? this.regexOptions | flag : this.regexOptions & ~flag;
			this.Update(ref this.regexOptions, newOptions, callerMemberName);
		}

		#endregion
	}
}
