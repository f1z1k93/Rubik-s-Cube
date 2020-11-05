﻿using UnityEngine;
using UnityEngine.Events;

public class PauseBotton : MonoBehaviour
{
    [SerializeField] private UnityEvent PauseEvent;
    [SerializeField] private UnityEvent ResumeEvent;

    private bool IsPause = false;

    private void Update()
    {
        if (IsPause)
        {
            Time.timeScale = 0f;
            PauseEvent.Invoke();
        } else
        {
            Time.timeScale = 1f;
            ResumeEvent.Invoke();
        }
    }

    public void OnPauseBottonClick()
    {
        IsPause = !IsPause;
    }
}
