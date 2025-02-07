using ExileCore2;
using ExileCore2.Shared.Nodes;
using ImGuiNET;

using Logger = ExileCore2CustomLogger.Logger;

namespace CraftAssistant;

public class CraftAssistant : BaseSettingsPlugin<Settings>
{
    public static CraftAssistant Main;
    private Logger _logger;
    private ItemProcessor _itemProcessor;
    private CraftingUI _ui;
    public State _state;
    private Storage _storage;
    public CraftingButtons _craftingButtons = new CraftingButtons();
    private HotkeyNode KeySelectItem => Settings.UIHoverInspectItem;
    private HotkeyNode KeyToggleWindow => Settings.ToggleWindowKey;
    private string _dataPath;
    // private string _poeDataFileName => Path.Combine(Settings.PluginDataDir, Settings.PoeDataFileName);
    private string _archiveZipPath => Path.Combine(Settings.PluginDataDir, Settings.TexturesZipName);
    public CraftAssistant()
    {
        Main = this;
    }

    public override bool Initialise()
    {
        var pluginName = "CraftAssistant";
        _logger = new Logger((ExileCore2CustomLogger.LogLevel)Settings.LogLevel);
        _state = new State(_logger);

        _dataPath = Path.Combine(GetPluginDir(pluginName), "data");

        _storage = new Storage(_logger, _dataPath);
        _itemProcessor = new ItemProcessor(_logger, GameController);
        _ui = new CraftingUI(_logger, _craftingButtons);

        _state.ItemsSaved = _storage.GetItemsSaved();
        _state.PoeData = _storage.LoadPoeData();
 
        if (Settings.PluginDataDir == "") Settings.PluginDataDir = _dataPath;
        // if (Settings.PoeDataFileName == "") Settings.PoeDataFileName = "poe.json";
        if (Settings.TexturesZipName == "") Settings.TexturesZipName = "textures.zip";

        // CheckPoeDataStatus();
        CheckTexturesStatus();
        _state.Running = true;
        return true;
    }

    public override void Render()
    {
        if (!Settings.Enable || !_state.Running) return;

        if (Settings.ShowDebug)
        {
            _logger.Render();
        }

        if (_state.CurrentUIState.IsMainWindowOpen)
        {
            _ui.RenderUI(_state, ProcessAction);
        }
    }

    public override void Tick()
    {
        if (!Settings.Enable || !_state.Running) return;

        if (KeySelectItem.PressedOnce())
        {
            _state.CurrentUIState.IsMainWindowOpen = true;
            ProcessHoveredItem();
        }

        if (KeyToggleWindow.PressedOnce())
        {
            _state.CurrentUIState.IsMainWindowOpen = !_state.CurrentUIState.IsMainWindowOpen;
        }
    }

    // public async void GeneratePoeData()
    // {
    //     Settings.PoeDataGenerating = true;
    //     _logger.Debug($"Generating poe data {_poeDataFileName}");
    //     await DataScraper.ConsolidateData(_logger, _poeDataFileName);
    //     Settings.PoeDataGenerating = false;
    //     CheckPoeDataStatus();
    // }

    // public void CheckPoeDataStatus()
    // {
    //     _logger.Debug($"Checking poe data status {_poeDataFileName}");
    //     var poeDataAvailable = File.Exists(_poeDataFileName);
    //     Settings.PoeDataAvailable = poeDataAvailable;
    //     _state.Running = poeDataAvailable;
    // }

    public void CheckTexturesStatus()
    {
        _logger.Debug($"Checking textures status {_archiveZipPath}");
        var texturesAvailable = File.Exists(_archiveZipPath);
        Settings.TexturesAvailable = texturesAvailable;
        
        var enableTextures = Settings.EnableTextures;
        _state.CurrentUIState.EnableTextures = enableTextures;
    
        if (texturesAvailable && enableTextures)
        {           
            foreach (var button in _craftingButtons.Materials)
            {
                button.TextureId = ImportImageToGraphics(button.ImagePath);
            }

            return;
        }
    }

    private void ProcessAction(string action)
    {
        try
        {
            switch (action)
            {
                case "CloneItem":
                    try
                    {
                        if (_state.HasItems)
                        {
                            var clonedItem = ItemMapper.CloneItem(_state.CurrentItem);
                            _state.AddItem(clonedItem);
                            _logger.Debug($"Cloning item: {clonedItem.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error cloning item: {ex.Message}");
                    }
                    break;

                case "SaveItem":
                    _storage.SaveItem(_state.CurrentItem);
                    _state.UpdateSavedItems(_storage.GetItemsSaved());
                    _state.SetStatus(new StatusMessage("Item saved", StatusCode.Success));
                    break;

                case "DeleteItem":
                    var currentItem = _state.CurrentItem;
                    if (currentItem != null)
                    {
                        _storage.DeleteItem(currentItem.FileName);
                        _state.RemoveItem(currentItem.Id);
                        _state.UpdateSavedItems(_storage.GetItemsSaved());
                        _state.SetStatus(new StatusMessage("Item deleted", StatusCode.Success));
                    }
                    break;

                case "LoadItem":
                    if (string.IsNullOrEmpty(_state.ItemSelected))
                    {
                        _state.SetStatus(new StatusMessage("No Item selected", StatusCode.Warning));
                        return;
                    }

                    if (_state.Items.FirstOrDefault(item => item.FileName == _state.ItemSelected) != null)
                    {
                        _state.SetStatus(new StatusMessage("Item is already loaded", StatusCode.Warning));
                        return;
                    }

                    var storedItem = _storage.LoadItem(_state.ItemSelected);
                    try
                    {
                        var item = ItemMapper.ToItem(storedItem);

                        // Load texture
                        item.TextureId = ImportImageToGraphics(item.ResourcePath);

                        // Find and set PoeDataBaseGroup
                        item.PoeDataBaseGroup = _itemProcessor.FindPoeDataItemGroup(
                            item.BaseGroups(),
                            _state.PoeData
                        );

                        _state.AddItem(item);
                        _state.SetStatus(new StatusMessage($"Item {_state.ItemSelected} loaded.", StatusCode.Success));
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error loading {storedItem} item: {ex.Message}");
                    }
                    break;

                case "ClearItems":
                    _state.ClearItems();
                    _state.SetStatus(new StatusMessage("Item deleted", StatusCode.Success));                    
                    break;
            }
            _state.ClearError();
        }
        catch (Exception ex)
        {
            _state.SetError(ex);
            _logger.Error($"Error processing action {action}: {ex.Message}");
        }
    }

    private void ProcessHoveredItem()
    {
        _logger.Debug("Processing hovered item");
        var hoveredEntity = GameController.Game.IngameState.UIHover?.Entity;
        if (hoveredEntity is not { IsValid: true }) return;

        var (item, status) = _itemProcessor.ProcessEntity(_state, hoveredEntity);
        _logger.Debug($"Processed hovered item: {item?.Name}");
        LoadItem(item, status);
    }

    private void LoadItem(Item item, StatusMessage status)
    {
        if (item == null)
        {
            _state.SetStatus(status);
            return;
        }

        _logger.Debug($"Loading item: {item.Name}");

        // Load item texture
        item.TextureId = ImportImageToGraphics(
            item.ResourcePath
        );

        _state.AddItem(item);
        _state.SetStatus(status);
    }

    public IntPtr ImportImageToGraphics(string texturePath)
    {
        if (string.IsNullOrEmpty(texturePath)) return 0;

        string textureName = Path.GetFileName(texturePath);
        if (string.IsNullOrEmpty(textureName))
        {
            _logger.Debug($"Texture path {texturePath} is not valid.");
            return 0;
        }

        try
        {
            var textureId = Graphics.GetTextureId(textureName);
            if (textureId != 0)
            {
                _logger.Debug($"Texture {textureName} is already loaded. Returning textureId {textureId}");
                return textureId;
            }
        }
        catch
        {
            _logger.Debug($"Texture {textureName} is not loaded yet. Will try to load now.");
        }

        var texture = _storage.LoadImageFromArchive(_archiveZipPath, texturePath);
        if (texture == null)
        {
            _logger.Debug($"Texture {textureName} is not found in the archive.");
            return 0;
        }

        try
        {
            Graphics.AddOrUpdateImage(textureName, texture);
            _logger.Debug($"Texture {textureName} loaded successfully.");
        }
        catch
        {
            _logger.Debug($"Failed to load texture to Graphics {textureName}.");
            return 0;
        }

        return Graphics.GetTextureId(textureName);
    }

    private string GetPluginDir(string pluginName)
    {
        var pluginPathOnDisk = PluginManager.Plugins.First(p => p.Name == pluginName).PathOnDisk;
        var pluginDir = Path.GetDirectoryName(pluginPathOnDisk) ?? throw new Exception("Failed to get plugin directory.");

        return pluginDir;
    }

    public override void DrawSettings()
    {
        if (ImGui.BeginTabBar("CraftAssistantSettings"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                base.DrawSettings();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                CustomSettings.DrawDataSettings();
                ImGui.EndTabItem();
            }

            // if (ImGui.BeginTabItem("Textures"))
            // {
            //     ImGui.EndTabItem();
            // }

            if (ImGui.BeginTabItem("Debug"))
            {
                CustomSettings.DrawDebugSettings();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}
