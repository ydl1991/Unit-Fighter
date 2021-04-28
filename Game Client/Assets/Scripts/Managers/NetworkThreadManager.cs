using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkThreadManager : MonoBehaviour
{
    private static readonly List<Action> s_executeOnMainThread = new List<Action>();
    private static readonly List<Action> s_executeCopiedOnMainThread = new List<Action>();
    private static bool s_actionToExecuteOnMainThread = false;
    
    void Update()
    {
        UpdateMain();
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sets an action to be executed on the main thread.
    ///
    /// Param
    ///     action - The action to be executed on the main thread.
    /// 
    /// -----------------------------------------------------------------------------------------
    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (s_executeOnMainThread)
        {
            s_executeOnMainThread.Add(action);
            s_actionToExecuteOnMainThread = true;
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.
    /// 
    /// -----------------------------------------------------------------------------------------
    public static void UpdateMain()
    {
        if (s_actionToExecuteOnMainThread)
        {
            s_executeCopiedOnMainThread.Clear();
            lock (s_executeOnMainThread)
            {
                s_executeCopiedOnMainThread.AddRange(s_executeOnMainThread);
                s_executeOnMainThread.Clear();
                s_actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < s_executeCopiedOnMainThread.Count; i++)
            {
                s_executeCopiedOnMainThread[i]();
            }
        }
    }
}