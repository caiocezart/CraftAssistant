using System;
using System.Collections.Generic;

namespace CraftAssistant;

public class Button
{
    public string Name { get; set; }
    public string ImagePath { get; set; } = "";
    public bool Selected { get; set; } = false;
    public IntPtr TextureId { get; set; } = 0;
}

public class CraftingButtons
{
    public List<Button> Materials { get; }

    public CraftingButtons()
    {
        Materials = new List<Button>
        {
            new()
            {
                Name = "TransmutationOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyUpgradeToMagic.dds"
            },
            new()
            {
                Name = "AugmentationOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyAddModToMagic.dds"
            },
            new()
            {
                Name = "AlchemyOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyUpgradeToRare.dds"
            },
            new()
            {
                Name = "RegalOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyUpgradeMagicToRare.dds"
            },
            new()
            {
                Name = "ChaosOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyRerollRare.dds"
            },
            new()
            {
                Name = "VaalOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyVaal.dds"
            },
            new()
            {
                Name = "ExaltOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyAddModToRare.dds"
            },
            new()
            {
                Name = "DivineOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyModValues.dds"
            },
            new()
            {
                Name = "AnnulmentOrb",
                ImagePath = "Art/2DItems/Currency/AnnullOrb.dds"
            },
            new()
            {
                Name = "ChanceOrb",
                ImagePath = "Art/2DItems/Currency/CurrencyUpgradeToUnique.dds"
            }
        };
    }
}