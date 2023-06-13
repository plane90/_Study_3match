using System.Collections.Generic;

public abstract class LoggerNode
{
}

public abstract class LogMsg : LoggerNode
{
    public readonly string text;

    protected LogMsg(string text)
    {
        this.text = text;
    }
}

public class LogMsgHeader : LogMsg
{
    public LogMsgHeader(string text) : base(text)
    {
    }
}

public class LogMsgBody : LogMsg
{
    public LogMsgBody(string text) : base(text)
    {
    }
}

public class LogContainer : LoggerNode
{
    public readonly List<LoggerNode> children = new();
    
    public void AddChild(LoggerNode child)
    {
        children.Add(child);
    }
}