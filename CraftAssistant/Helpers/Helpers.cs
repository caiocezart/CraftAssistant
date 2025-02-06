using System.Reflection;
using ExileCore2.Shared.Enums;
using Newtonsoft.Json;

namespace CraftAssistant;

public static class Helpers
{
    public static StatusMessage ValidateItem(Item item)
    {
        var _classesSupported = new List<string>
        {
            "Body Armour",
            "Belt",
            "Boots",
            "Gloves",
            "Helmet",
            "Ring",
            "Amulet",
            "Quiver",
            "Shield",
            "Weapon"
        };

        if (item == null)
        {
            return new StatusMessage
            {
                Code = StatusCode.InternalError,
                Message = "Failed to get game data. Try again",
                Show = true
            };
        }

        if (item.ItemRarity == ItemRarity.Unique)
        {
            return new StatusMessage
            {
                Code = StatusCode.CraftingError,
                Message = "Unique items are not supported for crafting",
                Show = true
            };
        }

        if (!_classesSupported.Contains(item.ClassName))
        {
            return new StatusMessage
            {
                Code = StatusCode.CraftingError,
                Message = $"This item is not supported for crafting: {item.ClassName}",
                Show = true
            };
        }

        return new StatusMessage
        {
            Message = "Item is valid for crafting"
        };
    }

    public static bool IsWithinRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    public static Dictionary<string, object> GetAllAttributes(object obj)
    {
        var attributes = new Dictionary<string, object>();

        if (obj == null)
        {
            attributes.Add("Error", "Object is null");
            return attributes;
        }

        // Get the type of the object
        Type type = obj.GetType();

        // Get all public properties
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                object value = property.GetValue(obj);
                attributes[property.Name] = value ?? "null";
            }
            catch (Exception ex)
            {
                attributes[property.Name] = $"Error: {ex.Message}";
            }
        }

        // Get all public fields
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                object value = field.GetValue(obj);
                attributes[field.Name] = value ?? "null";
            }
            catch (Exception ex)
            {
                attributes[field.Name] = $"Error: {ex.Message}";
            }
        }

        return attributes;
    }

    public static void ExportToJson(object obj)
    {
        // Export to JSON
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hovered_item.json");
        File.WriteAllText(filePath, json);
    }
}