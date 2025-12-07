using Assets.Source.Games;
using Assets.Source.Games.ClappingGame;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClappingGameSettleScript : MonoBehaviour, InputHandler
{
    public void OnClap(float x, float y, int idx)
    {
        ClappingGame game = GetComponentInParent<ClappingGame>();
        game.ChangeToMainGame();
    }
    public Canvas canvas;
    public TMPro.TextMeshProUGUI player1ScoreText;
    public TMPro.TextMeshProUGUI player2ScoreText;

    public void SetScores(int player1Score, int player2Score)
    {
        player1ScoreText.text = "P1: " + player1Score.ToString();
        player2ScoreText.text = "P2: " + player2Score.ToString();
    }

    public async Task<bool> OnStartGameAsync()
    {
        return await FadeUtility.FadeInAsync(canvas, 0.5f);
    }

    public async Task<bool> OnStopGame()
    {
        return await FadeUtility.FadeOutAsync(canvas, 0.5f);
    }
}
