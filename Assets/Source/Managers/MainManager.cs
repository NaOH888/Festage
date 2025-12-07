using Assets.Source.Input;
using Assets.Source.Managers;
using Assets.Source.MediaPipeExtra;
using Mediapipe.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
[RequireComponent(typeof(DebugInputSimulator))]
[RequireComponent(typeof(HandLandmarkerComponent))]
[RequireComponent(typeof(PoseLandmarkerComponent))]
public class MainManager : MonoBehaviour
{
    private GameManager  gameManager;
    private HandLandmarkerComponent handMarkerComp;
    private PoseLandmarkerComponent poseMarkerComp;
    private IResourceManager resourceManager;
    private WebCamTexture webcamTexture;
    private LandMarkProcess.LandmarkProcessor landmarkProcessor;

    public LandmarkPointer handPointerPrefab;

    public LandMarkProcess.LandmarkProcessor LandmarkProcessor { get { return landmarkProcessor; }}
    void Awake()
    {
        gameManager = GetComponent<GameManager>();
        handMarkerComp = GetComponent<HandLandmarkerComponent>();
        poseMarkerComp = GetComponent<PoseLandmarkerComponent>();
        landmarkProcessor = new();
#if !UNITY_ANDROID && !UNITY_WEBGL
        resourceManager = new LocalResourceManager();
#else
        resourceManager = new StreamingAssetsResourceManager();
#endif
        StartCamera();
        handMarkerComp.Initialize(webcamTexture, resourceManager, landmarkProcessor);
        poseMarkerComp.Initailize(webcamTexture, resourceManager, landmarkProcessor);
    }
    public void Start()
    {
        
        
        gameManager.PostStart();
        


    }
    void Update()
    {
        InputManager.Dispatch_exec_main_thread();
    }

    public HandLandmarkerComponent GetHand()
    {
        return handMarkerComp;
    }
    bool useFrontCamera = true;

    private void StartCamera()
    {
        // 若已有实例，不再重复创建
        if (webcamTexture != null)
            return;

#if UNITY_ANDROID
        // Android 需要根据设备列表选择摄像头
        var devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected on Android.");
            return;
        }

        // 挑选摄像头
        WebCamDevice selected = devices[0];

        foreach (var dev in devices)
        {
            if (useFrontCamera && dev.isFrontFacing)
            {
                selected = dev;
                break;
            }
            if (!useFrontCamera && !dev.isFrontFacing)
            {
                selected = dev;
                break;
            }
        }

        Debug.Log("Using camera: " + selected.name);

        webcamTexture = new WebCamTexture(
            selected.name,
            1280,
            720,
            30
        );

#else
        // Windows / macOS / 编辑器
        webcamTexture = new WebCamTexture(1920, 1080, 60);
#endif

        webcamTexture.Play();
    }
}
