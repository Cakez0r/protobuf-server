using System;
using UnityEngine;
using System.Collections.Generic;

public class UnityScheduler : MonoBehaviour
{
    private Queue<Action> m_workQueue = new Queue<Action>();

    private void Update()
    {
        lock (m_workQueue)
        {
            int itemsToRun = m_workQueue.Count;
            for (int i = 0; i < itemsToRun; i++)
            {
                m_workQueue.Dequeue()();
            }
        }
    }

    public void Schedule(Action a)
    {
        lock (m_workQueue)
        {
            m_workQueue.Enqueue(a);
        }
    }
}
