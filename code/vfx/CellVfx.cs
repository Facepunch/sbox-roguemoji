using Sandbox;
using System;

namespace Interfacer;

public abstract class CellVfx
{
    public GridCell GridCell { get; private set; }
    public IntVector GridPos { get; private set; }
    public GridPanel GridPanel { get; private set; }

    public TimeSince TimeSinceStart { get; protected set; }

    public virtual void Init(GridCell gridCell, IntVector gridPos, GridPanel gridPanel)
    {
        GridCell = gridCell;
        GridPos = gridPos;
        GridPanel = gridPanel;
        TimeSinceStart = 0f;
    }

    public virtual void Update(float dt)
    {

    }

    public void Remove()
    {
        GridPanel.RemoveCellVfx(GridPos, TypeLibrary.GetDescription(GetType()));
    }

    public virtual void OnRemove()
    {

    }
}