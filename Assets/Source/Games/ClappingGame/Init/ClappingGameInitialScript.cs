using Assets.Source.Games.ClappingGame;
using Assets.Source.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ClappingGameInitialScript : MonoBehaviour, InputHandler
{

    public Canvas canvas;
    private ClappingGame game;


    private readonly List<LandmarkPointer> landmarks = new();

    public async Task<bool> OnStartGameAsync()
    {
        game = GetComponentInParent<ClappingGame>();
        return await FadeUtility.FadeInAsync(canvas, 0.5f);
    }

    public async Task<bool> OnStopGame()
    {
        return await FadeUtility.FadeOutAsync(canvas, 0.5f);
    }

    public async Task<bool> OnUpdate()
    {
        return true;
    }

    public void OnClap(float x, float y, int idx)
    {
        game.ChangeToMainGame();
    }

    public ClappingGame Game { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
