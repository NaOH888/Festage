using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(ClapTimeStampsAsset))]
public class ClapTimeStampsAssetEditor : Editor
{
    private float bpm = 120f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClapTimeStampsAsset asset = (ClapTimeStampsAsset)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("节拍生成工具", EditorStyles.boldLabel);

        bpm = EditorGUILayout.FloatField("BPM", bpm);

        if (GUILayout.Button("按 BPM 自动生成拍点"))
        {
            GenerateBeats(asset);
        }
    }

    private void GenerateBeats(ClapTimeStampsAsset asset)
    {
        if (asset.audio == null)
        {
            Debug.LogError("需要设置 Audio Clip 才能生成拍点");
            return;
        }

        Undo.RecordObject(asset, "Generate Clap TimeStamps");

        float length_sec = asset.audio.length;
        double length_ms = length_sec * 1000.0;

        // beat 的间隔：60 秒 / BPM
        double beatInterval_ms = 60000.0 / bpm;

        int beatCount = Mathf.FloorToInt((float)(length_ms / beatInterval_ms));

        asset.timeStamps = new List<ClapTimeStampInfo>();

        for (int i = 0; i <= beatCount; i++)
        {
            double t = beatInterval_ms * i;

            ClapTimeStampInfo info = new ClapTimeStampInfo
            {
                judgeTimestamp_ms = t,
                perfectThres_ms = 400,        // 你可以调整
                showing_duration_ms = 1200    // 你可以调整
            };

            asset.timeStamps.Add(info);
        }

        // 按时间排序
        asset.timeStamps.Sort();

        EditorUtility.SetDirty(asset);

        Debug.Log(
            $"已按 BPM={bpm} 自动生成 {asset.timeStamps.Count} 个拍点"
        );
    }
}
#endif