using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class FadeUtility
{
    private const string FadeLayerName = "_AUTO_FADE_LAYER_";

    // 每个 Canvas 对应一个状态，防止多次调用
    private static Dictionary<Canvas, bool> isFading = new Dictionary<Canvas, bool>();


    public static async Task<bool> FadeOutAsync(Canvas canvas, float duration)
    {
        return await RunFadeAsync(canvas, duration, true);
    }

    public static async Task<bool> FadeInAsync(Canvas canvas, float duration)
    {
        return await RunFadeAsync(canvas, duration, false);
    }
    public static void BlackImmediate(Canvas canvas)
    {
        Image fadeLayer = GetOrCreateFadeLayer(canvas);

        Color c = fadeLayer.color;
        c.a = 1f;
        fadeLayer.color = c;

        // 强行中断任何正在进行的 fade
        isFading[canvas] = false;
    }


    private static async Task<bool> RunFadeAsync(Canvas canvas, float duration, bool fadeOut)
    {
        // fade 正在执行：直接拒绝调用
        if (isFading.TryGetValue(canvas, out bool running) && running)
            return false;  // ❗ 返回 false 代表：这次 fade 没有执行

        isFading[canvas] = true;

        Image fadeLayer = GetOrCreateFadeLayer(canvas);

        float timer = 0f;
        Color c = fadeLayer.color;
        float startA = fadeOut ? 0f : 1f;
        float endA = fadeOut ? 1f : 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            c.a = Mathf.Lerp(startA, endA, t);
            fadeLayer.color = c;
            await Task.Yield();
        }

        c.a = endA;
        fadeLayer.color = c;

        isFading[canvas] = false;

        return true; // ❗ 返回 true 代表：fade 成功执行
    }


    private static Image GetOrCreateFadeLayer(Canvas canvas)
    {
        if (canvas == null) return null ;
        Transform t = canvas.transform.Find(FadeLayerName);
        if (t != null) return t.GetComponent<Image>();

        GameObject go = new GameObject(FadeLayerName, typeof(Image));
        go.transform.SetParent(canvas.transform, false);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        RectTransform rect = img.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return img;
    }
}
