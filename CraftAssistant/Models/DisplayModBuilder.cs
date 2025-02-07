// using System.Collections.Generic;
// using ExileCore2.Shared.Enums;

// namespace CraftAssistant;

// public class DisplayModBuilder
// {
//     private ModType _affixType;
//     private DisplayValues _values;
//     private string _description;
//     private PoeDataModTier _tier;
//     private PoeDataMod _baseData;

//     public DisplayModBuilder WithAffixType(ModType affixType)
//     {
//         _affixType = affixType;
//         return this;
//     }

//     public DisplayModBuilder WithValues(List<int> values, bool isPercentage = false)
//     {
//         _values = DisplayValues.Create(values, isPercentage);
//         return this;
//     }

//     public DisplayModBuilder WithDescription(string description)
//     {
//         _description = description;
//         return this;
//     }

//     public DisplayModBuilder WithTier(PoeDataModTier tier)
//     {
//         _tier = tier;
//         return this;
//     }

//     public DisplayModBuilder WithBaseData(PoeDataMod baseData)
//     {
//         _baseData = baseData;
//         return this;
//     }

//     public DisplayMod Build()
//     {
//         return new DisplayMod(
//             _affixType,
//             _values ?? DisplayValues.Create(new List<int>()),
//             _description ?? string.Empty,
//             _tier,
//             _baseData
//         );
//     }
// }