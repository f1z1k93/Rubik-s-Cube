using System;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float TimerValue = 0f;
    private bool IsRunning = false;

    void Update()
    {
        if (IsRunning)
        {
            TimerValue += Time.deltaTime;
        }

        var tupleMinSecMillisec = ToMinSecMillisec(TimerValue);
        GetComponent<Text>().text = string.Format("{0:00}:{1:00}:{2:00}",
            tupleMinSecMillisec.Item1, tupleMinSecMillisec.Item2, tupleMinSecMillisec.Item3);
    }

    public void OnShuffleBottonDown()
    {
        ResetTimer();
        IsRunning = false;
    }

    public void OnShuffleBottonUp()
    {
        ResetTimer();
        IsRunning = true;
    }

    public void OnRubiksCubeSolved()
    {
        IsRunning = false;
    }

    private void ResetTimer()
    {
        TimerValue = 0f;
    }

    private Tuple<int, int, int> ToMinSecMillisec(float value)
    {
        int millisec = Mathf.FloorToInt(value * 100f);

        int sec = millisec / 100;
        millisec = millisec % 100;

        int min = sec / 60;
        sec = sec % 60;

        return Tuple.Create<int, int, int>(min, sec, millisec);
    }
}
