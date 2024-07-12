using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;

    public int priority;

    public Season season;

    public string targetSceneName;
    public Vector2Int targetGridPosition;
    public AnimationClip clipAnimationStop; //当角色停下来的时候，会不会播一些动画

    public bool isInteractable;
    public int GameTime => (hour * 100) + minute;

    public ScheduleDetails(int hour, int minute, int day, int priority, Season season, string targetSceneName, Vector2Int targetGridPosition, AnimationClip clipAnimationStop, bool isInteractable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.season = season;
        this.targetSceneName = targetSceneName;
        this.targetGridPosition = targetGridPosition;
        this.clipAnimationStop = clipAnimationStop;
        this.isInteractable = isInteractable;
    }

    public int CompareTo(ScheduleDetails other)
    {
        if (GameTime == other.GameTime)
        {
            if (priority > other.priority)
            {
                return 1;
            }
            else return -1;
        }
        else if (GameTime > other.GameTime)
        {
            return 1;
        }
        else if (GameTime < other.GameTime)
        {
            return -1;
        }
        return 0;
    }
}
