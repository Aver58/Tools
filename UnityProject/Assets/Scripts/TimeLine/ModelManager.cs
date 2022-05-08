using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Scenes.SS5VideoDemo.Script {
    public class ModelManager : Singleton<ModelManager> {
        private TimelineManager timelineManager;
        private Dictionary<string, GameObject> goMap;
        private Dictionary<string, AnimatorOverrideController> controllerMap;
        private List<string> loadQueue;
        // private List<AssetAsyncOperationHandle> loadings;
        private int syncCount = 6;                               // 同步加载并发数
        private const string defaultControllerPath = @"Assets\Scenes\SS5VideoDemo\VideoAnimator\VideoRoleController1.controller";
        private int ToLoadCount = 0;

        private Coroutine asyncCoroutine;
        public void Init(TimelineManager manager) {
            timelineManager = manager;
            // WarData.IsIntoWar = true; // 合并mesh

            var maxCount = 360;
            loadQueue = new List<string>(maxCount);
            goMap = new Dictionary<string, GameObject>(maxCount);
            controllerMap = new Dictionary<string, AnimatorOverrideController>(maxCount);
            syncCount = SystemInfo.processorCount;
            // loadings = new List<AssetAsyncOperationHandle>(syncCount);

            // MainXLua.Instance.InitLua();

            Debug.Log("syncCount " + syncCount);
        }

        public void StartCoroutine() {
            // asyncCoroutine = CoroutineUtil.Instance.StartCoroutine(LoadAllModelCoroutine());
        }

        // 协程异步加载版本
        // private IEnumerator LoadAllModelCoroutine() {
            // for (var i = 0; i < loadQueue.Count; i++) {
            //     System.Threading.ThreadPool.QueueUserWorkItem(x => {
            //
            //     });
            //     var suitId = loadQueue[i];
            //     Debug.Log("suitId" + suitId);
            //     var modelHandler = LoadModelAsync(suitId);
            //     var config = VideoRoleConfigConfig.Get(suitId);
            //     var animatorOverrideController = InitAnimatorController(config.defaultActionSign, config.actionSigns);
            //     controllerMap[suitId] = animatorOverrideController;
            //     yield return new WaitUntil(()=> ((AssetAsyncOperationHandle)modelHandler).IsDone);
            // }

            // timelineManager.PlayDirector();
        // }

        // 多线程同步加载版本：失败，不能在线程调用unity加载？
        public void LoadAllModelInThread() {
            ToLoadCount = loadQueue.Count;
            Debug.Log($"Launching {ToLoadCount} tasks...");
            var doneEvent = new ManualResetEvent(false);
            for (int i = 0; i < ToLoadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(x => {
                    var index = (int)x;
                    var suitId = loadQueue[index];
                    Debug.Log($"Thread suitId：{suitId} started...");
                    LoadModelSync(suitId);
                    var config = VideoRoleConfigConfig.Get(suitId);
                    var animatorOverrideController = InitAnimatorController(config.defaultActionSign, config.actionSigns);
                    controllerMap[suitId] = animatorOverrideController;
                    Debug.Log($"Thread suitId：{suitId} result calculated...");
                    if (Interlocked.Decrement(ref ToLoadCount) <= 0) {
                        doneEvent.Set();
                    }
                }, i);
            }

            doneEvent.WaitOne();
            Debug.Log("All LoadTask are complete.");

            timelineManager.PlayDirector();
        }

        // 分帧异步加载版本
        public void OnUpdate() {
            return;

            // if (loadQueue != null && loadQueue.Count <= 0) {
            //     timelineManager.PlayDirector();
            //     return;
            // }
            //
            // for (var i = loadings.Count - 1; i > 0; i--) {
            //     var handle = loadings[i];
            //     if (handle.IsDone) {
            //         loadings.RemoveAt(i);
            //     }
            // }
            //
            // while (loadings.Count < syncCount && loadQueue.Count > 0) {
            //     var suitId = loadQueue[loadQueue.Count-1];
            //     loadQueue.RemoveAt(loadQueue.Count-1);
            //     var modelHandler = LoadModelAsync(suitId);
            //     if (modelHandler != null) {
            //         loadings.Add((AssetAsyncOperationHandle) modelHandler);
            //         // 同步加载 todo异步
            //         var config = VideoRoleConfigConfig.Get(suitId);
            //         var animatorOverrideController = InitAnimatorController(config.defaultActionSign, config.actionSigns);
            //         controllerMap[suitId] = animatorOverrideController;
            //     }
            // }
        }

        public void AddLoadModelRequest(string suitId) {
            loadQueue.Add(suitId);
        }

        public GameObject GetModel(string suitId) {
            return goMap.ContainsKey(suitId) ? goMap[suitId] : null;
        }

        public AnimatorOverrideController GetAnimatorController(string suitId) {
            return controllerMap.ContainsKey(suitId) ? controllerMap[suitId] : null;
        }

        private Quaternion rotation = Quaternion.Euler(0, 90f, 0);
        // private AssetAsyncOperationHandle? LoadModelAsync(string suitId) {
        //     if (string.IsNullOrEmpty(suitId)) {
        //         Debug.Log("【VideoRoleControlAsset】没有配置suitID！");
        //         return null;
        //     }
        //     // 加载时装
        //     var suitName = "Suit" + suitId;
        //     GameObject go;
        //     if (Application.isPlaying) {
        //         var handle = AssetManager.LoadAssetAsync<GameObject>("Scenes/SS5VideoDemo/", "VideoRole",
        //             AssetManager.AssetType.Prefab);
        //         handle.Completed += asyncHandle => {
        //             var obj = asyncHandle.Acquire<GameObject>();
        //             if (obj!=null) {
        //                 go = Object.Instantiate(obj, Vector3.back, rotation);
        //                 goMap[suitId] = go;
        //                 go.name = suitName;
        //                 go.GetComponent<Animator>().runtimeAnimatorController = null;
        //                 go.transform.Find("shadow").gameObject.SetActive(true);
        //                 var skinManager = go.GetComponent<SkinManager>();
        //                 if (skinManager!=null) {
        //                     skinManager.Init(true, false, LayerUtil.UILayer);
        //                     skinManager.SetAllLayer(LayerUtil.UILayer);
        //                     // 时装数据在lua层
        //                     MainXLua.Instance.SetSkinInfo(skinManager, int.Parse(suitId));
        //                 }
        //                 go.SetActive(false);
        //             }
        //         };
        //
        //         return handle;
        //     }
        //     return null;
        // }

        private void LoadModelSync(string suitId) {
            // if (string.IsNullOrEmpty(suitId)) {
            //     Debug.Log("【VideoRoleControlAsset】没有配置suitID！");
            //     return;
            // }
            // // 加载时装
            // var suitName = "Suit" + suitId;
            // GameObject go;
            // if (Application.isPlaying) {
            //     var obj = AssetManager.LoadAsset<GameObject>("Scenes/SS5VideoDemo/", "VideoRole",
            //         AssetManager.AssetType.Prefab);
            //     if (obj!=null) {
            //         go = Object.Instantiate(obj, Vector3.back, rotation);
            //         goMap[suitId] = go;
            //         go.name = suitName;
            //         go.GetComponent<Animator>().runtimeAnimatorController = null;
            //         go.transform.Find("shadow").gameObject.SetActive(true);
            //         var skinManager = go.GetComponent<SkinManager>();
            //         if (skinManager!=null) {
            //             skinManager.Init(true, false, LayerUtil.UILayer);
            //             skinManager.SetAllLayer(LayerUtil.UILayer);
            //             // 时装数据在lua层
            //             MainXLua.Instance.SetSkinInfo(skinManager, int.Parse(suitId));
            //         }
            //         go.SetActive(false);
            //     }
            // }
        }

        private AnimatorOverrideController InitAnimatorController(string defaultActionSign, string[] actionSigns) {
            var defaultActionClip = GetAnimationClipBySearchSign(defaultActionSign);
            if (defaultActionClip == null) {
                Debug.LogError("【VideoRoleControlAsset】没有找到指定fbx下的 AnimationClip！defaultActionSign：" + defaultActionSign);
                return null;
            }

            var controllerPath = actionSigns.Length != 0
                ? $"Assets/Scenes/SS5VideoDemo/VideoAnimator/VideoRoleController{actionSigns.Length}.controller"
                : defaultControllerPath;
            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (animatorController == null) {
                Debug.LogError("【VideoRoleControlAsset】没有找到指定 controller！" + controllerPath);
                return null;
            }

            var animatorOverrideController = new AnimatorOverrideController {
                runtimeAnimatorController = animatorController,
                ["defaultAction"] = defaultActionClip
            };

            for (var i = 0; i < actionSigns.Length; i++) {
                var index = i + 1;
                var actionSign = actionSigns[i];
                var actionClip = GetAnimationClipBySearchSign(actionSign);
                if (actionClip == null) {
                    Debug.LogError("【VideoRoleControlAsset】没有找到指定 AnimationClip！" + actionSign);
                    return null;
                }
                animatorOverrideController["action" + index] = actionClip;
            }
            return animatorOverrideController;
        }

        private AnimationClip GetAnimationClipBySearchSign(string searchSign) {
            var sign = searchSign + " t:model";
            var guids = AssetDatabase.FindAssets(sign);
            if (guids == null || guids.Length <= 0) {
                Debug.Log("没有找到！" + sign);
                return null;
            }

            if (guids.Length > 1)
                Debug.LogWarning("找到多个！只取了第一个" + sign);

            var guid = guids[0];
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            return clip ? clip : null;
        }
    }
}