using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class IndentLogger
{
    private static List<LoggerNode> _nodes;

    private static readonly LogContainer Root = new();
    private static LogContainer _curContainer = Root;
    private static readonly Stack<LogContainer> PrevContainers = new();

    public static void LogHeader(string logMsg)
    {
        Root.children.Insert(0, new LogMsgHeader(logMsg));
    }

    public static void Log(string logMsg)
    {
        _curContainer.AddChild(new LogMsgBody(logMsg));
    }

    public static void Indent()
    {
        PrevContainers.Push(_curContainer);

        var newContainer = new LogContainer();
        _curContainer.AddChild(newContainer);
        _curContainer = newContainer;
    }

    public static void Unindent()
    {
        _curContainer = PrevContainers.TryPop(out var prevContainer) ? prevContainer : Root;
    }

    public static void PrintTo(string filePath, bool skipEmptyBody = true)
    {
        if (skipEmptyBody && !Root.children.Any(x => x is LogMsgBody or LogContainer))
            return;
        var writer = new StreamWriter(filePath);
        using (writer)
            foreach (var child in Root.children)
            {
                WriteRecursive(child, "", writer);
            }
    }


    public static void AppendAndPrintTo(string filePath, bool skipEmptyBody = true)
    {
        if (skipEmptyBody && !Root.children.Any(x => x is LogMsgBody or LogContainer))
            return;
        var writer = new StreamWriter(filePath, true);
        using (writer)
            foreach (var child in Root.children)
            {
                WriteRecursive(child, "", writer);
            }
    }

    private static void WriteRecursive(LoggerNode child, string padding, TextWriter writer)
    {
        if (child is LogMsg msg)
        {
            writer.WriteLine(padding + msg.text);
            return;
        }

        foreach (var grandChild in ((LogContainer)child).children)
        {
            WriteRecursive(grandChild, padding + "  ", writer);
        }
    }

    public static void Clear()
    {
        PrevContainers.Clear();
        Root.children.Clear();
        _curContainer = Root;
    }
}