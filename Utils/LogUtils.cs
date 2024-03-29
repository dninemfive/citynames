using d9.utl;
using System.Collections;
using System.Runtime.CompilerServices;

namespace citynames;
public static class LogUtils
{
    public static string MethodArguments([CallerMemberName] string callerName = "", params (string name, object? value)[] arguments)
        => $"{callerName}{arguments.Select(x => $"{x.name}: {x.value}").ListNotation(brackets: ("(", ")"))}";
    public static string NaturalLanguageList(this IEnumerable<object> objects, string conjunction = "or")
        => objects.Count() switch
        {
            0 => "",
            1 => $"{objects.First()}",
            2 => $"{objects.First()} {conjunction} {objects.Last()}",
            _ => $"{objects.SkipLast(1).ListNotation(brackets: null)}, {conjunction} {objects.Last()}"
        };
    public static string ReadableString(this Type type)
    {
        string result = type.Name.Split('`').First();
        if (type.IsGenericType)
            result += type.GenericTypeArguments.Select(x => x.ReadableString()).ListNotation("<", ">");
        return result;
    }
    public static string ReadableTypeString(this object? obj)
        => obj?.GetType().ReadableString()?.PrintNull()!;
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
}