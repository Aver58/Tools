using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(VideoRoleControlAsset))]
public class VideoRoleControlTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<VideoRoleControlMixerBehaviour>.Create(graph, inputCount);
    }
}
