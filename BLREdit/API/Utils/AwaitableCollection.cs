using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace BLREdit.API.Utils;

public sealed class AwaitableCollection<T> : System.IDisposable
{
    private readonly BlockingCollection<T> collection = [];
    private readonly ManualResetEvent emptyEvent = new(true);
    private readonly ManualResetEvent filledEvent = new(false);
    public void Add(T item)
    {
        emptyEvent.Reset();
        collection.Add(item);
        filledEvent.Set();
    }

    public T Take()
    {
        var temp = collection.Take();
        if (collection.Count <= 0) 
        {
            emptyEvent.Set();
            filledEvent.Reset();
        }
        return temp;
    }

    public void WaitForEmpty()
    {
        emptyEvent.WaitOne();
    }

    public void WaitForFill()
    {
        filledEvent.WaitOne();
    }

    public void Dispose()
    {
        collection.Dispose();
        emptyEvent?.Dispose();
        filledEvent?.Dispose();
    }
}
