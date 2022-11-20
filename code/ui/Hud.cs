using Sandbox;
using Sandbox.UI;

namespace Interfacer;

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

	public void GridCellClickedArena(IntVector gridPos)
	{
		InterfacerGame.CellClickedArenaCmd(gridPos.x, gridPos.y);
	}

	public void GridCellClickedInventory(IntVector gridPos)
	{
		InterfacerGame.CellClickedInventoryCmd(gridPos.x, gridPos.y);
	}
}
