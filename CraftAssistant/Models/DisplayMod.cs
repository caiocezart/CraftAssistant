using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExileCore2.Shared.Enums;

namespace CraftAssistant;

public class DisplayMod
{
    public ModType AffixType { get; set; }
    public List<int> Values { get; set; }
    public string Description { get; set; }
    public PoeDataModTier Tier { get; set; }
    public bool Float { get; set; }

    [JsonIgnore]
    public PoeDataMod BaseData { get; set; }

    public string NameWithValues()
    {

        var mods = Description?.Split(",");
        var formattedDisplay = new List<string>();
        for (var i = 0; i < mods.Length; i++)
        {
            formattedDisplay.Add(mods[i].Trim().Replace("#", Float ? ((float)Values[i] / 60).ToString("0.#") : Values[i].ToString()));
        }
        return string.Join("\n", formattedDisplay);
    }
}