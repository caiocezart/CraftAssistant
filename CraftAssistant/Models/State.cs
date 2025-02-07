using Logger = ExileCore2CustomLogger.Logger;

namespace CraftAssistant;

public class State
{
    private Logger _logger;
    public bool Running { get; set; } = false;
    public List<Item> Items { get; private set; }
    public List<PoeDataBaseGroup> PoeData { get; set; }
    public Item CurrentItem { get; private set; }
    public String ItemSelected { get; set; }
    public List<string> ItemsSaved { get; set; }
    public StatusMessage Status { get; set; }
    public bool HasItems => Items.Any();
    public bool HasError { get; private set; }
    public string ErrorMessage { get; private set; }
    public string _archiveZipPath { get; set; }

    public State(Logger logger)
    {
        _logger = logger;
        Status = new StatusMessage();
        Items = new List<Item>();
    }

    public class UIState
    {
        public bool IsMainWindowOpen { get; set; }
        public bool EnableTextures { get; set; }
        public Guid CurrentItemTab { get; set; } = Guid.NewGuid();
        public bool IsPaused { get; set; }
        public ItemActionType Action { get; set; } = ItemActionType.None;
    }

    public UIState CurrentUIState { get; } = new();

    public void AddItem(Item item)
    {
        if (item == null) return;

        Items.Add(item);
        SetCurrentItem(item);
    }

    public void RemoveItem(Guid id)
    {
        Items.RemoveAll(i => i.Id == id);
        SetCurrentItem();
    }

    public void SetCurrentItem(Item item = null)
    {
        if (item == null)
        {
            CurrentItem = Items.FirstOrDefault();
            CurrentUIState.CurrentItemTab = CurrentItem?.Id ?? Guid.Empty;
            return;
        }

        CurrentItem = item;
        CurrentUIState.CurrentItemTab = item.Id;
    }

    public void UpdateSavedItems(List<string> items)
    {
        ItemsSaved = items;
        ItemSelected = ItemsSaved.Contains(ItemSelected) ? ItemSelected : null;
    }

    public void ClearItems()
    {
        Items.Clear();
        SetCurrentItem();
    }

    public void SetError(Exception ex)
    {
        HasError = true;
        ErrorMessage = ex.Message;
    }

    public void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    public void SetStatus(StatusMessage status)
    {
        Status = status;
    }

}


public enum ItemActionType
{
    None,
    OverwriteItem,
    DeleteItem,
    ClearItems
}