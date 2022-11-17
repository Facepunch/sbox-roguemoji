using Sandbox;
using System;

namespace Interfacer;
public partial class Rock : Thing
{
	public Rock()
	{
		DisplayIcon = "🗿";
		ShouldLogBehaviour = true;
		Tooltip = "A rock.";
	}
}
