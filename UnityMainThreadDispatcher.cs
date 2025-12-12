using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private static bool initialized = false;

    private Queue<Action> executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (!initialized)
        {
            if (!instance)
            {
                instance = new GameObject("MainThreadDispatcher")
                    .AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(instance.gameObject);
            }
            initialized = true;
        }
        return instance;
    }

    public void Enqueue(Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        if (executionQueue.Count > 0)
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}