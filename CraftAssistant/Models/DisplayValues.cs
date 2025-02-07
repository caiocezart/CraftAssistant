// using System.Collections.Generic;
// using System.Text.RegularExpressions;

// namespace CraftAssistant;

// public class DisplayValues
// {
//     private readonly List<int> _values;
//     public bool IsPercentage { get; }
//     private static readonly Regex PlaceholderPattern = new("#");

//     private DisplayValues(List<int> values, bool isPercentage)
//     {
//         _values = values ?? new List<int>();
//         IsPercentage = isPercentage;
//     }

//     public string FormatDescription(string description)
//     {
//         var parts = description.Split(',');
//         var formattedParts = new List<string>();
//         var valueIndex = 0;

//         foreach (var part in parts)
//         {
//             var formatted = PlaceholderPattern.Replace(part.Trim(), _ => FormatValue(valueIndex++));
//             formattedParts.Add(formatted);
//         }

//         return string.Join("\n", formattedParts);
//     }

//     private string FormatValue(int index)
//     {
//         if (index >= _values.Count) return "#";
//         var value = _values[index];
//         return IsPercentage ? ((float)value / 60).ToString("0.#") : value.ToString();
//     }

//     public static DisplayValues Create(List<int> values, bool isPercentage = false)
//         => new(values, isPercentage);
// }