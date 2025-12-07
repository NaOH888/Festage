using LandMarkProcess;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.MediaPipeExtra
{
    public class PoseLandmarkerComponent : MonoBehaviour
    {

        [Header("Config")]
        public int numPose = 2;
        public float minPoseDetectionConfidence = 0.9f;
        public float minPosePresenceConfidence = 0.8f;
        public float minTrackingConfidence = 0.5f;


        private IResourceManager resourceManager;
        private WebCamTexture webCamDevice;
        private const string assetPath = "pose_landmarker_lite.bytes";
        private readonly List<Vector3[]> poseBuffer = new();
        private readonly object poseBufferLock = new();
        private PoseLandmarker poseLandmarker = null;
        private LandmarkProcessor landmarkProcessor;
        // Reusable texture and pixel buffer to avoid per-frame allocations
        private Texture2D reusableTexture = null;
        private Color32[] pixelBuffer = null;
        // Throttle detection to a max FPS and avoid overlapping DetectAsync calls
        public float maxDetectFps = 30f;
        private int detectIntervalMs = 33;
        private long lastDetectTime = 0;
        private bool isDetecting = false;
        public List<Vector3[]> PoseBuffer
        {
            get { return poseBuffer; }
        }



        public void Initailize(WebCamTexture webcam, IResourceManager _resourceManager, LandmarkProcessor lmp)
        {
            resourceManager = _resourceManager;
            webCamDevice = webcam;
            landmarkProcessor = lmp;
            StartCoroutine(Run());
        }

        private System.Collections.IEnumerator Run()
        {
            yield return resourceManager.PrepareAssetAsync(assetPath);

            PoseLandmarkerOptions plmo = new(
            new BaseOptions(BaseOptions.Delegate.CPU, modelAssetPath: assetPath),
            numPoses: numPose,
            minPosePresenceConfidence: minPosePresenceConfidence,
            minTrackingConfidence: minTrackingConfidence,
            minPoseDetectionConfidence: minPoseDetectionConfidence,
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM,
            resultCallback: OnPoseLandmarkerResult
            );
            poseLandmarker = PoseLandmarker.CreateFromOptions(plmo, GpuManager.GpuResources);
            // compute interval ms from configured max fps
            detectIntervalMs = Mathf.Max(1, Mathf.RoundToInt(1000f / Mathf.Max(1f, maxDetectFps)));
            while (true)
            {
                if (webCamDevice.didUpdateThisFrame)
                {
                    // allocate reusable texture and pixel buffer if needed or size changed
                    if (reusableTexture == null || reusableTexture.width != webCamDevice.width || reusableTexture.height != webCamDevice.height)
                    {
                        if (reusableTexture != null) Destroy(reusableTexture);
                        reusableTexture = new Texture2D(webCamDevice.width, webCamDevice.height, TextureFormat.RGBA32, false);
                        pixelBuffer = new Color32[webCamDevice.width * webCamDevice.height];
                    }

                    // copy pixels into reusable buffer to avoid allocations
                    webCamDevice.GetPixels32(pixelBuffer);
                    reusableTexture.SetPixels32(pixelBuffer);
                    reusableTexture.Apply();

                    long now = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (!isDetecting && now - lastDetectTime >= detectIntervalMs)
                    {
                        isDetecting = true;
                        lastDetectTime = now;
                        Mediapipe.Image image = new(reusableTexture);
                        // use callback to clear detecting flag when finished
                        poseLandmarker.DetectAsync(image, now, imageProcessingOptions: null);
                        // Note: OnPoseLandmarkerResult runs on a worker thread; unset isDetecting there if needed.
                        // We'll unset a short time later to avoid starvation in case callback not invoked immediately.
                        _ = ClearDetectingFlagLater();
                    }
                }
                yield return null;
            }

            
        }
        private void OnPoseLandmarkerResult(PoseLandmarkerResult result, Mediapipe.Image image, long timestampMillisec)
        {
            if (result.Equals(default(PoseLandmarkerResult)) || result.poseLandmarks == null || result.poseLandmarks.Count == 0)
            { return; }

            var tmp = new List<Vector3[]>();
            foreach (var normalized in result.poseLandmarks)
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
            lock (poseBufferLock)
            {
                poseBuffer.Clear();
                poseBuffer.AddRange(tmp);
                
            }
            // allow next detection
            isDetecting = false;
        }
        private async System.Threading.Tasks.Task ClearDetectingFlagLater()
        {
            await System.Threading.Tasks.Task.Delay(Mathf.Max(1, detectIntervalMs));
            isDetecting = false;
        }
        private void Update()
        {
            lock (poseBufferLock) { landmarkProcessor?.UpdateInfo(poseBuffer); }
        }
        private void OnDestroy()
        {
            webCamDevice?.Stop();
            poseLandmarker?.Close();
            if (reusableTexture != null)
            {
                Destroy(reusableTexture);
                reusableTexture = null;
            }
        }

    }


    
}
