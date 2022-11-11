using Sandbox;
using Sandbox.UI;

namespace Interfacer;

public partial class Hud : RootPanel
{
	public int GridWidth { get; private set; }
	public int GridHeight { get; private set; }

	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }

	public Hud(int width, int height)
	{
		Instance = this;

		GridWidth = width;
		GridHeight = height;

		MainPanel = AddChild<MainPanel>();
	}

	public void GridCellClicked(GridCell gridCell)
	{
		//if ( gridCell == SelectedCell )
		//	return;

		IndexClicked = gridCell.Index;

		SelectedCell?.SetSelected(false);

		SelectedCell = gridCell;
		SelectedCell.SetSelected(true);

		var coords = gridCell.GetCoords();
		InterfacerGame.CellClicked( coords.x, coords.y );
	}
}
