using LandMarkProcess;
using UnityEngine;

public class HandGestureDetector : IDetector
{
    private readonly PersonState person;

    // ====== Output Flags ======
    public bool ThrowThisUpdate { get; private set; }
    public bool PullDownThisUpdate { get; private set; }
    public bool RotateThisUpdate { get; private set; }
    public bool TaikoHitLeftThisUpdate { get; private set; }
    public bool TaikoHitRightThisUpdate { get; private set; }

    // ====== 内部缓存 ======
    private Vector3? lastWristRight;
    private Vector3? lastWristLeft;

    private Quaternion lastRightRot;
    private Quaternion lastLeftRot;

    private float lastHitRightTime = -999;
    private float lastHitLeftTime = -999;

    public float MinHitIntervalMS = 250;

    private bool Started = false;

    public HandGestureDetector(PersonState p)
    {
        person = p;
    }

    public void Start()
    {
        Started = true;
    }

    public void Update()
    {
        if (!Started) return;

        ThrowThisUpdate = false;
        PullDownThisUpdate = false;
        RotateThisUpdate = false;
        TaikoHitLeftThisUpdate = false;
        TaikoHitRightThisUpdate = false;

        if (person.Pose == null) return;

        // ❗ GetValidLandmarks 返回 Vector3?[]
        var LM = person.Pose.GetSmooth();
        if (LM == null || LM.Length < 33) return;

        // 安全读取
        Vector3? wristR = LM[16];
        Vector3? wristL = LM[15];

        // ====== 右手动作 ======
        if (wristR.HasValue && lastWristRight.HasValue)
        {
            DetectThrowMotion(wristR.Value);
            DetectPullDown(wristR.Value);
            DetectTaikoHitRight(wristR);
        }

        // ====== 太鼓达人 ======
        DetectTaikoHitLeft(wristL);

        // 记录
        lastWristRight = wristR;
        lastWristLeft = wristL;
    }

    // ============================================================
    // 1. 向前挥动（右手扔球）
    // ============================================================
    private void DetectThrowMotion(Vector3 wrist)
    {
        // lastWristRight 是 Vector3?
        if (!lastWristRight.HasValue) return;
        Vector3 v = wrist - lastWristRight.Value;

        if (Vector3.Dot(v, Vector3.forward) > 0.07f)
        {
            ThrowThisUpdate = true;
            Debug.LogWarning("右手扔球");
        }
    }

    // ============================================================
    // 2. 拉灯绳（右手自上而下）
    // ============================================================
    private void DetectPullDown(Vector3 wrist)
    {
        if (!lastWristRight.HasValue) return;

        if (wrist.y - lastWristRight.Value.y < -0.1f)
        {
            PullDownThisUpdate = true;
            Debug.LogWarning("拉灯绳");
        }
    }


    // ============================================================
    // 4. 左手敲击（太鼓达人）
    // ============================================================
    private void DetectTaikoHitLeft(Vector3? wrist)
    {
        if (!wrist.HasValue || !lastWristLeft.HasValue) return;

        float vy = wrist.Value.y - lastWristLeft.Value.y;

        if (vy < -0.08f &&
            Time.time * 1000 - lastHitLeftTime > MinHitIntervalMS)
        {
            TaikoHitLeftThisUpdate = true;
            lastHitLeftTime = Time.time * 1000;
            Debug.LogWarning("左手敲击（太鼓达人）");
        }
    }

    // ============================================================
    // 5. 右手敲击（太鼓达人）
    // ============================================================
    private void DetectTaikoHitRight(Vector3? wrist)
    {
        if (!wrist.HasValue || !lastWristRight.HasValue) return;

        float vy = wrist.Value.y - lastWristRight.Value.y;

        if (vy < -0.08f &&
            Time.time * 1000 - lastHitRightTime > MinHitIntervalMS)
        {
            TaikoHitRightThisUpdate = true;
            lastHitRightTime = Time.time * 1000;
            Debug.LogWarning("右手敲击（太鼓达人）");
        }
    }
}
