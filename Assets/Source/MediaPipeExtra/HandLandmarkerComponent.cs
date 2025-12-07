using Assets.Source.Input;
using Assets.Source.Managers;
using Assets.Source.MediaPipeExtra;
using LandMarkProcess;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public class HandLandmarkerComponent : MonoBehaviour
{
    [Header("Config")]
    public int numHands = 1;
    public float minHandDetectionConfidence = 0.5f;
    public float minHandPresenceConfidence = 0.5f;
    public float minTrackingConfidence = 0.5f;



    [Header("WebCam")]
    public WebCamTexture webcamTexture;

    [Header("Buffer")]
    public List<Vector3[]> handLandmarkBuffer = new(); // 每帧手势点云
    private bool processor_inited = false;

    private HandLandmarker handLandmarker;
    private readonly object handBufferLock = new();
    float t = 0f;
    IResourceManager resourceManager;
    private LandmarkProcessor landmarkProcessor;

    private void Awake()
    {
        resourceManager = new StreamingAssetsResourceManager();
    }
    public void Initialize(WebCamTexture webcam, IResourceManager _resourceManager, LandmarkProcessor lmp)
    {
        resourceManager = _resourceManager;
        webcamTexture = webcam;
        StartCoroutine(Run());
        landmarkProcessor = lmp;
    }

    

    private IEnumerator Run()
    {
        // 初始化 HandLandmarker
        yield return resourceManager.PrepareAssetAsync("hand_landmarker.bytes");
        HandLandmarkerOptions hlmo = new(
            new BaseOptions(BaseOptions.Delegate.CPU,modelAssetPath: "hand_landmarker.bytes"),
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM,
            numHands:4,
            resultCallback: OnHandLandmarkerResult
            );
        handLandmarker = HandLandmarker.CreateFromOptions(hlmo, GpuManager.GpuResources);

        while (true)
        {
            if (webcamTexture.didUpdateThisFrame)
            {
                var frameTexture = Texture2DFromWebCam(webcamTexture);
                Mediapipe.Image image = new(frameTexture);
                handLandmarker.DetectAsync(image, System.DateTimeOffset.Now.ToUnixTimeMilliseconds(), imageProcessingOptions:null);
            }
            yield return null;
        }
    }

    private Texture2D Texture2DFromWebCam(WebCamTexture cam)
    {
        Texture2D tex = new Texture2D(cam.width, cam.height, TextureFormat.RGBA32, false);
        tex.SetPixels32(cam.GetPixels32());
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// 手部识别异步结果回调（由 HandLandmarker 内部线程调用）  
    /// 签名与 SDK 的 BuildPacketsCallback 里一致： (HandLandmarkerResult, Image, long timestamp)
    /// 注意：回调里不要做耗时工作，只把结果拷贝到线程安全的 buffer 里，供主线程读取
    /// </summary>
    private void OnHandLandmarkerResult(HandLandmarkerResult result, Mediapipe.Image image, long timestampMillisec)
    {
        // result 可能为 default（表示未检测到），我们需要处理这种情况
        if (result.Equals(default(HandLandmarkerResult)) || result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            // 没检测到手：清空 buffer 或保持原样，根据需求选择
            return;
        }

        // 将 NormalizedLandmarks 转成 Vector3[]（x,y,z）并写入 latestHandLandmarks
        var tmp = new List<Vector3[]>();
        foreach (var normalized in result.handLandmarks)
        {
            // normalized 是 NormalizedLandmarks（一个点序列）
            var pts = new Vector3[normalized.landmarks.Count];
            for (int i = 0; i < normalized.landmarks.Count; i++)
            {
                var lm = normalized.landmarks[i];
                // 这里是归一化坐标 (x,y,z)；x,y 在 [0,1]，z 为深度（单位依 SDK）
                pts[i] = new Vector3(lm.x, lm.y, lm.z);
            }
            tmp.Add(pts);
        }

        // 线程安全写入
        lock (handBufferLock)
        {
            handLandmarkBuffer.Clear();
            handLandmarkBuffer.AddRange(tmp);
            
           
        }
    }
   
    private void OnDestroy()
    {
        handLandmarker?.Close();
        if (webcamTexture != null) webcamTexture.Stop();
    }
    void Update()
    {
        t = Time.time;
        
    }

}

