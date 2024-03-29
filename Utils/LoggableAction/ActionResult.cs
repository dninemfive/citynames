namespace citynames;
/// <summary>
/// Represents the result of a <see cref="LoggableAction"/>, with convenient shorthand for the
/// pattern of returning <see langword="true"/> if the action was successful or just an error
/// message otherwise.
/// </summary>
/// <param name="success">
/// <see langword="true"/> if the action was successful or <see langword="false"/> otherwise.
/// </param>
/// <param name="message">An error message to print if the action was not successful.</param>
public class ActionResult(bool success, string? message = null)
{
    public override string ToString()
        => $"{(success ? "Success" : "Failure")}{(!(success || message is null) ? $": {message}" : "!")}";
    /// <summary>
    /// Implicitly converts <paramref name="b"/> to an <see cref="ActionResult"/> with no message.
    /// </summary>
    /// <param name="b">Whether the action was successful.</param>
    public static implicit operator ActionResult(bool b)
        => new(b);
    /// <summary>
    /// Implicitly converts <paramref name="msg"/> to an <see cref="ActionResult"/> representing
    /// a failure with <paramref name="msg"/> as the failure message.
    /// </summary>
    /// <param name="msg">The failure message.</param>
    public static implicit operator ActionResult(string msg)
        => new(false, msg);
}