using Assets.Source.MediaPipeExtra;
using LandMarkProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.Input
{
    public enum TrackPoint
    {
        None,
        LeftHand,
        RightHand,
    }
    [RequireComponent(typeof(Image))]
    public class LandmarkPointer : MonoBehaviour
    {
        private PersonState state;
        private TrackPoint point;
        private Image image;
        private Canvas canvas;

        /// <summary>
        /// 初始化 LandmarkPointer
        /// </summary>
        /// <param name="idx">索引</param>
        /// <param name="component">ILandMarker</param>
        /// <param name="color">手指示器颜色</param>
        public void ResetInfo(PersonState _state, TrackPoint _point, Color color)
        {
            state = _state;
            point = _point;

            // 获取 Image 组件，如果没有就加一个
            image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = false; // 不拦截 UI 输入

            // ResetInfo is called when this pointer is instantiated/prepared. Registration
            // will be performed by the TrackPoseComponent / HandDrawer during initialization
            // lifecycle, so no static/global registration is required here.
        }

        /// <summary>
        /// 每帧更新手指示器位置
        /// </summary>
        private Vector2 prevCanvasPos; // 上一帧的画布位置
        private bool hasPrev = false;

        public void Update()
        {
            if (image == null)
                return;
            if(canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            if (canvas == null) return;

            Vector2 pos = default;
            switch (point)
            {
                case TrackPoint.LeftHand:
                    pos = state.GetRoughLeftHandBound().Middle(); break;
                case TrackPoint.RightHand:
                    pos = state.GetRoughRightHandBound().Middle(); break;
            }

            if (!pos.Equals(default))
            {
                image.enabled = true;

                RectTransform rt = canvas.GetComponent<RectTransform>();

                // 镜像
                float mirroredX = 1f - pos.x;
                float x = (mirroredX - 0.5f) * rt.sizeDelta.x;
                float y = (pos.y - 0.5f) * rt.sizeDelta.y;

                Vector2 targetCanvasPos = new(x, y);

                // 平滑插值
                const float smooth = 0.2f; // 你可以调，比如 0.05 更柔和，0.3 更跟手

                if (!hasPrev)
                {
                    prevCanvasPos = targetCanvasPos;
                    hasPrev = true;
                }
                else
                {
                    prevCanvasPos = Vector2.Lerp(prevCanvasPos, targetCanvasPos, smooth);
                }

                image.rectTransform.anchoredPosition = prevCanvasPos;
            }
            else
            {
                image.enabled = false;
                hasPrev = false; // 丢失时清空历史，避免突然跳位置
            }
        }

    }
}
