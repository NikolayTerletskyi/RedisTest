using StackExchange.Redis;
using System.Globalization;
using Common;
using RedisTestSub;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using System.Text;

using var redis = ConnectionMultiplexer.Connect("localhost:6379");
var subscriber = redis.GetSubscriber();

Console.WriteLine("Connection estabilished");

var statisticsGlobal = new Statistics();
int startPoint = 0;

void Measure(Message message, DateTime recieveTime)
{
    if(startPoint <= 0)
    {
        return;
    }
    var recieveTimeInTicks = (int)(recieveTime.TimeOfDay.TotalMilliseconds - startPoint);
    var latency = recieveTimeInTicks - message.SendTime;
    statisticsGlobal.Increment(latency);
}

void StartPointHandler  (string payload)
{
    startPoint = int.Parse(payload);
    Console.WriteLine($"Startpoint set to {startPoint}");
}

void OpenChannelHandler(string payload)
{
    var messageCount = 0;
    var channelName = payload;
    subscriber.Subscribe(channelName, (_, messageStr) =>
    {
        var recieveTime = DateTime.UtcNow;
        var message = Message.ParseMessage(messageStr);
        Measure(message, recieveTime);
        messageCount++;
        if (messageCount % 100 == 0)
        {
            Console.WriteLine($"{channelName} recieve {messageCount} messages");
        }
    });    
    Console.WriteLine($"Subscribed to {payload}");
}

void HandleSystemMessage(SystemMessage message)
{
    switch (message.Type)
    {
        case SystemMessageTypes.SetStartPoint:
            StartPointHandler (message.Payload);
            break;
        case SystemMessageTypes.OpenChannel:
            OpenChannelHandler(message.Payload);
            break;
    }
}

subscriber.Subscribe(SystemChannels.CommandChannel.ToString(), (_, value) =>
{
    var message = Message.ParseMessage(value);
    var systemMessage = new SystemMessage(message);
    HandleSystemMessage(systemMessage);
});

Console.WriteLine("Subscribed press any key to exit");

Console.ReadKey();

var reportBuilder = new StringBuilder();
reportBuilder.AppendLine($"Total message count: {statisticsGlobal.Count}");
reportBuilder.AppendLine($"Average latency: {statisticsGlobal.Sum / statisticsGlobal.Count}");
reportBuilder.AppendLine($"Min latency: {statisticsGlobal.Min}");
reportBuilder.AppendLine($"Max latency: {statisticsGlobal.Max}");
reportBuilder.AppendLine($"Working time: {statisticsGlobal.WorkingTime}");
reportBuilder.AppendLine($"Avg messages per sec: {statisticsGlobal.AvgMessagesPerSecond}");

var report = reportBuilder.ToString();

Console.WriteLine(report);