using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public partial class Collection : Control
{
	[Export] private PackedScene _cardScene; // Drag and drop Card.tscn in the Inspector
	[Export] private string LocalImagePath = "user://Images/CharacterImages/"; // Path to stored images
	[Export] private string CharacterDataFile = "user://Images/character_data.json"; // Local JSON file path

	private GridContainer _gridContainer;
	private Label _titleLabel;
	private Button _backButton;
	private APIClient _apiClient;
	private Dictionary<int, JsonElement> _characterData;
	private HashSet<int> _ownedCards;

	public override async void _Ready()
	{
		GD.Print("Collection scene ready. Initializing...");

		// Get nodes
		_gridContainer = GetNode<GridContainer>("VBoxContainer/ScrollContainer/GridContainer");
		_titleLabel = GetNode<Label>("VBoxContainer/Label");
		_backButton = GetNode<Button>("VBoxContainer/Button");

		GD.Print("Nodes fetched successfully.");

		// Initialize APIClient
		_apiClient = new APIClient();
		AddChild(_apiClient);

		// Load player and character data
		await LoadPlayerData();
		await LoadCharacterData();

		// Populate the collection
		PopulateCollection();
	}

	private async Task LoadPlayerData()
	{
		GD.Print("Fetching player data...");

		// Fetch player data using APIClient
		JsonDocument playerDataJson = await _apiClient.GetPlayerDataAsync();

		if (playerDataJson == null)
		{
			GD.PrintErr("Failed to load player data.");
			return;
		}

		GD.Print("Player data fetched successfully.");

		// Extract OwnedCards
		_ownedCards = new HashSet<int>();
		foreach (var cardId in playerDataJson.RootElement.GetProperty("OwnedCards").EnumerateArray())
		{
			_ownedCards.Add(cardId.GetInt32());
		}

		GD.Print($"Owned cards loaded. Total owned cards: {_ownedCards.Count}");
	}

	private async Task LoadCharacterData()
	{
		GD.Print("Loading character data...");

		// Load character data from local file
		string globalCharacterDataPath = ProjectSettings.GlobalizePath(CharacterDataFile);

		if (File.Exists(globalCharacterDataPath))
		{
			GD.Print($"Character data file found: {globalCharacterDataPath}");
			string characterDataContent = await File.ReadAllTextAsync(globalCharacterDataPath);
			JsonDocument characterDataJson = JsonDocument.Parse(characterDataContent);

			// Populate _characterData dictionary
			_characterData = new Dictionary<int, JsonElement>();
			foreach (var character in characterDataJson.RootElement.EnumerateArray())
			{
				int id = character.GetProperty("ID").GetInt32();
				_characterData[id] = character;
			}

			GD.Print($"Character data loaded successfully. Total characters: {_characterData.Count}");
		}
		else
		{
			GD.PrintErr($"Character data file not found: {globalCharacterDataPath}");
		}
	}

	private void PopulateCollection()
	{
		if (_characterData == null)
		{
			GD.PrintErr("Character data is null. Cannot populate collection.");
			return;
		}

		if (_ownedCards == null)
		{
			GD.PrintErr("Owned cards data is null. Cannot populate collection.");
			return;
		}

		GD.Print("Populating collection grid...");

		foreach (var character in _characterData.Values)
		{
			// Extract details from character data
			int id = character.GetProperty("ID").GetInt32();
			string faction = character.GetProperty("Faction").GetString().Replace(" ", "");
			string role = character.GetProperty("Role").GetString().Replace(" ", "");
			string characterName = character.GetProperty("Character_Name").GetString().Replace(" ", "");

			GD.Print($"Processing character: {characterName} (ID: {id}, Faction: {faction}, Role: {role})");

			// Build the local image path
			string localImagePath = $"{LocalImagePath}{faction}/{role}/{characterName}.png";
			string globalImagePath = ProjectSettings.GlobalizePath(localImagePath);

			// Check if the character is owned
			bool isOwned = _ownedCards.Contains(id);
			GD.Print(isOwned
				? $"Character {characterName} is owned."
				: $"Character {characterName} is not owned.");

			if (isOwned && !File.Exists(globalImagePath))
			{
				GD.PrintErr($"Image file for owned character not found: {globalImagePath}");
			}

			// Instantiate the card
			CardDisplay card = (CardDisplay)_cardScene.Instantiate();
			card.InitializeCard(
				isOwned && File.Exists(globalImagePath) ? (Texture2D)ResourceLoader.Load(localImagePath) : null,
				(Texture2D)ResourceLoader.Load("res://Assets/MythraleaLogo.png"), // Back texture
				isOwned,
				new Vector2(200, 350) // Card size (adjust as needed)
			);

			GD.Print($"Card for {characterName} instantiated and added to grid.");

			// Add the card to the grid
			_gridContainer.AddChild(card);
		}

		GD.Print("Collection grid population complete.");
	}
}
