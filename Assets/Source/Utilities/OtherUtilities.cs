
using UnityEngine;

static class OtherUtility
{
    public static Color ColorFromIndex(int idx)
    {
        // 黄金角（归一化到0~1）
        const float golden = 0.6180339887498948f;

        float h = (idx * golden) % 1f;
        float s = 1f;
        float v = 1f;

        return Color.HSVToRGB(h, s, v);
    }
}
