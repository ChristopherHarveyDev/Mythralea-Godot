using Godot;

public partial class visLogin : Control
{
	private const float MaxButtonWidth = 300.0f;
	
	public override void _Ready()
	{
		AdjustUI();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			AdjustUI();
		}
	}

	private void AdjustUI()
	{
		// Get the screen size
		Vector2 screenSize = GetViewportRect().Size;

		// Adjust the label font size
		Label titleLabel = GetNode<Label>("VBoxContainer/Title");
		int fontSize = (int)(screenSize.Y * 0.05f); // 5% of screen height
		titleLabel.AddThemeFontSizeOverride("font_size", fontSize);

		// Adjust input widths and heights
		float inputWidth = screenSize.X * 0.4f; // 40% of screen width
		float inputHeight = screenSize.Y * 0.05f; // 5% of screen height
		int inputFontSize = (int)(screenSize.Y * 0.03f); // 3% of screen height for input font size

		// Adjust username input
		LineEdit usernameField = GetNode<LineEdit>("VBoxContainer/UsernameField");
		usernameField.CustomMinimumSize = new Vector2(inputWidth, inputHeight);
		usernameField.AddThemeFontSizeOverride("font_size", inputFontSize); // Adjust font size

		// Adjust password input
		LineEdit passwordField = GetNode<LineEdit>("VBoxContainer/PasswordField");
		passwordField.CustomMinimumSize = new Vector2(inputWidth, inputHeight);
		passwordField.AddThemeFontSizeOverride("font_size", inputFontSize); // Adjust font size

		// Adjust button size
		Button loginButton = GetNode<Button>("VBoxContainer/LoginButton");
		float buttonWidth = Mathf.Min(inputWidth, MaxButtonWidth); // Take the smaller of inputWidth or MaxButtonWidth
		loginButton.CustomMinimumSize = new Vector2(buttonWidth, inputHeight * 1.1f); // Slightly taller button
		
		loginButton.AddThemeFontSizeOverride("font_size", inputFontSize); // Optionally adjust button font size
	}
}
