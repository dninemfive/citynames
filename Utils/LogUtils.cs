﻿using d9.utl;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace citynames;
/// <summary>
/// Utilities which make logging objects and method calls more convenient.
/// </summary>
public static class LogUtils
{
    /// <summary>
    /// Prints the name of the calling method, as well as the specified arguments, which should but
    /// are not required to correspond to the arguments of the calling method.
    /// </summary>
    /// <param name="callerName">The name of the calling method. This is automatically set by 
    /// compiler services.</param>
    /// <param name="args">The arguments of the method, as described above.</param>
    /// <returns>A string which summarizes the method call, as described above.</returns>
    public static string Method([CallerMemberName] string callerName = "", params (string name, object? value)[] args)
        => $"{callerName}{args.Select(x => $"{x.name}: {x.value.Summary()}").ListNotation(brackets: ("(", ")"))}";
    /// <summary>
    /// Prints the specified items as a list in natural English, with the specified conjunction.
    /// </summary>
    /// <param name="items">The items to print.</param>
    /// <param name="conjunction">The conjunction at the end of the string, just before the last
    /// item.</param>
    /// <returns>The items in the list separated by commas as appropriate, with a conjunction
    ///          between the last two items.</returns>
    /// <remarks>Uses the Oxford comma, which is the correct way to write such lists.</remarks>
    public static string NaturalLanguageList<T>(this IEnumerable<T> items, string conjunction = "or")
        => items.Count() switch
        {
            0 => "",
            1 => $"{items.First()}",
            2 => $"{items.First()} {conjunction} {items.Last()}",
            _ => $"{items.SkipLast(1).ListNotation(brackets: null)}, {conjunction} {items.Last()}"
        };
    /// <summary>
    /// Produces a string which describes the specified type as it would be written in code.
    /// </summary>
    /// <param name="type">The type to summarize.</param>
    /// <returns>The type and its generic arguments, separated with angle brackets and commas as
    ///          appropriate.</returns>
    public static string ReadableString(this Type type)
    {
        string result = type.Name.Split('`').First();
        if (type.IsGenericType)
            result += type.GenericTypeArguments.Select(x => x.ReadableString()).ListNotation("<", ">");
        return result;
    }
    /// <summary>
    /// Produces a <see cref="ReadableString(Type)"/> of the type of the specified object, if it
    /// is not <see langword="null"/>.
    /// </summary>
    /// <param name="obj">The object whose type to summarize.</param>
    /// <returns>The readable string as described above, or <see cref="Constants.NullString"/>
    ///          if the object is <see langword="null"/>.</returns>
    public static string ReadableTypeString(this object? obj)
        => obj?.GetType().ReadableString()?.PrintNull()!;
    /// <summary>
    /// Produces a short summary of the specified object.
    /// </summary>
    /// <param name="obj">The object to summarize.</param>
    /// <returns>If the object is a <see langword="string"/>, the string in quotes; if it is an
    ///          enumerable, the type of the enumerable and the count of objects it contains.
    ///          Otherwise, <c>
    ///          <paramref name="obj"/>.<see cref="StringUtils.PrintNull(object?, string)">PrintNull</see>()</c>.
    ///          </returns>
    public static string Summary(this object? obj)
    {
        if (obj is string s)
        {
            return $"\"{s}\"";
        }
        else if (obj is IEnumerable enumerable)
        {
            int ct = 0;
            foreach (object? _ in enumerable)
                ct++;
            return $"{enumerable.ReadableTypeString()}({ct})";
        }
        else
        {
            return obj.PrintNull();
        }
    }
    public static T LogInvocation<T>(string initialMessage, Func<T> function, string finalMessage = "Done!")
    {
        Console.Write($"{initialMessage}...");
        T result = function();
        Console.WriteLine(finalMessage);
        return result;
    }
    public static T LogAndTime<T>(string initialMessage, Func<T> function, string finalMessage = "Done! Time elapsed: ")
    {
        Stopwatch stopwatch = new();
        Console.Write($"{initialMessage}...");
        stopwatch.Start();
        T result = function();
        stopwatch.Stop();
        Console.WriteLine($"{finalMessage}{stopwatch.Elapsed:g}");
        return result;
    }
}