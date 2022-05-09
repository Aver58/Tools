using System;
using Scenes.SS5VideoDemo.Script;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

public class VideoRoleControlMixerBehaviour : PlayableBehaviour {
    private GameObject roleGo;
    private Animator animator;
    // private CommonAnimatorControl commonAnimatorControl;
    private AnimatorOverrideController animatorOverrideController;
    private bool isPlayingAction = false;
    private string[] effectSigns;
    private bool isInit = false;
    private bool isStartPlay = false;
    private PlayableDirector playableDirector;
    private TimelineClip timelineClip;

    #region PlayableBehaviour

    // 相当于 Update 帧
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        if (!Application.isPlaying) {
            return;
        }
        
        var inputPlayable = (ScriptPlayable<VideoRoleControlBehaviour>)playable.GetInput(0);
        var input = inputPlayable.GetBehaviour();
        
        // Init
        if (isInit == false) {
            var suitId = input.suitID;
            effectSigns = input.effectSigns;

            isPlayingAction = false;
            roleGo = ModelManager.Instance.GetModel(suitId);
            animatorOverrideController = ModelManager.Instance.GetAnimatorController(suitId);
            if (roleGo) {
                animator = roleGo.GetComponent<Animator>();
                // commonAnimatorControl = roleGo.GetComponent<CommonAnimatorControl>();
                roleGo.SetActive(false);
            } else {
                Debug.Log("【Model】没有取到指定id的模型！" + suitId);
            }

            if (playableDirector != null) {
                var timelineAsset = playableDirector.playableAsset as TimelineAsset;
                if (timelineAsset != null) {
                    foreach (var trackAsset in timelineAsset.GetRootTracks()) {
                        foreach (var clip in trackAsset.GetClips()) {
                            var videoRoleControlAsset = clip.asset as VideoRoleControlAsset;
                            if (videoRoleControlAsset != null && videoRoleControlAsset.template.suitID == suitId) {
                                timelineClip = clip;
                                break;
                            }
                        }
                    }
                }
            }
            
            isInit = true;
        }
        
        // return
        var totalTime = playable.GetTime();
        Debug.LogError($"{roleGo.name} {input.startTime} {input.duration}");
        if (totalTime < input.startTime || totalTime > input.startTime + input.duration) {
            isStartPlay = false;
            HideModel();
            return;
        }

        // OnStart
        if (!isStartPlay) {
            ShowModel();
            if (roleGo) {
                animator.runtimeAnimatorController = animatorOverrideController;
            }

            isStartPlay = true;
        }
        
        // Update
        var curClipTime = totalTime - input.startTime;
        if (roleGo != null) {
            roleGo.transform.position = Vector3.Lerp(input.startPos, input.endPos, (float)(curClipTime / input.duration));

            if (input.actionSigns != null) {
                for (var i = 0; i < input.actionSigns.Length; i++) {
                    var actionSign = input.actionSigns[i];
                    var actionStartTime = input.actionStartTimes[i];
                    if (!isPlayingAction && Math.Abs(totalTime - actionStartTime) < 0.0167f) {
                        isPlayingAction = true;
                        Debug.Log("Play Action：" + actionSign);
                        if (Application.isPlaying) {
                            PlayAction(i);
                        }
                    }

                    var actionEndTime = actionStartTime + input.actionDurations[i];
                    if (isPlayingAction && Math.Abs(totalTime - actionEndTime) < 0.0167f) {
                        isPlayingAction = false;
                        Debug.Log("Stop Action：" + actionSign);
                        // commonAnimatorControl.Stop();
                        StopAction(i);
                    }
                }
            }
        }
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        playableDirector = (playable.GetGraph().GetResolver() as PlayableDirector);
    }

    #endregion

    // 卸载模型
    private void HideModel() {
        if (roleGo) {
            roleGo.SetActive(false);
        }
    }

    private void ShowModel() {
        if (roleGo) {
            roleGo.SetActive(true);
        }
    }
    
    private void StopAction(int i) {
        var actionIndex = i + 1;
        animator.SetTrigger("action"+ actionIndex + "Back");
    }

    private void PlayAction(int i) {
        if (roleGo) {
            var actionIndex = i + 1;
            animator.Update(0);
            animator.SetTrigger("action"+ actionIndex);

            // commonAnimatorControl.Stop();
            var effectSign = effectSigns[i];
            LoadEffect(effectSign);
        }
    }

    private void LoadEffect(string effectId) {
        // if (effectId == "0") {
        //     Debug.Log("【VideoRoleControlAsset】沒有配置effectId，" + effectId);
        //     return;
        // }
        // Debug.Log("LoadEffect：" + effectId);
        // var effect = EffectPool.Get(effectId);
        // if (effect == null) {
        //     return;
        // }
        // var effectControl = effect.GetComponent<EffectControl>();
        // if (effectControl == null) {
        //     Debug.LogErrorFormat("检查对应 {0} 特效是否没有添加EffectControl脚本", effectId);
        //     EffectPool.Release(effectId, effect);
        //     return;
        // }
        //
        // effectControl.Init(commonAnimatorControl);
        // if (!commonAnimatorControl.effectControls.ContainsKey(effectId)) {
        //     commonAnimatorControl.effectControls.Add(effectId, effectControl);
        // }
        //
        // Transform mParent = null;
        // if (effectControl.parentName == "NotRotating") {
        //     mParent = roleGo.transform;
        // } else {
        //     if (commonAnimatorControl.skinManager != null) {
        //         mParent = commonAnimatorControl.skinManager.meshCombiner.GetRootBoone(effectControl.parentName);
        //     }
        // }
        //
        // if (mParent == null) {
        //     mParent = roleGo.transform;
        // }
        //
        // effect.transform.SetParentEx(mParent, Vector3.zero, Quaternion.identity, Vector3.one);
        // effect.SetLayer(LayerUtil.UILayer, true);
    }
}