using System;
using UnityEngine;

public class WaitTimer
{
    private Action timerAction;
    private float time;
    private float setTime;
    private bool isWaiting;

    public WaitTimer(float time, Action timerAction)
    {
        this.time = time;
        this.timerAction = timerAction;
    }

    public WaitTimer()
    {
        
    }

    /// <summary>
    /// Sets a timer with a specified duration and action to be executed when the timer expires.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="timerAction"></param>
    public void SetTimer(float time, Action timerAction)
    {
        this.time = time;
        this.setTime = time;
        this.timerAction = timerAction;
        isWaiting = true;
    }

    /// <summary>
    /// Updates the timer countdown. If the timer expires, the specified action is executed.
    /// </summary>
    public void UpdateTimer()
    {
        if (timerAction != null)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                isWaiting = false;
                Action waitAction = timerAction;
                timerAction = null;
                waitAction();
            }
        }
    }

    /// <summary>
    /// Adds additional time to the current timer.
    /// </summary>
    /// <param name="time"></param>
    public void AddTime(float time)
    {
        this.time += time;
    }

    /// <summary>
    /// Resets the timer to its initial state.
    /// </summary>
    public void ResetTimer()
    {
        time = 0;
        isWaiting = false;
        timerAction = null;
    }

    /// <summary>
    /// Checks if the timer is currently active.
    /// </summary>
    /// <returns></returns>
    public bool IsWaiting()
    {
        return isWaiting;
    }

    /// <summary>
    /// Returns the timer progress as a float value between 0 and 1, where 0 means the timer has just started and 1 means it has finished.
    /// </summary>
    /// <returns></returns>
    public float GetProgress()
    {
        if (setTime <= 0)
            return 0f;
        return Mathf.Clamp01((setTime - time) / setTime);
    }
}
