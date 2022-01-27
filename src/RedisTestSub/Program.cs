using StackExchange.Redis;

using var redis = ConnectionMultiplexer.Connect("localhost:6379");

Console.WriteLine("Connection estabilished");

var subscriber = redis.GetSubscriber();
subscriber.Subscribe("channel1", (_, value) =>
{
    Console.WriteLine($"I recieve {value}");
});

Console.WriteLine("Subscribed press any key to exit");

Console.ReadKey();