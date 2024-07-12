
using System;

public static class NotifyCenter<T1, T2, T3>
{

    public static event Action<T1, T2, T3> notifyCenter;
    public static void NotifyObservers(T1 eventType, T2 param2 = default, T3 param3 = default)
    {
        notifyCenter?.Invoke(eventType, param2, param3);
    }

}
