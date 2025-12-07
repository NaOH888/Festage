using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameInterface : MonoBehaviour
{
    public abstract void StartGame();
    public abstract void EndGame();
    public abstract void ResetGame();
    public abstract void OnClap(int idx);

    public abstract int MinPlayers();
    public abstract int MaxPlayers();
}
