using Godot;
using System.Text.Json;

public partial class GlobalState : Node
{
	public static JsonElement PlayerData;

	public override void _Ready()
	{
		GD.Print("GlobalState initialized.");
	}
}
