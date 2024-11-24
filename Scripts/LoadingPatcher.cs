using Godot;
using System;
using System.IO; // For directory and file operations
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic; // For Dictionary
using System.Threading.Tasks;

public partial class LoadingPatcher : Control
{
	[Export] private string CharacterDataUrl = "https://mythralea.com/api/CharacterData";
	[Export] private string ImageBaseUrl = "https://images.mythralea.com/";
	[Export] private string LocalImagePath = "user://Images/CharacterImages/";
	[Export] private string MetadataFilePath = "user://Images/metadata.json";
	
	private const string MainMenuScenePath = "res://Scenes/MainMenu.tscn";

	private ProgressBar _loadingBar;
	private Label _statusLabel;
	private Dictionary<string, string> _imageMetadata = new();

	public override void _Ready()
	{
		_loadingBar = GetNode<ProgressBar>("LoadingBar");
		_statusLabel = GetNode<Label>("StatusLabel");

		// Load existing metadata
		LoadMetadata();

		// Start the patching process
		StartPatching();
	}

	private void LoadMetadata()
	{
		// Load the metadata JSON if it exists
		if (File.Exists(ProjectSettings.GlobalizePath(MetadataFilePath)))
		{
			string metadataContent = File.ReadAllText(ProjectSettings.GlobalizePath(MetadataFilePath));
			_imageMetadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataContent);
		}
		else
		{
			_imageMetadata = new Dictionary<string, string>();
		}
	}

	private void SaveMetadata()
	{
		// Save the metadata back to the file
		string metadataContent = JsonSerializer.Serialize(_imageMetadata);
		File.WriteAllText(ProjectSettings.GlobalizePath(MetadataFilePath), metadataContent);
	}

	private async void StartPatching()
	{
		_statusLabel.Text = "Fetching character data...";
		var characterData = await FetchCharacterData();

		if (characterData == null)
		{
			_statusLabel.Text = "Failed to fetch character data.";
			GD.PrintErr("Character data fetch failed.");
			return;
		}

		_statusLabel.Text = "Downloading images...";
		await DownloadImages(characterData);

		// Save the updated metadata
		SaveMetadata();

		_statusLabel.Text = "Patching complete!";
		_loadingBar.Value = 100;

		GD.Print("Patching complete!");
		
		var error = GetTree().ChangeSceneToFile(MainMenuScenePath);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to load scene: {MainMenuScenePath}");
		}
	}

	private async Task<JsonDocument> FetchCharacterData()
	{
		try
		{
			using (var client = new System.Net.Http.HttpClient())
			{
				var response = await client.GetAsync(CharacterDataUrl);
				response.EnsureSuccessStatusCode();

				string responseBody = await response.Content.ReadAsStringAsync();
				GD.Print("Character data fetched successfully.");
				return JsonDocument.Parse(responseBody);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error fetching character data: ", ex.Message);
			return null;
		}
	}

	private async Task DownloadImages(JsonDocument characterData)
	{
		var characters = characterData.RootElement.EnumerateArray();
		int totalCharacters = 0;
		foreach (var _ in characters) totalCharacters++; // Count characters
		int processedCharacters = 0;

		foreach (var character in characterData.RootElement.EnumerateArray())
		{
			string faction = character.GetProperty("faction").GetString().Replace(" ", "");
			string role = character.GetProperty("role").GetString().Replace(" ", "");
			string name = character.GetProperty("character_Name").GetString().Replace(" ", "");
			string imageUrl = $"{ImageBaseUrl}{faction}/{role}/{name}.png";
			string localPath = $"{LocalImagePath}{faction}/{role}/{name}.png";

			// Check and download the image only if it's updated
			await DownloadAndSaveImageIfUpdated(imageUrl, localPath, faction, role, name);

			// Update progress bar and status
			processedCharacters++;
			_loadingBar.Value = (processedCharacters / (float)totalCharacters) * 100;
			_statusLabel.Text = $"Downloading images... ({processedCharacters}/{totalCharacters})";
		}
	}

	private async Task DownloadAndSaveImageIfUpdated(string url, string localPath, string faction, string role, string name)
	{
		try
		{
			using (var client = new System.Net.Http.HttpClient())
			{
				// Set "If-Modified-Since" header if metadata exists
				string metadataKey = $"{faction}/{role}/{name}";
				if (_imageMetadata.TryGetValue(metadataKey, out var lastModified))
				{
					client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Parse(lastModified);
				}

				// Fetch the image
				var response = await client.GetAsync(url);

				if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
				{
					GD.Print($"Image not modified: {url}");
					return;
				}

				response.EnsureSuccessStatusCode();

				// Save the new image data
				byte[] imageData = await response.Content.ReadAsByteArrayAsync();
				string absolutePath = ProjectSettings.GlobalizePath(localPath);
				string directoryPath = Path.GetDirectoryName(absolutePath);
				EnsureDirectoryExists(directoryPath);
				File.WriteAllBytes(absolutePath, imageData);

				// Update metadata
				if (response.Content.Headers.LastModified.HasValue)
				{
					_imageMetadata[metadataKey] = response.Content.Headers.LastModified.Value.ToString("R");
				}

				GD.Print($"Image updated: {url}");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error downloading image {url}: {ex.Message}");
		}
	}

	private void EnsureDirectoryExists(string directoryPath)
	{
		if (!Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
			GD.Print($"Created directory: {directoryPath}");
		}
	}
}
