using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
using System.Runtime.CompilerServices;

namespace citynames;
public static class LogUtils
{
    public static void InvokeWithMessage(this LoggableAction action, string initialMessage)
    {
        Console.Write($"{initialMessage}...");
        Console.Out.Flush();
        Console.WriteLine(action.Invoke());
    }
    public static void PrintAndWrite(string path, string s)
    {
        Console.WriteLine($"\t{s}");
        File.AppendAllText(path, $"{s}\n");
    }
    public static void PrintMethodArguments([CallerMemberName] string callerName = "", params (string name, object? value)[] arguments)
        => Console.WriteLine($"{callerName}{arguments.Select(x => $"{x.name}: {x.value}").ListNotation("(", ")")}");
    public static void PrintPreview(this IDataView dataView, int maxRows = 100)
    {
        DataDebuggerPreview preview = dataView.Preview(maxRows);
        Console.WriteLine($"{maxRows}\t{preview.ColumnView.Select(x => x.Column.Name).Aggregate((x, y) => $"{x}\t{y}")}");
        int ct = 0;
        foreach (DataDebuggerPreview.RowInfo row in preview.RowView)
        {
            Console.Write($"{ct++}");
            foreach (object value in row.Values.Select(x => x.Value))
            {
                if (value is IEnumerable enumerable)
                {
                    foreach (object item in enumerable)
                    {
                        Console.Write($"\t{item}");
                    }
                }
                else
                {
                    Console.Write($"\t{value}");
                }
            }
            Console.WriteLine();
            if (ct > maxRows)
                break;
        }
    }
    public static string ReadableString(this Type type)
    {
        string result = type.Name;
        if (type.IsGenericType)
            result += type.GenericTypeArguments.Select(x => x.ReadableString()).ListNotation("<", ">");
        return result;
    }
    public static string ReadableTypeString(this object? obj)
        => obj?.GetType().ReadableString()?.PrintNull()!;
}