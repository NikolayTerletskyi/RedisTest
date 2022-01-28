using Common;
using StackExchange.Redis;

using var redis = ConnectionMultiplexer.Connect("localhost:6379");
var channelStartupDelayMs = 300;
var channelMessageDelayMs = 100;
var channelsCount = 10000;
var messageText = "Privet, kak dela. Zachem ti nadel kepku. Neznau. No Zil Bil Pes horoshiy multick. Da, no zolushka luche. Ebanutii";
var channelPrefix = "channel";
var traceAfterMessagesCount = 100;

Console.WriteLine("Connection estabilished");

var startPoint = (int)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
int Now() => (int)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds - startPoint);
var startPointMessage = new SystemMessage(Now(), SystemMessageTypes.SetStartPoint, startPoint.ToString());

var subscriber = redis.GetSubscriber();
await subscriber.PublishAsync(SystemChannels.CommandChannel.ToString(), startPointMessage.ToString());
Console.WriteLine("Startpoint messege sent");

async Task StartChannel(int channelNumber)
{
    var channelName = $"{channelPrefix}{channelNumber}";
    var openChannelMessage = new SystemMessage(Now(), SystemMessageTypes.OpenChannel, $"channel{channelNumber}");
    await subscriber.PublishAsync(SystemChannels.CommandChannel.ToString(), openChannelMessage.ToString());

    Console.WriteLine($"{channelName} opened");

    await Task.Delay(channelStartupDelayMs);

    int count = 0;
    while (true)
    {
        var message = new Message(Now(), messageText);
        await subscriber.PublishAsync(channelName, message.ToString());
        await Task.Delay(channelMessageDelayMs);
        count++;
        if (count % traceAfterMessagesCount == 0)
        {
            Console.WriteLine($"channel{channelNumber} sent {count} messages");
        }
    }
}

var channelTasks = new List<Task>(channelsCount);
for(int i = 0; i < channelsCount; i++)
{
    channelTasks.Add(StartChannel(i + 1));
    await Task.Delay(10);
}

await Task.WhenAll(channelTasks);