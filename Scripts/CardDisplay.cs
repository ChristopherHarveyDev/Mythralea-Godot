using Godot;

public partial class CardDisplay : Button
{
	private TextureRect _frontSide;
	private TextureRect _backSide;

	public override void _Ready()
	{
		_frontSide = GetNode<TextureRect>("CharacterImage");
		_backSide = GetNode<TextureRect>("CardBack");
	}

	public void InitializeCard(Texture2D frontTexture, Texture2D backTexture, bool isOwned, Vector2 cardSize)
	{
		// Set card size
		CustomMinimumSize = cardSize;

		// Assign textures
		_frontSide.Texture = frontTexture;
		_backSide.Texture = backTexture;

		// Set visibility based on ownership
		_frontSide.Visible = isOwned;
		_backSide.Visible = !isOwned;
	}
}
