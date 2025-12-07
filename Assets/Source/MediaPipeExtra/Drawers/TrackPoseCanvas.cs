using Assets.Source.Input;
using LandMarkProcess;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class TrackPoseComponent : MonoBehaviour
{

    private Canvas canvas;
    private LandmarkProcessor processor;
    private LandmarkPointer prefab;
    private HandDrawer handDrawer;
    private PoseDrawer poseDrawer;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        
    }

    private void Start()
    {
        var mm = FindObjectOfType<MainManager>();
        if (mm == null) return;
        processor = mm.LandmarkProcessor;
        prefab = mm.handPointerPrefab;
        if (processor == null || canvas == null) return;

        // Create a HandDrawer instance scoped to this Canvas/context
        handDrawer = new HandDrawer(canvas, processor, prefab);
        handDrawer.InitializeFromProcessor();

        // Create a PoseDrawer instance to render body silhouettes for this Canvas
        poseDrawer = new PoseDrawer(canvas, processor);
        poseDrawer.Initialize();
    }
    private void Update()
    {
        if (processor == null) return;

        // Delegate syncing of players/pointers to HandDrawer
        handDrawer?.SyncFromProcessor();

        // Update silhouettes
        poseDrawer?.SyncFromProcessor();

    }

    private void OnDestroy()
    {
        handDrawer?.Dispose();
        poseDrawer?.Dispose();
    }




}

