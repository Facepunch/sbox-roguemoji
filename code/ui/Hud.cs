using Sandbox;
using Sandbox.UI;

namespace Interfacer;

public partial class Hud : RootPanel
{
	public static Hud Instance { get; private set; }

	public int IndexClicked { get; private set; }

	public GridCell SelectedCell { get; private set; }

	public MainPanel MainPanel { get; private set; }

	public Hud(int width, int height)
	{
		Instance = this;

		StyleSheet.Load("/ui/Hud.scss");

		MainPanel = AddChild<MainPanel>();
	}

	public void GridCellClicked(int x, int y)
	{
		InterfacerGame.CellClicked( x, y );
	}
}
