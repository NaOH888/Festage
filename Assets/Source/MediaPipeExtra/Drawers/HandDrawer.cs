using Assets.Source.Input;
using LandMarkProcess;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all LandmarkPointer instances for a single Canvas/context.
/// This class is instance-based and should be held as a field on the TrackPoseComponent
/// so its lifecycle matches the Canvas.
/// </summary>
public class HandDrawer
{
    private readonly Canvas canvas;
    private readonly LandmarkPointer prefab;
    private readonly LandmarkProcessor processor;

    // Map a player state to its left/right pointers (index 0 = left, 1 = right)
    private readonly Dictionary<PersonState, LandmarkPointer[]> pointers = new();

    public HandDrawer(Canvas canvas, LandmarkProcessor processor, LandmarkPointer prefab)
    {
        this.canvas = canvas;
        this.processor = processor;
        this.prefab = prefab;
    }

    

    public void InitializeFromProcessor()
    {
        if (processor == null || canvas == null || prefab == null) return;

        foreach (var p in processor.GetPlayers())
        {
            var color = OtherUtility.ColorFromIndex(p.Context.PlayerID);

            LandmarkPointer pointer_l = Object.Instantiate(prefab, canvas.transform);
            pointer_l.ResetInfo(p, TrackPoint.LeftHand, color);

            LandmarkPointer pointer_r = Object.Instantiate(prefab, canvas.transform);
            pointer_r.ResetInfo(p, TrackPoint.RightHand, color);

            pointers[p] = new LandmarkPointer[2] { pointer_l, pointer_r };
        }
    }

    public void SyncFromProcessor()
    {
        if (processor == null) return;

        var currentPlayers = processor.GetPlayers();
        HashSet<PersonState> currentSet = new(currentPlayers);

        var toRemove = new List<PersonState>();
        foreach (var kvp in pointers)
        {
            if (!currentSet.Contains(kvp.Key))
            {
                foreach (var pointer in kvp.Value)
                {
                    if (pointer != null)
                        Object.Destroy(pointer.gameObject);
                }
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var p in toRemove)
            pointers.Remove(p);

        foreach (var p in currentPlayers)
        {
            if (!pointers.ContainsKey(p))
            {
                var color = OtherUtility.ColorFromIndex(p.Context.PlayerID);

                LandmarkPointer pointer_l = Object.Instantiate(prefab, canvas != null ? canvas.transform : null);
                pointer_l.ResetInfo(p, TrackPoint.LeftHand, color);

                LandmarkPointer pointer_r = Object.Instantiate(prefab, canvas != null ? canvas.transform : null);
                pointer_r.ResetInfo(p, TrackPoint.RightHand, color);

                pointers[p] = new LandmarkPointer[2] { pointer_l, pointer_r };
            }
        }
    }

    public void Dispose()
    {
        // Destroy all pointers managed by this HandDrawer
        foreach (var kvp in pointers)
        {
            foreach (var pointer in kvp.Value)
            {
                if (pointer != null)
                {
                    Object.Destroy(pointer.gameObject);
                }
            }
        }
        pointers.Clear();
    }
}
