using Godot;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public partial class Login : VBoxContainer
{
	[Export] private string LoginUrl = "https://mythralea.com/api/auth/login";

	private LineEdit _usernameField;
	private LineEdit _passwordField;
	private Button _loginButton;
	private ProgressBar _progressBar;
	private Label _loadingText;
	private APIClient _apiClient;
	private const string LoadingPatcherScenePath = "res://Scenes/LoadingPatcher.tscn";

	public override void _Ready()
	{
		// Fetch UI nodes
		_usernameField = GetNode<LineEdit>("UsernameField");
		_passwordField = GetNode<LineEdit>("PasswordField");
		_loginButton = GetNode<Button>("LoginButton");
		_progressBar = GetNode<ProgressBar>("ProgressBar");
		_loadingText = GetNode<Label>("LoadingText");

		// Hide optional progress UI
		_progressBar.Visible = false;
		_loadingText.Visible = false;

		// Initialize API client
		_apiClient = new APIClient();
		AddChild(_apiClient);

		// Connect button signal
		_loginButton.Pressed += OnLoginButtonPressed;
	}

	private async void OnLoginButtonPressed()
	{
		string username = _usernameField.Text.Trim();
		string password = _passwordField.Text.Trim();

		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			GD.Print("Please fill in all fields.");
			return;
		}

		_loadingText.Text = "Logging in...";
		_loadingText.Visible = true;

		var request = await SendLoginRequest(username, password);
		if (request != null && request.StatusCode == System.Net.HttpStatusCode.OK)
		{
			var responseBody = await request.Content.ReadAsStringAsync();
			var loginResponse = JsonDocument.Parse(responseBody);
			string token = loginResponse.RootElement.GetProperty("token").GetString();

			SaveToken(token);

			GD.Print("Login successful!");
			_loadingText.Text = "Login successful!";

			// Fetch and print player data
			var playerData = await _apiClient.GetPlayerDataAsync();
			if (playerData != null)
			{
				GD.Print("Player data: ", playerData.RootElement.ToString());
				
				// Change to the new scene
		var error = GetTree().ChangeSceneToFile(LoadingPatcherScenePath);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to load scene: {LoadingPatcherScenePath}");
		}
			}
			else
			{
				GD.PrintErr("Failed to fetch player data.");
			}

			_loadingText.Visible = false;
		}
		else
		{
			GD.PrintErr("Login failed: ", request?.StatusCode);
			_loadingText.Text = "Login failed!";
		}
	}

	private async Task<System.Net.Http.HttpResponseMessage> SendLoginRequest(string username, string password)
{
	using (var client = new System.Net.Http.HttpClient())
	{
		var payload = new
		{
			username,
			password
		};

		string jsonData = JsonSerializer.Serialize(payload);
		var content = new System.Net.Http.StringContent(jsonData, Encoding.UTF8, "application/json");

		try
		{
			return await client.PostAsync(LoginUrl, content);
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error during login request: ", ex.Message);
			return null;
		}
	}
}


	private void SaveToken(string token)
	{
		var filePath = "user://auth_token.txt";
		var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
		file.StoreString(token);
		file.Close();

		GD.Print("Token saved to: ", filePath);
	}
}
