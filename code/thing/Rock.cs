using Sandbox;
using System;

namespace Interfacer;
public partial class Rock : Thing
{
	public override string Tooltip => "A rock.";

	public Rock()
	{
		DisplayIcon = "🗿";
		IconPriority = 0f;
		ShouldLogBehaviour = true;
	}
}
