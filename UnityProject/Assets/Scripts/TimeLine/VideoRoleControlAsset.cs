using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class VideoRoleControlAsset : PlayableAsset {
    public VideoRoleControlBehaviour template;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var playable = ScriptPlayable<VideoRoleControlBehaviour>.Create(graph, template);

        return playable;   
    }
}