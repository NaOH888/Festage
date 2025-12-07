using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ClapTimeStampInfo : IComparable<ClapTimeStampInfo>
{
    public int CompareTo(ClapTimeStampInfo other)
    {
        if (other == null) return 1;
        return judgeTimestamp_ms.CompareTo(other.judgeTimestamp_ms);
    }
    
    public bool IsPerfectHit(double clapTime_ms)
    {
        return Math.Abs(clapTime_ms - judgeTimestamp_ms) <= perfectThres_ms;
    }
    public bool TooEarly(double clapTime_ms)
    {
        return clapTime_ms < judgeTimestamp_ms - perfectThres_ms;
    }
    public bool TooLate(double clapTime_ms)
    {
        return clapTime_ms > judgeTimestamp_ms + perfectThres_ms;
    }

    public double judgeTimestamp_ms = 400;
    public double perfectThres_ms;
    public double showing_duration_ms;
}