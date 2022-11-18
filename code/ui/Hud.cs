using Sandbox;
using Sandbox.UI;

namespace Interfacer;

public enum GridPanelType { None, Arena, Inventory };

public partial class Hud : RootPanel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }

	public Hud()
	{
		Instance = this;

		StyleSheet.Load("/ui/Hud.scss");

		MainPanel = AddChild<MainPanel>();
	}

	public void GridCellClicked(GridPanelType gridPanelType, IntVector gridPos)
	{
		InterfacerGame.CellClickedCmd( gridPanelType, gridPos.x, gridPos.y);
	}

	public GridPanel GetGridPanel(GridPanelType gridPanelType)
    {
		if (gridPanelType == GridPanelType.Arena)
			return MainPanel.ArenaPanel;
		else if(gridPanelType == GridPanelType.Inventory)
			return MainPanel.InventoryPanel;

		return null;
    }
}
