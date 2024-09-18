using _Project.Data.Persistent;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BonusView : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText;

    private DateTime _timeOfEnd;

    public void Init(DateTime timeOfEnd)
    {
        gameObject.SetActive(true);
        _timeOfEnd = timeOfEnd;
        UpdateTimer();
    }

    private void FixedUpdate()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        var timerRemainingHours = (_timeOfEnd - DateTime.Now).Hours;
        var timerRemainingMinutes = (_timeOfEnd - DateTime.Now).Minutes;
        var timerRemainingSeconds = (_timeOfEnd - DateTime.Now).Seconds;

        if (timerRemainingHours <= 0 && timerRemainingMinutes <= 0 && timerRemainingSeconds <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            if (timerRemainingHours > 0)
                _timeText.text = String.Format("{0:00}h {1:00}m", timerRemainingHours, timerRemainingMinutes);
            else if (timerRemainingMinutes > 0)
                _timeText.text = String.Format("{0:00}m {1:00}s", timerRemainingMinutes, timerRemainingSeconds);
            else if (timerRemainingSeconds > 0)
                _timeText.text = String.Format("{0:00}s", timerRemainingSeconds);
        }
    }
}
