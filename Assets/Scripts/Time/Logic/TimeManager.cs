using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TimeManager : Singleton<TimeManager>
{
    private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;
    private Season gameSeason = Season.Spring;

    private int monthInSeason = 3;

    public bool gameClockPause;
    private float tikTime;

    public TimeSpan GameTime => new TimeSpan(gameHour, gameMinute, gameSecond);

    protected override void Awake()
    {
        base.Awake();
        InitTime();
    }
    // private void Awake()
    // {
    //     InitTime();
    // }

    private void Start()
    {
        NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.DayMonthYear, true);

        NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.MinuteAndHour, true);
    }
    private void Update()
    {
        if (!gameClockPause)
        {
            tikTime += Time.deltaTime;
            //based on the secondThreshold, we turn one real second into secondThreshold value.
            if (tikTime >= Settings.secondThreshold)
            {
                tikTime -= Settings.secondThreshold;
                UpdateGameTime();
            }
        }

        //cheating
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     // for (int i = 0; i < 60; ++i)
        //     // {
        //     //     UpdateGameTime(); //相当于1分钟
        //     // }
        //     gameSeason++;
        //     if (gameSeason > Season.Winter)
        //     {
        //         gameSeason = Season.Spring;
        //     }
        //     NotifyCenter<UIEvent, DayMonthYear, float>.NotifyObservers(UIEvent.DayMonthYear, new DayMonthYear(gameHour, gameDay, gameMonth, gameYear, gameSeason));
        // }
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            NotifyCenter<SceneEvent, int, Season>.NotifyObservers(SceneEvent.UpdateOneDay, gameDay, gameSeason);
            // NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.DayMonthYear, true);
        }
    }

    private void InitTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 2022;
        gameSeason = Season.Spring;
    }


    //相当于游戏时间的1秒的更新，真实时间是setting里的时间
    private void UpdateGameTime()
    {
        gameSecond++;
        if (gameSecond >= Settings.sixtyHold)
        {
            gameMinute++;
            gameSecond = 0;
            if (gameMinute >= Settings.sixtyHold)
            {
                gameHour++;
                gameMinute = 0;
                if (gameHour >= Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;
                    if (gameDay >= Settings.dayHold)
                    {
                        gameDay = 1;
                        gameMonth++;
                        monthInSeason--;
                        if (monthInSeason == 0)
                        {
                            monthInSeason = 3;
                            gameSeason++;
                            if (gameSeason > Season.Winter)
                            {
                                gameSeason = Season.Spring;
                            }
                        }
                        if (gameMonth >= Settings.monthHold)
                        {
                            gameYear++;
                            gameMonth = 1;
                            if (gameYear >= 9999) { gameYear = 2022; }
                        }
                    }
                    NotifyCenter<SceneEvent, int, Season>.NotifyObservers(SceneEvent.UpdateOneDay, gameDay, gameSeason);
                }
                NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.DayMonthYear, true);
            }
            NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.MinuteAndHour, true);
        }
    }

    // private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;
    public int GetCurrentGameMinute()
    {
        return gameMinute;
    }
    public int GetCurrentGameDay()
    {
        return gameDay;
    }

    public int GetCurrentGameHour()
    {
        return gameHour;
    }
    public int GetCurrentGameMonth()
    {
        return gameMonth;
    }
    public Season GetCurrentSeason()
    {
        return gameSeason;
    }

    public int GetCurrentGameYear()
    {
        return gameYear;
    }
}
