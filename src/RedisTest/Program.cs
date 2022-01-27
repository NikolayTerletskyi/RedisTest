using StackExchange.Redis;

using var redis = ConnectionMultiplexer.Connect("localhost:6379");

Console.WriteLine("Connection estabilished");

var subscriber = redis.GetSubscriber();
subscriber.PublishAsync("channel1", "hello");

Console.WriteLine("Hello sended");
