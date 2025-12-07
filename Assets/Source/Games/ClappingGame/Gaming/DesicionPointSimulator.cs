using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public enum DesicionPointResult
{
    Common,
    Perfect,
    Miss
}
[Serializable]
public class DesicionPointState : FSMActionExecState
{
    public DesicionPointResult Result { get; private set; }
    public AnimationCurve scaleCurve;
    public AnimationCurve alphaCurve;
    public DesicionPointState(DesicionPointResult result)
    {
        Result = result;
    }

}


[RequireComponent(typeof(Image))]
public class DesicionPointSimulator : MonoBehaviour
{
    public float duration_ms = 1000f;

    private FSM<DesicionPointState> fsm = new FSM<DesicionPointState>();

    private float timer = 0f;
    private  bool isPlaying = false;
    private bool is_perfection = false;
    private bool is_missed = false;

    private Image image;
    public DesicionPointState common_pre;
    public DesicionPointState missed_pre;
    public DesicionPointState perfect_pre;

    private float T
    {
        get
        {
            return timer * 1000.0f / duration_ms;
        }
    }
    void Awake()
    {
        image = GetComponent<Image>();
    }
    void Start()
    {
        timer = 0;

        var color = image.color;
        color.a = 0 ;
        image.color = color;

        var common = common_pre;
        var perfect = perfect_pre;
        var miss = missed_pre;

        fsm.AddTransition(common, perfect,() => { return is_perfection; } );
        fsm.AddTransition(common, miss,() => { return is_missed; } );

        common.UpdateAction = () => {
            if (T >= 1.0f)
            {
                image.color = Color.gray;
                is_missed = true;
            }
            
            var color = image.color;
            color.a = common.alphaCurve.Evaluate(T);
            image.color = color;

            float scale = common.scaleCurve.Evaluate(T);
            transform.localScale = new Vector3(scale, scale, scale);
        };
        perfect.Enter += () =>
        {
            timer = duration_ms / 1000.0f - timer;
            image.color = Color.yellow;
            
        };
        perfect.UpdateAction = () =>
        {
            if(T >= 1.0f)
            {
                Destroy(gameObject);
            }
            var color = image.color;
            color.a = perfect.alphaCurve.Evaluate(T);
            image.color = color;

            float scale = perfect.scaleCurve.Evaluate(T);
            transform.localScale = new Vector3(scale, scale, scale);

        };
        miss.Enter += () =>
        {
            timer = 0f;
            image.color = Color.gray;
        };
        miss.UpdateAction = () =>
        {
            if (T >= 1.0f)
            {
                Destroy(gameObject);
            }
            var color = image.color;
            color.a = miss.alphaCurve.Evaluate(T); ;
            image.color = color;
            float scale = miss.scaleCurve.Evaluate(T);
            transform.localScale = new Vector3(scale, scale, scale);
        };

        fsm.CurrentState = common;


    }
    public void SetRGB(Color color)
    {
        color.a = image.color.a;
        image.color = color;
    }
    public void SetPosition(float percentX, float percentY, float pixelOffsetX, float pixelOffsetY)
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);

        float x = Screen.width * percentX + pixelOffsetX;
        float y = Screen.height * percentY + pixelOffsetY;

        // 保证不出屏幕
        float halfW = rect.rect.width * rect.lossyScale.x * 0.5f;
        float halfH = rect.rect.height * rect.lossyScale.y * 0.5f;

        x = Mathf.Clamp(x, halfW, Screen.width - halfW);
        y = Mathf.Clamp(y, halfH, Screen.height - halfH);

        rect.anchoredPosition = new Vector2(x, y);
    }

    void Update()
    {
        if (!isPlaying) return;
        timer += Time.deltaTime;
        fsm.Update();

    }
  

    public void Play()
    {
        isPlaying = true;
        timer = 0f;
    }
    public void TriggerPerfect()
    {
        if (is_missed) return;
        is_perfection = true;
    }
}
