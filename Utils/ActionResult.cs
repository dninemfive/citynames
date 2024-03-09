namespace citynames;
public class ActionResult(bool success, string? message = null)
{
    public override string ToString()
        => $"{(success ? "Success" : "Failure")}{(!(success || message is null) ? $": {message}" : "!")}";
    public static implicit operator ActionResult(bool b)
        => new(b);
    public static implicit operator ActionResult(string msg)
        => new(false, msg);
}