using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;

// Use fully qualified System.Net.Http.HttpClient to resolve ambiguity
public partial class APIClient : Node
{
	private static readonly System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();

	[Export] private string BaseApiUrl = "https://mythralea.com/api/PlayerData";

	public override void _Ready()
	{
		// Set default request headers if needed (e.g., Authorization)
		if (HasToken())
		{
			string token = LoadToken();
			HttpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}
	}

	private string LoadToken()
	{
		var filePath = "user://auth_token.txt";
		if (!FileAccess.FileExists(filePath))
		{
			GD.PrintErr("No token found. User not logged in.");
			return null;
		}

		using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
		{
			return file.GetAsText().Trim();
		}
	}

	private bool HasToken() => !string.IsNullOrEmpty(LoadToken());

	public async Task<JsonDocument> GetPlayerDataAsync()
	{
		if (!HasToken())
		{
			GD.PrintErr("Unauthorized access: Token not found.");
			return null;
		}

		try
		{
			var response = await HttpClient.GetAsync(BaseApiUrl);
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();

			GD.Print("Player data fetched successfully.");
			return JsonDocument.Parse(responseBody);
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error fetching player data: ", ex.Message);
			return null;
		}
	}
}
