using System.Globalization;

namespace Common;

public enum SystemChannels
{
    CommandChannel
}

public enum SystemMessageTypes
{
    OpenChannel,
    CloseChannel,
    SetStartPoint
}

public record Message(int SendTime, string Payload)
{
    public static Message ParseMessage(string message)
    {
        var parts = message.Split(',');
        return new Message(int.Parse(parts[0]), string.Join(',', parts.Where((_, i) => i != 0)));
    }

    public override string ToString()
    {
        return $"{SendTime},{Payload}";
    }
}

public record SystemMessage(int SendTime, SystemMessageTypes Type, string Payload)
{
    private static (SystemMessageTypes Type, string Payload) ParsePayload(string payload)
    {
        var parts = payload.Split(',');
        var type = Enum.Parse<SystemMessageTypes>(parts[0]);
        return (type, parts[1]);
    }

    public SystemMessage(Message message)
        : this(message.SendTime,
              ParsePayload(message.Payload).Type, 
              ParsePayload(message.Payload).Payload) { }

    public override string ToString()
    {
        return $"{SendTime},{Type},{Payload}";
    }
}
