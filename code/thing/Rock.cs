using Sandbox;
using System;

namespace Interfacer;
public partial class Rock : Thing
{
	public Rock()
	{
		DisplayIcon = "🗿";
		IconPriority = 0f;
		ShouldLogBehaviour = true;
		Tooltip = "A rock.";
	}
}
