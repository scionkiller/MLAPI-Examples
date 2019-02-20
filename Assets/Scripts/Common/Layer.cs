public class Layer
{
	// MUST BE KEPT IN SYNC WITH LAYERS IN THE EDITOR
	enum LayerId
	{
		  Tool = 8
	}

	public static readonly int TOOL_MASK = 1 << (int)LayerId.Tool;
}