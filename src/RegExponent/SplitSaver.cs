namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Menees;

	#endregion

	// TODO: Move this into WindowSaver and use it from WirePeep. [Bill, 5/16/2022]
	internal sealed class SplitSaver
	{
		#region Private Data Members

		private readonly GridSplitter splitter;
		private readonly string nodeName;

		#endregion

		#region Constructors

		public SplitSaver(GridSplitter splitter, string? nodeName = null)
		{
			this.splitter = splitter;
			this.nodeName = nodeName ?? nameof(GridSplitter);
		}

		#endregion

		#region Public Methods

		public void Load(ISettingsNode baseNode)
		{
			ISettingsNode? splitterNode = baseNode.TryGetSubNode(this.nodeName);
			if (splitterNode != null)
			{
				RowDefinition[] splitterTargetRows = this.GetTargetRows();
				for (int i = 0; i < splitterTargetRows.Length; i++)
				{
					ISettingsNode? rowNode = splitterNode.TryGetSubNode($"Row{i}");
					if (rowNode != null)
					{
						double value = rowNode.GetValue(nameof(GridLength.Value), 1.0);
						GridUnitType unitType = rowNode.GetValue(nameof(GridLength.GridUnitType), GridUnitType.Star);
						RowDefinition row = splitterTargetRows[i];
						row.Height = new GridLength(value, unitType);
					}
				}
			}
		}

		public void Save(ISettingsNode baseNode)
		{
			baseNode.DeleteSubNode(this.nodeName);
			RowDefinition[] splitterTargetRows = this.GetTargetRows();
			if (splitterTargetRows.Length > 0)
			{
				ISettingsNode splitterNode = baseNode.GetSubNode(this.nodeName);
				for (int i = 0; i < splitterTargetRows.Length; i++)
				{
					RowDefinition row = splitterTargetRows[i];
					GridLength rowHeight = row.Height;
					ISettingsNode rowNode = splitterNode.GetSubNode($"Row{i}");
					rowNode.SetValue(nameof(rowHeight.Value), rowHeight.Value);
					rowNode.SetValue(nameof(rowHeight.GridUnitType), rowHeight.GridUnitType);
				}
			}
		}

		#endregion

		#region Private Methods

		private RowDefinition[] GetTargetRows()
		{
			RowDefinition[] result = Array.Empty<RowDefinition>();

			if (this.splitter.Parent is Grid grid)
			{
				int splitterRow = Grid.GetRow(this.splitter);
				result = new[]
				{
					grid.RowDefinitions[splitterRow - 1],
					grid.RowDefinitions[splitterRow + 1],
				};
			}

			return result;
		}

		#endregion
	}
}
