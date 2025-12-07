using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using LandMarkProcess;

/// <summary>
/// Draws body pose for multiple players using UILineRenderer.
/// Each body part is an independent line (head, arms, legs, torso).
/// Supports smoothing and auto-creation/removal of players.
/// </summary>
public class PoseDrawer
{
    private readonly Canvas canvas;
    private readonly LandmarkProcessor processor;

    // map: player -> list of UILineRenderer (one per body segment)
    private readonly Dictionary<PersonState, List<UILineRenderer>> lines = new();

    // settings
    public float lineWidth = 4f;     // UI pixels
    public Color color = Color.cyan;
    public bool smooth = true;
    public float smoothFactor = 2f;      // smooth iterations

    public PoseDrawer(Canvas canvas, LandmarkProcessor processor)
    {
        this.canvas = canvas;
        this.processor = processor;
    }

    public void Initialize()
    {
        foreach (var p in processor.GetPlayers())
            CreatePlayerLines(p);
    }

    public void Dispose()
    {
        foreach (var ls in lines.Values)
            foreach (var lr in ls)
                if (lr != null)
                    Object.Destroy(lr.gameObject);

        lines.Clear();
    }

    public void SyncFromProcessor()
    {
        var players = processor.GetPlayers();
        var activeSet = new HashSet<PersonState>(players);

        // remove disappeared
        var toRemove = new List<PersonState>();
        foreach (var kv in lines)
        {
            if (!activeSet.Contains(kv.Key))
            {
                foreach (var lr in kv.Value)
                    if (lr != null) Object.Destroy(lr.gameObject);
                toRemove.Add(kv.Key);
            }
        }
        foreach (var p in toRemove) lines.Remove(p);

        // add & update
        foreach (var p in players)
        {
            if (!lines.ContainsKey(p))
                CreatePlayerLines(p);

            UpdatePlayerLines(p);
        }
    }

    // ------------------------------
    // Create segment lines per player
    // ------------------------------
    private void CreatePlayerLines(PersonState p)
    {
        var list = new List<UILineRenderer>();

        for (int i = 0; i < SEGMENTS.Length; i++)
        {
            var go = new GameObject($"Player{p.Context.PlayerID}_Seg{i}");
            go.transform.SetParent(canvas.transform, false);

            var lr = go.AddComponent<UILineRenderer>();
            lr.color = color;
            lr.LineThickness = lineWidth;
            lr.LineList = false;       // polyline
            lr.UseNativeSize = false;

            list.Add(lr);
        }

        lines[p] = list;
    }

    // ------------------------------
    // Update actual geometry
    // ------------------------------
    private void UpdatePlayerLines(PersonState p)
    {
        if (p.Pose == null || p.Pose.Landmarks == null) return;

        var lm = p.Pose.Landmarks;
        RectTransform rt = canvas.GetComponent<RectTransform>();

        var renderList = lines[p];
        for (int i = 0; i < SEGMENTS.Length; i++)
        {
            int[] seg = SEGMENTS[i];
            var uiPts = new List<Vector2>();

            foreach (int id in seg)
            {
                if (id >= lm.Length) continue;
                var p3 = lm[id];

                // MediaPipe normalized, mirror X
                float mx = 1f - p3.x;
                float x = (mx - 0.5f) * rt.sizeDelta.x;
                float y = (p3.y - 0.5f) * rt.sizeDelta.y;

                uiPts.Add(new Vector2(x, y));
            }

            if (smooth && uiPts.Count >= 3)
                uiPts = Chaikin(uiPts, smoothFactor);

            renderList[i].Points = uiPts.ToArray();
            renderList[i].SetAllDirty();
        }
    }

    // ------------------------------
    // Chaikin smoothing
    // ------------------------------
    private List<Vector2> Chaikin(List<Vector2> pts, float iterations)
    {
        List<Vector2> result = new List<Vector2>(pts);
        int it = Mathf.Clamp((int)iterations, 1, 5);

        for (int t = 0; t < it; t++)
        {
            List<Vector2> newPts = new();
            for (int i = 0; i < result.Count - 1; i++)
            {
                var p0 = result[i];
                var p1 = result[i + 1];

                var q = Vector2.Lerp(p0, p1, 0.25f);
                var r = Vector2.Lerp(p0, p1, 0.75f);

                newPts.Add(q);
                newPts.Add(r);
            }
            result = newPts;
        }
        return result;
    }

    // ------------------------------
    // Segments (no cross connections)
    // ------------------------------
    private static readonly int[][] SEGMENTS =
    {
        new[]{10, 9, 8, 6, 5, 4},     // head curve
        new[]{11, 23},                // left torso side
        new[]{12, 24},                // right torso side
        new[]{11, 13, 15},            // left arm
        new[]{12, 14, 16},            // right arm
        new[]{23, 25, 27},            // left leg
        new[]{24, 26, 28},            // right leg
    };
}
