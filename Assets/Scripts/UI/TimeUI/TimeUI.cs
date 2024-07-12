using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public RectTransform dayNightImageTrans;
    public RectTransform clockParent;
    // public GameObject clockParentTest; //cannot get the count of the children

    public Image seasonImage;

    public TextMeshProUGUI dateText;

    public TextMeshProUGUI timeText;

    public Sprite[] seasonSprites;

    private List<GameObject> clocksBlocks = new List<GameObject>();
    private void Awake()
    {
        InitClock();
    }
    private void Start()
    {
        // Debug.Log(123);
    }
    private void InitClock()
    {
        for (int i = 0; i < clockParent.childCount; i++)
        {
            clocksBlocks.Add(clockParent.GetChild(i).gameObject);
            clockParent.GetChild(i).gameObject.SetActive(false); ;
        }
    }

    private void OnEnable()
    {
        // Debug.Log(444);
        NotifyCenter<UIEvent, bool, float>.notifyCenter += OnMinuteChange;
        NotifyCenter<UIEvent, bool, float>.notifyCenter += OnYearChange;
    }

    private void OnDisable()
    {
        NotifyCenter<UIEvent, bool, float>.notifyCenter -= OnMinuteChange;
        NotifyCenter<UIEvent, bool, float>.notifyCenter -= OnYearChange;

    }

    private void OnMinuteChange(UIEvent iEvent, bool arg2, float arg3)
    {


        // Debug.Log(time.gameHour + "a");
        if (iEvent == UIEvent.MinuteAndHour)
        {
            var gameHour = TimeManager.Instance.GetCurrentGameHour();
            var gameMinute = TimeManager.Instance.GetCurrentGameMinute();
            timeText.text = gameHour.ToString("00") + ":" + gameMinute.ToString("00");
        }
    }
    private void OnYearChange(UIEvent iEvent, bool arg2, float arg3)
    {
        // Debug.Log(time.gameDate + "b");


        if (iEvent == UIEvent.DayMonthYear)
        {
            var gameYear = TimeManager.Instance.GetCurrentGameYear();
            var gameMonth = TimeManager.Instance.GetCurrentGameMonth();
            var gameDay = TimeManager.Instance.GetCurrentGameDay();
            var gameSeason = TimeManager.Instance.GetCurrentSeason();
            var gameHour = TimeManager.Instance.GetCurrentGameHour();
            dateText.text = gameYear.ToString() + "/" + gameMonth.ToString("00") + "/" + gameDay.ToString("00");
            seasonImage.sprite = seasonSprites[(int)gameSeason];
            SwitchHourImage(gameHour);
            DayNightImageRotate(gameHour);
        }
    }

    private void SwitchHourImage(int hour)
    {
        int index = hour / 4;
        for (int i = 0; i < clocksBlocks.Count; i++)
        {
            clocksBlocks[i].SetActive(i < index);
        }
    }

    private void DayNightImageRotate(int hour)
    {
        var target = new Vector3(0, 0, hour * 15 - 90);
        dayNightImageTrans.DORotate(target, 1f, RotateMode.Fast);
    }
}
