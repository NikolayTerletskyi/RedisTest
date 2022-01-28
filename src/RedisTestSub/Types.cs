namespace RedisTestSub;

internal record Statistics
{
    private int count = 0;
    private int sum = 0;
    private DateTime startTime = DateTime.UtcNow;

    private object lockObj = new object();

    public int Count => count;
    public int Sum => sum;
    public int Min { get; private set; } = 0;
    public int Max { get; private set; } = 0;

    public TimeSpan WorkingTime => DateTime.UtcNow - startTime;

    public int AvgMessagesPerSecond => Count / (int)WorkingTime.TotalSeconds;

    public void Increment(int latency)
    {
        Interlocked.Add(ref sum, latency);
        Interlocked.Increment(ref count);
        if(latency < Min || Min == 0)
        {
            lock (lockObj)
            {
                if (latency < Min || Min == 0)
                {
                    Min = latency;
                }
            }
        }
        if (latency > Max || Max == 0)
        {
            lock (lockObj)
            {
                if (latency > Max || Max == 0)
                {
                    Max = latency;
                }
            }
        }
    }
}
