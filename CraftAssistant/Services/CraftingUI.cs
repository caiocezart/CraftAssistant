using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore2.Shared.Enums;
using ImGuiNET;
using Logger = ExileCore2CustomLogger.Logger;

namespace CraftAssistant;

public class CraftingUI
{
    private readonly Logger _logger;
    private readonly ImGuiStylePtr _style;
    private readonly float _padding;
    private readonly CraftingButtons _craftingButtons;

    private readonly Dictionary<string, string> _cachedStrings = new();

    public CraftingUI(Logger logger, CraftingButtons craftingButtons)
    {
        _logger = logger;
        _style = ImGui.GetStyle();
        _padding = Constants.PADDING;
        _craftingButtons = craftingButtons;

        SetDefaultStyling();
    }

    private void SetDefaultStyling()
    {
        _style.WindowPadding = new Vector2(_padding, _padding);
    }

    public void RenderUI(State state, Action<string> onAction)
    {
        if (!state.CurrentUIState.IsMainWindowOpen) return;

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize;
        windowFlags |= ImGuiWindowFlags.AlwaysAutoResize;

        var isWindowOpen = true;
        if (ImGui.Begin("CraftingAssistant", ref isWindowOpen))
        {
            RenderStatusMessage(state.Status);

            RenderControls(state, onAction);

            if (!state.HasItems) return;
            {
                if (ImGui.BeginTabBar("ItemTabs"))
                {
                    RenderTabs(state);
                    ImGui.EndTabBar();
                }
            }
        }
        if (!isWindowOpen)
        {
            _logger.Debug("Closing window");
            state.CurrentUIState.IsMainWindowOpen = false;
        }
        ImGui.End();
    }


    private void RenderStatusMessage(StatusMessage status)
    {
        if (!status.Show) return;

        var color = status.Code switch
        {
            StatusCode.Success => new Vector4(0, 1, 0, 1),
            StatusCode.Warning => new Vector4(1, 1, 0, 1),
            StatusCode.CraftingError => new Vector4(1, 1, 0, 1),
            StatusCode.InternalError => new Vector4(1, 0, 0, 1),
            _ => new Vector4(1, 1, 1, 1)
        };

        ImGui.TextColored(color, status.Message);
        ImGui.Separator();
    }

    private void RenderControls(State state, Action<string> onAction)
    {
        var buttonHeight = ImGui.GetFrameHeight();
        var padding = ImGui.GetStyle().FramePadding.Y * 2;
        var spacing = ImGui.GetStyle().ItemSpacing.Y;

        var loadItemWindow = new Vector2(
            ImGui.GetContentRegionAvail().X,
            buttonHeight + padding * 2 + spacing
        );

        ImGui.BeginChild($"Load Item", loadItemWindow, ImGuiChildFlags.Border);

        RenderLoadItem(state, onAction);
        ImGui.EndChild();
        if (state.HasItems && state.CurrentItem != null)
        {
            var qtyRows = 1 + (state.CurrentUIState.Action != ItemActionType.None ? 1 : 0);

            var itemControlsWindow = new Vector2(
                ImGui.GetContentRegionAvail().X,
                (buttonHeight * qtyRows) + // Height for all rows
                padding * 2 + // padding top and bottom
                (spacing * qtyRows) +
                (3 * qtyRows) // extra pixels to prevent scrollbar
            );

            ImGui.BeginChild($"Item Controls", itemControlsWindow, ImGuiChildFlags.Border);

            RenderItemControls(state, onAction);

            if (state.CurrentUIState.Action != ItemActionType.None)
            {
                RenderActionItemConfirmation(state, state.CurrentUIState.Action, onAction);
            }

            ImGui.EndChild();
        }

    }

    private void RenderLoadItem(State state, Action<string> onAction)
    {
        if (ImGui.BeginCombo("##LoadItemCombo", state.ItemSelected ?? "Load Item"))
        {
            foreach (var itemName in state.ItemsSaved)
            {
                bool isSelected = itemName == state.ItemSelected;
                if (ImGui.Selectable(itemName, isSelected))
                {
                    state.ItemSelected = itemName;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        if (ImGui.Button("Load") && !string.IsNullOrEmpty(state.ItemSelected))
        {
            onAction?.Invoke("LoadItem");
        }
    }

    private void RenderItemControls(State state, Action<string> onAction)
    {
        ImGui.PushID($"ItemNameEdit");

        var refFilename = state.CurrentItem.FileName;
        if (ImGui.InputText("", ref refFilename, 100))
        {
            state.CurrentItem.FileName = refFilename;
        }
        ImGui.PopID();

        ImGui.SameLine();

        if (ImGui.Button("Save Item"))
        {
            var fileExists = state.ItemsSaved.FirstOrDefault(item => item == state.CurrentItem.FileName) != null;
            if (!fileExists)
            {
                onAction?.Invoke("SaveItem");
            }
            else
            {
                state.CurrentUIState.Action = ItemActionType.OverwriteItem;
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Clone Item"))
        {
            onAction?.Invoke("CloneItem");
        }

        ImGui.SameLine();

        if (ImGui.Button("Delete Item"))
        {
            state.CurrentUIState.Action = ItemActionType.DeleteItem;
        }

        ImGui.SameLine();

        if (ImGui.Button("Clear Items"))
        {
            state.CurrentUIState.Action = ItemActionType.ClearItems;
        }
    }

    private void RenderActionItemConfirmation(State state, ItemActionType action, Action<string> onAction)
    {
        var actionName = action.ToString();
        var itemAction = "";
        switch (action)
        {
            case ItemActionType.OverwriteItem:
                itemAction = "Do you want to overwrite the item?";
                break;
            case ItemActionType.DeleteItem:
                itemAction = "Do you want to delete the item?";
                break;
            case ItemActionType.ClearItems:
                itemAction = "Do you want to clear all the items?";
                break;
            default:
                itemAction = "";
                break;
        }

        if(itemAction == "") return;

        ImGui.Text(itemAction);
        ImGui.SameLine();

        if (ImGui.Button("Yes"))
        {
            onAction?.Invoke(actionName);
            state.CurrentUIState.Action = ItemActionType.None;
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            state.CurrentUIState.Action = ItemActionType.None;
        }
    }

    private void RenderTabs(State state)
    {
        var items = state.Items.ToList();
        var itemsToRemove = new List<Guid>();

        // foreach (var item in items)
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var tabFocused = state.CurrentUIState.CurrentItemTab == item.Id;
            var tabOpen = true;

            ImGui.PushID($"Tab_{item.Id}");
            ImGui.SetNextItemOpen(tabFocused);
            if (ImGui.BeginTabItem($"{item.FileName}##{item.Id}", ref tabOpen))
            {
                if (ImGui.IsItemActive())
                {
                    state.SetCurrentItem(item);
                }

                var tabWindow = new Vector2(
                    ImGui.GetContentRegionAvail().X,
                    ImGui.GetContentRegionAvail().Y
                );

                ImGui.BeginChild($"{item.Id}", tabWindow, ImGuiChildFlags.Border);

                // state.SetCurrentItem(item);

                RenderUIItemDetails(item, state);

                ImGui.Dummy(new Vector2(0, _padding));
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, _padding));

                RenderPoeDataAffixesTable(state, "[GAME DATA] Item Prefixes", item.PoeDataBaseGroup.Prefixes);

                ImGui.Dummy(new Vector2(0, _padding));

                RenderPoeDataAffixesTable(state, "[GAME DATA] Item Suffixes", item.PoeDataBaseGroup.Suffixes);

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            if (!tabOpen)
            {
                _logger.Debug($"Tab closing requested: {item.FileName} (ID: {item.Id}, IsCurrentTab: {tabFocused})");
                itemsToRemove.Add(item.Id);
                
                if (tabFocused)
                {
                    var nextItem = items.FirstOrDefault(x => x.Id != item.Id);
                    if (nextItem != null)
                    {
                        _logger.Debug($"Switching to next tab: {nextItem.FileName} (ID: {nextItem.Id})");
                    }
                    else
                    {
                        _logger.Debug("No next tab available, clearing current tab");
                    }
                }
                // _logger.Debug($"Closing tab for {item.FileName}");
                // state.RemoveItem(item.Id);
                // i--;
                // state.SetStatus(new StatusMessage($"{item.FileName} tab closed", StatusCode.Success));
            }
            ImGui.PopID();
        }

        // Process removals after the loop
        foreach (var itemId in itemsToRemove)
        {
            _logger.Debug($"Removing tab: {itemId}");
            var item = items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                state.RemoveItem(item.Id);
                _logger.Debug($"Removed item: {item.FileName} (ID: {item.Id})");
                state.SetStatus(new StatusMessage($"{item.FileName} tab closed", StatusCode.Success));
            }
        }
    }

    private void RenderUIItemDetails(Item item, State state)
    {
        var itemWindow = new Vector2(
            ImGui.GetContentRegionAvail().X,
            ImGui.GetContentRegionAvail().Y
        );

        RenderUIItemImage(item, state);
        RenderCraftingButtonsGrid(item, state);

        ImGui.Dummy(new Vector2(0, _padding));

        RenderItemProperties(item);

        if(state.Status.Code == StatusCode.CraftingError) return;

        var lowestILvlAffix = item.Mods.OrderBy(mod => mod.Tier.ItemLevel).FirstOrDefault();

        ImGui.Dummy(new Vector2(0, _padding));

        RenderLabelValue("Red text:", "Lowest iLvl Affix", new Vector4(1, 0, 0, 1));
        RenderLabelValue("Yellow text:", "Item affix tier", new Vector4(1.0f, 1.0f, 0.0f, 1.0f));

        ImGui.Dummy(new Vector2(0, _padding));

        RenderItemAffixesTable("Item Prefixes", item.Mods.FindAll(mod => mod.AffixType == ModType.Prefix), lowestILvlAffix);

        ImGui.Dummy(new Vector2(0, _padding));
        RenderItemAffixesTable("Item Suffixes", item.Mods.FindAll(mod => mod.AffixType == ModType.Suffix), lowestILvlAffix);
    }

    private void RenderUIItemImage(Item item, State state)
    {
        var windowWidth = ImGui.GetContentRegionAvail().X;
        var windowHeight = ImGui.GetContentRegionAvail().Y;

        // Calculate base image size while maintaining aspect ratio
        float baseSize = Math.Min(windowWidth, windowHeight) * Constants.ITEM_DETAILS_IMAGE_SIZE;
        float cellSize = baseSize / Math.Max(item.Width, item.Height);
        // Adjust cell size based on item dimensions to prevent small items from being too large
        if (item.Width == 1 && item.Height == 1)
        {
            // For 1x1 items, limit the cell size to prevent them from being too large
            cellSize = Math.Min(cellSize, baseSize * 0.5f);
        }
        else
        {
            // For larger items, ensure they maintain a reasonable minimum size
            float minCellSize = baseSize * 0.15f;
            cellSize = Math.Max(cellSize, minCellSize);
        }

        float imageWidth = cellSize * item.Width;
        float imageHeight = cellSize * item.Height;
        float topMargin = Constants.ITEM_DETAILS_TOP_MARGIN;
        // Center the image horizontally
        Vector2 imagePos = new Vector2((windowWidth - imageWidth) * 0.5f, topMargin);

        if (state.CurrentUIState.EnableTextures && item.TextureId != 0)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
            ImGui.SetCursorPos(imagePos);
            ImGui.Image((IntPtr)item.TextureId, new Vector2(imageWidth, imageHeight), Vector2.Zero, Vector2.One, Vector4.One,
                new Vector4(1, 1, 1, 1));
            ImGui.PopStyleVar();
            return;
        }

        // Fallback image to text if no texture is available / enabled
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
        ImGui.SetCursorPos(imagePos);
        
        // Create child window for text
        ImGui.BeginChild("ItemNameBox", new Vector2(imageWidth, imageHeight), ImGuiChildFlags.Border);
        
        // Calculate text wrapping width
        float wrapWidth = imageWidth - (ImGui.GetStyle().FramePadding.X * 2);
        
        // Center text vertically and horizontally
        var text = item.Name;
        var textSize = ImGui.CalcTextSize(text, wrapWidth);
        var textPos = new Vector2(
            (imageWidth - textSize.X) * 0.5f,
            (imageHeight - textSize.Y) * 0.5f
        );
        
        ImGui.SetCursorPos(textPos);
        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth);
        ImGui.TextWrapped(text);
        ImGui.PopTextWrapPos();
        
        ImGui.EndChild();
        ImGui.PopStyleVar();
    }

    private void RenderCraftingButtonsGrid(Item item, State state)
    {
        float squareSize = 50f;
        float spacing = 2.0f;
        float totalWidth = (squareSize + spacing) * 5 - spacing;
        float craftingHeight = (squareSize + spacing) * 2 + _padding * 2;

        ImGui.BeginChild("Crafting", new Vector2(ImGui.GetContentRegionAvail().X, craftingHeight));

        float startX = (ImGui.GetContentRegionAvail().X - totalWidth) * 0.5f;
        float startY = ImGui.GetCursorPosY() + _padding;

        var drawList = ImGui.GetWindowDrawList();
        var windowPos = ImGui.GetWindowPos();

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                int index = row * 5 + col;
                if (index >= _craftingButtons.Materials.Count) break;

                var button = _craftingButtons.Materials[index];

                float x = startX + (squareSize + spacing) * col;
                float y = startY + (squareSize + spacing) * row;

                Vector2 min = windowPos + new Vector2(x, y);
                Vector2 max = min + new Vector2(squareSize, squareSize);

                bool isHovered = ImGui.IsMouseHoveringRect(min, max);
                uint color = isHovered ? 0xFFFFFFFF : 0xFFAAAAAA;

                bool isSelected = button.Selected;
                uint borderColor = _craftingButtons.Materials[index].Selected
                    ? ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 0, 1)) // Yellow when clicked
                    : ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)); // White by default

                drawList.AddRect(min, max, borderColor);
                if(state.CurrentUIState.EnableTextures && button.TextureId != 0)
                {
                    drawList.AddImage(button.TextureId, min + new Vector2(1, 1), max + new Vector2(-1, -1), Vector2.Zero, Vector2.One, color);
                }                
                else
                {
                    var shortName = button.Name.Length > 3 ? button.Name[..3] : button.Name;
                    var textSize = ImGui.CalcTextSize(shortName);
                    var textPos = new Vector2(
                        min.X + (squareSize - textSize.X) * 0.5f,
                        min.Y + (squareSize - textSize.Y) * 0.5f
                    );
                    drawList.AddText(textPos, color, shortName);
                }

                if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    // Unclick all other squares and toggle current one
                    for (int i = 0; i < _craftingButtons.Materials.Count; i++)
                    {
                        _craftingButtons.Materials[i].Selected =
                            (i == index) && !_craftingButtons.Materials[index].Selected;
                    }
                }

                if (isHovered && ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(button.Name);
                }
            }
        }

        ImGui.EndChild();

        // Calculate button width and position to center it
        float buttonWidth = 100f; // Set desired button width
        float windowWidth = ImGui.GetContentRegionAvail().X;
        float buttonX = (windowWidth - buttonWidth) * 0.5f;

        ImGui.SetCursorPosX(buttonX);

        ImGui.BeginDisabled(!_craftingButtons.Materials.Any(material => material.Selected));
        if (ImGui.Button("Craft", new Vector2(buttonWidth, 0)))
        {
            state.SetStatus(new StatusMessage($"Feature not implemented yet.", StatusCode.Warning));
        }
        ImGui.EndDisabled();
    }

    private void RenderItemProperties(Item item)
    {
        if (item == null) return;

        var labelsQty = 8;
        var itemDetailsHeight = ImGui.GetTextLineHeight() * labelsQty + _padding * 2;
        try
        {
            ImGui.BeginChild("Item Details", new Vector2(ImGui.GetContentRegionAvail().X, itemDetailsHeight), ImGuiChildFlags.Border);

            RenderLabelValue("Name:", item.UniqueName);
            RenderLabelValue("Base:", item.BaseName);

            var requirements = item.ItemRequirements();
            if (requirements != "")
            {
                RenderLabelValue("Requires:", requirements);
            }

            RenderLabelValue("Class:", item.ClassName);
            RenderLabelValue("Rarity:", ((ItemRarity)Enum.ToObject(typeof(ItemRarity), item.ItemRarity)).ToString());
            RenderLabelValue("Item Level:", item.ItemLevel.ToString());

            ImGui.EndChild();
        }
        catch (Exception ex)
        {
            _logger.Debug($"Error in RenderItemProperties: {ex}");
        }
    }

    private void RenderItemAffixesTable(string affixType, List<DisplayMod> affixes, DisplayMod lowestILvlAffix)
    {
        ImGui.SetCursorPos(new Vector2(_padding, ImGui.GetCursorPosY()));
        ImGui.Text($"{affixType} Available for Crafting: {3 - affixes.Count}");

        if (ImGui.BeginTable(affixType, 5,
            ImGuiTableFlags.Borders |
            ImGuiTableFlags.RowBg |
            ImGuiTableFlags.SizingFixedFit))
        {
            RenderAffixTableHeader(affixType);

            for (int i = 0; i < affixes.Count; i++)
            {
                var affix = affixes[i];
                RenderAffixTableRows(affix.NameWithValues(), affixType, i, lowestILvlAffix.Description == affix.Description, affix.Tier, affix.Description, affix.BaseData.Tiers);
            }

            ImGui.EndTable();
        }
    }

    private void RenderPoeDataAffixesTable(State state, string affixType, List<PoeDataMod> affixes)
    {
        if (ImGui.BeginTable(affixType, 5,
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.SizingFixedFit))
        {
            RenderAffixTableHeader(affixType);

            for (int i = 0; i < affixes.Count; i++)
            {
                var affix = affixes[i];
                if (state.CurrentItem.Mods.Any(mod => mod.Description == affix.Description))
                {
                    continue;
                }
                var tier = new PoeDataModTier
                {
                    Name = affix.Tiers.OrderBy(t => t.ItemLevel).First().Name,
                    ItemLevel = affix.Tiers.Max(t => t.ItemLevel),
                    Weighting = affix.Tiers.Sum(t => t.Weighting),
                    WeightPercent = affix.Tiers.Sum(t => t.AffixPercent)
                };

                RenderAffixTableRows(affix.SplitDescription, affixType, i, false, tier, affix.Description, affix.Tiers);
            }

            ImGui.EndTable();
        }
    }

    private void RenderAffixTableHeader(string affixType)
    {
        float totalWidth = ImGui.GetContentRegionAvail().X;
        float nameColumnWidth = totalWidth * 0.5f; // 35% of total width
        float otherColumnWidth = (totalWidth - nameColumnWidth) / 4;

        ImGui.TableSetupColumn(affixType, ImGuiTableColumnFlags.WidthFixed,
            nameColumnWidth);
        ImGui.TableSetupColumn("Tier", ImGuiTableColumnFlags.WidthFixed,
            otherColumnWidth);
        ImGui.TableSetupColumn("iLvl", ImGuiTableColumnFlags.WidthFixed,
            otherColumnWidth);
        ImGui.TableSetupColumn("Weight", ImGuiTableColumnFlags.WidthFixed,
            otherColumnWidth);
        ImGui.TableSetupColumn("Percent", ImGuiTableColumnFlags.WidthFixed,
            otherColumnWidth);
        ImGui.TableHeadersRow();
    }

    private void RenderAffixTableRows(string affixName, string affixType, int affixIndex, bool isLowestiLvlAffix, PoeDataModTier tier, string affixDescription, List<PoeDataModTier> tiers)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        if (affixType == "Item Prefixes" || affixType == "Item Suffixes")
        {
            if (isLowestiLvlAffix)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            }
        }

        bool isExpanded = ImGui.TreeNodeEx($"{affixName}###{affixType}{affixIndex}",
            ImGuiTreeNodeFlags.SpanFullWidth);

        ImGui.TableSetColumnIndex(1);
        ImGui.Text(tier.Name);
        ImGui.TableSetColumnIndex(2);
        ImGui.Text(tier.ItemLevel.ToString());
        ImGui.TableSetColumnIndex(3);
        ImGui.Text(tier.Weighting.ToString());
        ImGui.TableSetColumnIndex(4);
        ImGui.Text(tier.AffixPercent.ToString("0.##") + "%");

        if (isLowestiLvlAffix)
        {
            ImGui.PopStyleColor();
        }

        if (isExpanded)
        {
            foreach (var affixTier in tiers)
            {
                bool isItemTier = false;
                if (affixType == "Item Prefixes" || affixType == "Item Sufixes")
                {
                    if (tier.Name == affixTier.Name)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                        isItemTier = true;
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted(affixTier.DescriptionWithValues(affixDescription));
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(affixTier.Name);
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(affixTier.ItemLevel.ToString());
                ImGui.TableSetColumnIndex(3);
                ImGui.Text(affixTier.Weighting.ToString());
                ImGui.TableSetColumnIndex(4);
                ImGui.Text(affixTier.AffixPercent.ToString("0.##") + "%");

                if (isItemTier)
                {
                    ImGui.PopStyleColor();
                }
            }

            ImGui.TreePop();
        }
    }

    private void RenderLabelValue(string label, string value, Vector4? color = null)
    {
        if (string.IsNullOrEmpty(label) || value == null) return;

        try
        {
            var underscoredLabel = label.Replace(":", "");
            // ImGui.SetCursorPos(new Vector2(_padding, ImGui.GetCursorPosY() + _padding));

            // Label with underline
            var labelColor = color ?? new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
            ImGui.PushStyleColor(ImGuiCol.Text, labelColor);
            Vector2 startPos = ImGui.GetCursorScreenPos();
            ImGui.Text(label);
            Vector2 endPos = ImGui.GetCursorScreenPos();
            float textWidth = ImGui.CalcTextSize(underscoredLabel).X;

            // Draw underline
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(startPos.X, startPos.Y + ImGui.GetTextLineHeight() + 1),
                new Vector2(startPos.X + textWidth, startPos.Y + ImGui.GetTextLineHeight() + 1),
                ImGui.GetColorU32(labelColor)
            );
            ImGui.PopStyleColor();

            // Value
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            ImGui.Text(value);
            ImGui.PopStyleColor();
        }
        catch (Exception ex)
        {
            _logger.Debug($"Error in RenderLabelValue: {ex}");
        }
    }
}