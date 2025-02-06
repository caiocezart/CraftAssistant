using System.Data;
using System.Numerics;
using System.Text.Json.Serialization;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ImGuiNET;

namespace CraftAssistant;

public class Settings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu(null, "Key to Open/Close CraftAssistant window")]
    public HotkeyNode ToggleWindowKey { get; set; } = new(Keys.F10);

    [Menu(null, "Key to inspect item hovered by the mouse")]
    public HotkeyNode UIHoverInspectItem { get; set; } = new(Keys.F9);
    
    public string PluginDataDir { get; set; } = "";
    
    // public string PoeDataFileName { get; set; } = "";
    
    // public bool PoeDataGenerating { get; set; } = false;
    
    // public bool PoeDataAvailable { get; set; } = false;

    // public string PoeDataPath { get; set; } = "";
    
    public bool EnableTextures { get; set; } = false;
    
    public bool TexturesAvailable { get; set; } = false;
    
    public string TexturesZipName { get; set; } = "";

    public bool ShowDebug { get; set; } = false;

    public int LogLevel { get; set; } = 0;
}

// [Submenu]
// public class TexturesSettings
// {
//     [Menu(null, "Location of your Plugin data directory")]
//     public TextNode PluginDataDir { get; set; } = new("");

//     [Menu(null, "Name of your textures archive (zip) file")]
//     public TextNode TexturesZipName { get; set; } = new("");
// }

// [Submenu(CollapsedByDefault = true)]
// public class DebugSettings
// {
//     [Menu("Show Debug Window")] 
//     public ToggleNode ShowWindow { get; set; } = new ToggleNode(false);

//     [Menu(null, "Debug (0) | Error (1) | Info (2) (needs restart)")]
//     public RangeNode<int> LogLevel { get; set; } = new(2, 0, 2);
// }

// [Submenu]
// public class TexturesSettings
// {
//     public TexturesSettings()
//     {
//         TexturesPanel = new CustomNode
//         {
//             DrawDelegate = () =>
//             {
//                 if (ImGui.BeginTable("textures_status_table", 2, ImGuiTableFlags.NoBordersInBody))
//                 {
//                     ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.WidthStretch);
//                     ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

//                     ImGui.TableNextRow();
//                     ImGui.TableNextColumn();

//                     Vector4 statusColor = _texturesAvailable ?
//                         new Vector4(0.2f, 0.8f, 0.2f, 1.0f) : // Green
//                         new Vector4(1.0f, 0.6f, 0.0f, 1.0f);  // Orange
//                     ImGui.TextColored(statusColor, _statusMessage);

//                     ImGui.TableNextColumn();

//                     if (ImGui.Button("Check Textures File"))
//                     {
//                         if (OnCheckPressed != null)
//                         {
//                             var handler = OnCheckPressed;
//                             OnCheckPressed = null;
//                             handler();
//                         }
//                     }

//                     ImGui.EndTable();
//                 }
//             }
//         };
//     }

//     [JsonIgnore]
//     public CustomNode TexturesPanel { get; set; }

    
//     [Menu(null, "Location of your Plugin data directory")]
//     public TextNode PluginDataDir { get; set; } = "";

//     [Menu(null, "Name of your textures archive (zip) file")]
//     public TextNode TexturesZipName { get; set; } = "";

//     [JsonIgnore]
//     private bool _texturesAvailable;
//     [JsonIgnore]
//     private string _statusMessage = "Checking textures...";
//     public event Action OnCheckPressed;
    
//     public void UpdateStatus(bool available, string message)
//     {
//         _texturesAvailable = available;
//         _statusMessage = message;
//     }
//     public void SetPluginDataDir(string pluginDir)
//     {
//         PluginDataDir.Value = pluginDir;
//     }
// }

// [Submenu(CollapsedByDefault = true)]
// public class DebugSettings
// {
//     [Menu("Show Debug Window")] public ToggleNode ShowWindow { get; set; } = new ToggleNode(false);

//     [Menu(null, "Debug (0) | Error (1) | Info (2) (needs restart)")]
//     public RangeNode<int> LogLevel { get; set; } = new(2, 0, 2);
// }

