using Godot;
using System;

public partial class MainMenu : Control
{
	private Button playButton;
	private Button shopButton;
	private Button collectionButton;
	
	private const string CollectionScenePath = "res://Scenes/Collection.tscn";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playButton = GetNode<Button>("PlayButton");
		shopButton = GetNode<Button>("ShopButton");
		collectionButton = GetNode<Button>("CollectionButton");
		
		playButton.Pressed += PlayPressed;
		shopButton.Pressed += ShopPressed;
		collectionButton.Pressed += CollectionPressed;
	}	

	public async void PlayPressed() {
		GD.Print("Play button has been pressed!");
	}
	
	public async void CollectionPressed() {
		GD.Print("Collection button has been pressed!");
		var error = GetTree().ChangeSceneToFile(CollectionScenePath);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to load scene: {CollectionScenePath}");
		}
	}
	
	public async void ShopPressed() {
		GD.Print("Shop button has been pressed!");
	}
}
