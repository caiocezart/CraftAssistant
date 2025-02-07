
using System.Diagnostics;
using System.Numerics;
using ExileCore2;
using ImGuiNET;
using static CraftAssistant.CraftAssistant;

namespace CraftAssistant;

public class CustomSettings
{
    private static Vector4 _colorGreen = new Vector4(0.2f, 0.8f, 0.2f, 1.0f);
    private static Vector4 _colorYellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
    private static Vector4 _colorRed = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
    private static Dictionary<string, Vector4> _statusColors = new Dictionary<string, Vector4>
    {
        { "Generating PoE data - Craft Assistant not running", _colorYellow },
        { "PoE data missing - Craft Assistant not running", _colorRed },
        { "PoE data available - Craft Assistant running", _colorGreen },
        { "Textures available", _colorGreen },
        { "Textures missing", _colorRed }
    };
    private static bool _texturesExtractorError = false;
    private static string _texturesExtractorMessage = "";

    public static void DrawDataSettings()
    {
        // var poeDataMessage = "";
        // if (Main.Settings.PoeDataGenerating) poeDataMessage = "Generating PoE data - Craft Assistant not running";
        // else
        // {

        //     poeDataMessage = !Main.Settings.PoeDataGenerating && Main.Settings.PoeDataAvailable ?
        // "PoE data available - Craft Assistant running" : "PoE data missing - Craft Assistant not running";
        // }

        // ImGui.TextColored(_statusColors[poeDataMessage], poeDataMessage);

        // ImGui.Spacing();

        // var pluginDir = Main.Settings.PluginDataDir;

        // ImGui.Text("Plugin data directory:");
        // if (ImGui.InputText("##plugindir", ref pluginDir, 256))
        // {
        //     Main.Settings.PluginDataDir = pluginDir;
        // }

        // var poeDataFileName = Main.Settings.PoeDataFileName;
        // ImGui.Text("Poe data file name:");
        // if (ImGui.InputText("##poedatafilename", ref poeDataFileName, 256))
        // {
        //     Main.Settings.PoeDataFileName = poeDataFileName;
        // }

        // ImGui.Spacing();

        // if (ImGui.Button("Check PoE data"))
        // {
        //     Main.CheckPoeDataStatus();
        // }

        // ImGui.SameLine();

        // if (ImGui.Button("Generate PoE data"))
        // {
        //     Main.GeneratePoeData();
        // }

        // TEXTURES

        var texturesAvailable = Main.Settings.TexturesAvailable;
        var enableTextures = Main.Settings.EnableTextures;

        ImGui.BeginDisabled(!texturesAvailable);
        if (ImGui.Checkbox("Enable Textures", ref enableTextures))
        {
            Main.Settings.EnableTextures = enableTextures;
            Main.CheckTexturesStatus();
        }
        ImGui.EndDisabled();

        var archiveName = Main.Settings.TexturesZipName;
        ImGui.Text("Textures Archive Name:");
        if (ImGui.InputText("##archivename", ref archiveName, 256))
        {
            Main.Settings.TexturesZipName = archiveName;
        }

        ImGui.Spacing();

        Vector4 statusColor = Main.Settings.TexturesAvailable ?
            _colorGreen : _colorRed;

        ImGui.TextColored(statusColor, Main.Settings.TexturesAvailable ?
            "Textures available" :
            "Textures missing");

        if (ImGui.Button("Check Textures"))
        {
            Main.CheckTexturesStatus();
        }

        ImGui.Spacing();
        
        ImGui.BeginChild("TexturesInfo", new Vector2(ImGui.GetContentRegionAvail().X, 300), ImGuiChildFlags.Border);

        ImGui.TextWrapped("To generate textures, you will need to run the external ExileCore2TexturesHandler tool.\n\n" +
                         "The tool can be found where this plugin was compiled. You only need to run it once (unless you are missing textures).\n\n" +
                         "You can click the button below to run the tool or go to the plugin folder to run it manually.\n\n" +
                         "Notes: \n\nTextures are optional and not required for the plugin to function.\n\n" +
                         "Supports both standalone and steam game versions.\n\n\n" +
                         "How to use: \n\n- Click the button below\n" + 
                         "- Close the game and ExileCore2\n" + 
                         "- Go back to the tool\n" + 
                         "- Select the folder where your game is installed\n" +
                         "- Click 'Extract Textures'\n" + 
                         "- Open the game and ExileCore2 again\n" + 
                         "- Check this tab to see status if Textures are now available\n" +
                         "- Tick the 'Enable Textures' checkbox to use them in the plugin");
        ImGui.EndChild();
        

        if (ImGui.Button("Open Textures Handler Location"))
        {
            var texturesHandlerExe = Path.Combine(Main.Settings.PluginDataDir, "ExileCore2TexturesHandler.exe");
            if (File.Exists(texturesHandlerExe))
            {
                Process.Start("explorer.exe", texturesHandlerExe);
            }
            else
            {
                _texturesExtractorError = true;
                _texturesExtractorMessage = "Error: Textures Handler executable not found.";
            }
        }

        if (_texturesExtractorError)
        {
            ImGui.TextColored(_colorYellow, _texturesExtractorMessage);
        }

    }

    public static void DrawDebugSettings()


    {
        var showDebug = Main.Settings.ShowDebug;
        if (ImGui.Checkbox("Show Debug Window", ref showDebug))
        {
            Main.Settings.ShowDebug = showDebug;
        }

        var logLevel = Main.Settings.LogLevel;
        ImGui.Text("Log Level:");
        if (ImGui.SliderInt("##loglevel", ref logLevel, 0, 2,
            GetLogLevelText(logLevel)))
        {
            Main.Settings.LogLevel = logLevel;
        }

        ImGui.SameLine();

        ImGui.TextDisabled("(?)");

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("0: Debug\n1: Error\n2: Info");
        }
    }

    private static string GetLogLevelText(int level)
    {
        return level switch
        {
            0 => "Debug",
            1 => "Error",
            2 => "Info",
            _ => "Unknown"
        };
    }
}