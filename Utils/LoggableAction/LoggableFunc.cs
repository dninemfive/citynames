using System.Diagnostics.CodeAnalysis;

namespace citynames;

/// <summary>
/// Represents an action which returns an <see cref="ActionResult"/>.
/// </summary>
/// <returns>The result of the specified action.</returns>
public delegate ActionResult LoggableFuncDelegate<T>([NotNullWhen(true)]out T? result);
/// <summary>
/// Wrapper for an action which produces a result to be logged.
/// </summary>
/// <param name="delegate">The action to be invoked.</param>
public class LoggableFunc<T>(LoggableFuncDelegate<T> @delegate)
{
    public ActionResult Invoke([NotNullWhen(true)]out T? result) => @delegate(out result);
    /// <summary>
    /// Logs the initial message and then the result of invoking this action in the following format:
    /// <para><paramref name="initialMessage"/>...&lt;result of invocation&gt;</para>
    /// </summary>
    /// <param name="initialMessage">The message to print before invoking this action.</param>
    public T? InvokeWithMessage(string initialMessage)
    {
        Console.WriteLine($"{initialMessage}...{Invoke(out T? result)}");
        return result;
    }
    public static implicit operator LoggableFunc<T>(LoggableFuncDelegate<T> @delegate)
        => new(@delegate);
    public static implicit operator LoggableFuncDelegate<T>(LoggableFunc<T> lf)
        => lf.Invoke;
}