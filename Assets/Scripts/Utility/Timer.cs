using System.Diagnostics;

public class Timer
{
    private Stopwatch _stopWatch;

    public Timer()
    {
        _stopWatch = new Stopwatch();
    }

    public void Start()
    {
        if (_stopWatch.IsRunning)
        {
            _stopWatch.Stop();
        }

        _stopWatch.Reset();
        _stopWatch.Start();
    }

    public void Stop()
    {
        _stopWatch.Stop();
    }

    public void Print(string functionName)
    {
        UnityEngine.Debug.Log(functionName + ": " + _stopWatch.ElapsedMilliseconds + "ms");
    }
}
