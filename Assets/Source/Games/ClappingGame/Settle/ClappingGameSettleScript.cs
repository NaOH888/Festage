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
    public TMPro.TextMeshProUGUI player1CalText;
    public TMPro.TextMeshProUGUI player2CalText;

    public void SetScores(int player1Score, int player2Score)
    {
        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text =  player2Score.ToString();
        player1CalText.text = (player1Score/2-3).ToString();
        player2CalText.text = (player1Score/2-3).ToString();
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
