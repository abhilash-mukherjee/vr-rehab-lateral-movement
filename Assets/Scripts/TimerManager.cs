﻿using UnityEngine;
using System.Collections;
using System;

public class TimerManager : MonoBehaviour
{
    public delegate void TimerEndHandler(SessionData data);
    public static event TimerEndHandler OnTimerEnd;

    [SerializeField]
    private GameConfig gameConfig;

    private float _timeLeft = 0;

    private SessionData _currentSessionData;

    private bool _isRunning = false;

    private void OnEnable()
    {
        SessionManager.OnNewSessionCreated += NewSessionCreated;
        SessionManager.OnSessionPaused += SessionPaused;
        SessionManager.OnSessionResumed += SessionResumed;
        SessionManager.OnSessionEnded += SessionEnded;
    }

    private void OnDisable()
    {
        SessionManager.OnNewSessionCreated -= NewSessionCreated;
        SessionManager.OnSessionPaused -= SessionPaused;
        SessionManager.OnSessionResumed -= SessionResumed;
        SessionManager.OnSessionEnded -= SessionEnded;
    }

    private void SessionEnded(SessionData data)
    {
        _currentSessionData = null;
        StopAllCoroutines();
        ResetTimer();
    }

    private void SessionPaused(SessionData data)
    {
        if (data.code != _currentSessionData.code)
        {
            Debug.Log("A new session is paused while old timer running");
            _currentSessionData = data;
            _timeLeft = data.duration;
        }
        StopAllCoroutines();
        PauseTimer();
    }

    private void SessionResumed(SessionData data)
    {
        if (data.code != _currentSessionData.code)
        {
            Debug.Log("A new session is trying to be resumed while old timer running");
            _currentSessionData = data;
            _timeLeft = data.duration;
        }
        StopAllCoroutines();
        ResumeTimer();
    }

    private void NewSessionCreated(SessionData data)
    {
        _currentSessionData = data;
        _timeLeft = data.duration;
        StopAllCoroutines();
        StartCoroutine(StartTimer(data.duration));
    }

    IEnumerator StartTimer(int sessionDuration)
    {
        yield return new WaitForSeconds(gameConfig.SessionStartDelay);
        ResumeTimer();
    }

    private void TickTimer()
    {
        _timeLeft -= 1;
        if (_timeLeft <= 0)
        {
            _timeLeft = 0;
            PauseTimer();
            OnTimerEnd?.Invoke(_currentSessionData);
        }
    }

    private void PauseTimer()
    {
        _isRunning = false;
        Debug.Log("Timer paused");
        CancelInvoke(nameof(TickTimer));
    }
    private void ResetTimer()
    {
        _isRunning = false;
        _timeLeft = 0;
        Debug.Log("Timer reset");
        CancelInvoke(nameof(TickTimer));
    }
    
    private void ResumeTimer()
    {
        if (_isRunning) return;
        _isRunning = true;
        Debug.Log("Timer Resumed");
        InvokeRepeating(nameof(TickTimer), 1, 1);
    }
}