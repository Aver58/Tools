using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Scenes.SS5VideoDemo.Script {
    public class TimelineManager : MonoBehaviour {
        public bool isShowButton;
        private string goPrefix = "Suit";
        private string pathPrefix = "Path";
        private static PlayableDirector playableDirector;
        private bool isPlay = false;

        private void Start() {
            // 先全量GC一下
            System.GC.Collect();
            Resources.UnloadUnusedAssets();

            ModelManager.Instance.Init(this);

            ReBindingAllFashionByConfig();

            isPlay = false;

            ModelManager.Instance.StartCoroutine();
            // ModelManager.Instance.LoadAllModelInThread();
        }

        private void Update() {
            ModelManager.Instance.OnUpdate();
        }

        private void OnGUI() {
            if (isShowButton && GUILayout.Button("DeserializeConfig")) {
                DeserializeConfig();
            }
        }

        private void ReBindingAllFashionByConfig() {
            var timeLine = GameObject.Find("TimeLine");
            if (null == timeLine) {
                Debug.LogError("【TimelineManager】没有找到 TimeLine GameObject！");
                return;
            }
        
            playableDirector = timeLine.GetComponent<PlayableDirector>();
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            if (timelineAsset != null) {
                // 清除所有轨道绑定
                foreach (var item in playableDirector.playableAsset.outputs) {
                    var key = item.sourceObject as TrackAsset;
                    var value = playableDirector.GetGenericBinding(key);
                    // Debug.Log($"删除key {key} value {value}");
                    timelineAsset.DeleteTrack(key);
                }
                
                // 读取 VideoRoleConfig 生成规定绑定
                foreach (var suitId in VideoRoleConfigConfig.GetKeys()) {
                    var config = VideoRoleConfigConfig.Get(suitId);
                    if (config != null) {
                        //创建timeline一条一条的滑轨条
                        var track = timelineAsset.CreateTrack<VideoRoleControlTrack>(null, suitId);
                        //设置时间轨道上的clips 设置开始时间和持续时间
                        var clip = track.CreateDefaultClip();
                        var videoRoleControlAsset = clip.asset as VideoRoleControlAsset;
                        var videoRoleControlBehaviour = videoRoleControlAsset.template;
                        if (videoRoleControlAsset.template == null) {
                            videoRoleControlAsset.template = new VideoRoleControlBehaviour();
                            videoRoleControlBehaviour = videoRoleControlAsset.template;
                        }
                        if (videoRoleControlBehaviour != null) {
                            clip.start = config.startTime;
                            clip.duration = config.duration;
                            videoRoleControlBehaviour.suitID = config.id;
                            videoRoleControlBehaviour.startPos = config.startPos;
                            videoRoleControlBehaviour.endPos = config.endPos;
                            videoRoleControlBehaviour.startTime = config.startTime;
                            videoRoleControlBehaviour.duration = config.duration;
                            videoRoleControlBehaviour.actionStartTimes = config.actionStartTimes;
                            videoRoleControlBehaviour.actionDurations = config.actionDurings;
                            videoRoleControlBehaviour.effectSigns = config.effectSigns;
                            videoRoleControlBehaviour.actionSigns = config.actionSigns;
                            videoRoleControlBehaviour.defaultActionSign = config.defaultActionSign;
                        }

                        playableDirector.SetGenericBinding(videoRoleControlAsset, null);
                    }

                    ModelManager.Instance.AddLoadModelRequest(suitId);
                }
            }
        }

        List<string> tempList = new List<string>();
        private string DeserializeStringArray(string[] array) {
            tempList.Clear();
            var output = string.Empty;
            if (array.Length > 0) {
                foreach (var sign in array) {
                    tempList.Add(sign);
                }
                output = string.Join("|", tempList);
            }

            return output;
        }

        private string DeserializeFloatArray(float[] array) {
            tempList.Clear();
            var output = string.Empty;
            if (array.Length > 0) {
                foreach (var sign in array) {
                    tempList.Add(sign.ToString());
                }
                output = string.Join("|", tempList);
            }

            return output;
        }

        // 反序列
        [MenuItem("Funny/反序列保存VideoRoleConfig")]
        private void DeserializeConfig() {
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            StringBuilder sb = new StringBuilder() {};
            sb.AppendLine("string\tVector3\tVector3\tfloat\tfloat\tstring\tstring[]\tfloat[]\tfloat[]\tstring[]");
            sb.AppendLine("id\tstartPos\tendPos\tstartTime\tduration\tdefaultActionSign\tactionSigns\tactionStartTimes\tactionDurings\teffectSigns");
            sb.AppendLine("时装id\t起始位置\t结束位置\t起始时间（单位秒）\t持续时间\t默认动作\t播放动作(目前最多支持到3个动作，需要的话再提)\t播放动作时间\t动作持续时长(暂时只支持单次动作，播完返回默认动作)\t播放特效");

            if (timelineAsset != null) {
                foreach (var trackAsset in timelineAsset.GetRootTracks()) {
                    foreach (var clip in trackAsset.GetClips()) {
                        var videoRoleControlAsset = clip.asset as VideoRoleControlAsset;
                        if (videoRoleControlAsset != null) {
                            var videoRoleControlBehaviour = videoRoleControlAsset.template;
                            videoRoleControlBehaviour.startTime = (float)clip.start;
                            videoRoleControlBehaviour.duration = (float)clip.duration;
                            
                            var actionStr = DeserializeStringArray(videoRoleControlBehaviour.actionSigns);
                            var actionStartTimeStr = DeserializeFloatArray(videoRoleControlBehaviour.actionStartTimes);
                            var actionDurationStr = DeserializeFloatArray(videoRoleControlBehaviour.actionDurations);
                            var effectSignStr = DeserializeStringArray(videoRoleControlBehaviour.effectSigns);

                            sb.AppendLine($"{videoRoleControlBehaviour.suitID}\t" +
                                          $"{videoRoleControlBehaviour.startPos.ToString()}\t" +
                                          $"{videoRoleControlBehaviour.endPos.ToString()}\t" +
                                          $"{videoRoleControlBehaviour.startTime.ToString()}\t" +
                                          $"{videoRoleControlBehaviour.duration.ToString()}\t" +
                                          $"{videoRoleControlBehaviour.defaultActionSign}\t" +
                                          $"{actionStr}\t" +
                                          $"{actionStartTimeStr}\t" +
                                          $"{actionDurationStr}\t" +
                                          $"{effectSignStr}\t");
                        }

                        break;
                    }
                }
            }
            Debug.Log(sb.ToString());
            string path = Application.dataPath + @"\ToBundle\Config\Txt\VideoRoleConfig.txt";
            File.WriteAllText(path, sb.ToString());
        }

        public void PlayDirector() {
            // 模型加载完成才开始播放
            if (!isPlay) {
                isPlay = true;
                playableDirector.Play();
            }
        }
    }
}