namespace CraftAssistant;

public class StatusMessage
{
    public StatusCode Code { get; set; } = StatusCode.Success;
    public string Message { get; set; }
    public bool Show { get; set; } = false;

    public StatusMessage(string message = "", StatusCode code = StatusCode.Success)
    {
        Message = message;
        Code = code;
        Show = !string.IsNullOrEmpty(message);
    }
}

public enum StatusCode
{
    Success,
    Warning,
    CraftingError,
    InternalError
}