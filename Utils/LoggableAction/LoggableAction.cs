namespace citynames;

/// <summary>
/// Represents an action which returns an <see cref="ActionResult"/>.
/// </summary>
/// <returns>The result of the specified action.</returns>
public delegate ActionResult LoggableActionDelegate();
/// <summary>
/// Wrapper for an action which produces a result to be logged.
/// </summary>
/// <param name="delegate">The action to be invoked.</param>
public class LoggableAction(LoggableActionDelegate @delegate)
{
    public ActionResult Invoke() => @delegate();
    public void InvokeWithMessage(string initialMessage)
        => Console.WriteLine($"{initialMessage}...{Invoke()}");
    public static implicit operator LoggableAction(LoggableActionDelegate @delegate)
        => new(@delegate);
    public static implicit operator LoggableActionDelegate(LoggableAction la)
        => la.Invoke;
}