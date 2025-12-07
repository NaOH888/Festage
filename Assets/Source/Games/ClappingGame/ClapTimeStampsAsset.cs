using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
[CreateAssetMenu(fileName = "ClapTimeStampsAsset", menuName = "ScriptableObjects/ClapTimeStampsAsset", order = 1)]
public class ClapTimeStampsAsset : ScriptableObject
{
    public string songName;
    public List<ClapTimeStampInfo> timeStamps;
    public VideoClip video;
    public AudioClip audio;

    public bool Check(bool require_stamp = false)
    {
        bool res = true;
        if(require_stamp) res &= (timeStamps != null && timeStamps.Count > 0);
        res &= (video != null);
        res &= (audio != null);
        return res;
    }
}
