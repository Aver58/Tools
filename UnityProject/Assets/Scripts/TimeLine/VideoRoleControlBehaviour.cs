using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class VideoRoleControlBehaviour : PlayableBehaviour {
    public string suitID;
    public Vector3 startPos;
    public Vector3 endPos;
    public float startTime;
    public float duration;

    public float[] actionStartTimes;    // 动作开始时间
    public float[] actionDurations;    // 动作持续时间
    public string[] effectSigns;    // 确认一下是否和动作同时开始
    public string[] actionSigns;    // 外围动作标识，用item直接播放
    public string defaultActionSign;    // 默认动作
}