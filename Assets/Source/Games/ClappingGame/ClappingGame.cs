using Assets.Source.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Source.Games.ClappingGame
{
    public class ClappingGame : GameInterface
    {
        public GameObject initPrefab;
        public GameObject gamePrefab;
        public GameObject settlePrefab;

        private ClappingGameInitialScript initialScript;
        private ClappingHandGameScript mainScript;

        public MainManager mainManager;

        public async void Update()
        {
            initialScript?.OnUpdate();
        }
        public async override void StartGame()
        {
            var init = Instantiate(initPrefab, transform);
            initialScript = init.GetComponent<ClappingGameInitialScript>();
            if (initialScript == null)
            {
                Debug.LogError("ClappingGame requires ClappingGameInitialScript in initPrefab!");
            }
            initialScript.gameObject.SetActive(true);
            InputManager.RegisterHandler(initialScript);
            await initialScript.OnStartGameAsync();

            mainManager = FindObjectOfType<MainManager>();



        }
        public async void ChangeToMainGame()
        {
            if (!await initialScript.OnStopGame()) return;
            InputManager.UnregisterHandler(initialScript);
            Destroy(initialScript.gameObject);
            var main = Instantiate(gamePrefab, transform);
            mainScript = main.GetComponent<ClappingHandGameScript>();
            if (mainScript == null)
            {
                Debug.LogError("ClappingGame requires ClappingHandGameScript in gamePrefab!");
            }
            mainScript.gameObject.SetActive(true);
            InputManager.RegisterHandler(mainScript);
            await mainScript.OnStartGameAsync();

        }
        public async void ChangeToSettleGame()
        {
            if (!await mainScript.OnStopGame()) return;
            InputManager.UnregisterHandler(mainScript);
            Destroy(mainScript.gameObject);
            var settle = Instantiate(settlePrefab, transform);
            var settleScript = settle.GetComponent<ClappingGameSettleScript>();
            if (settleScript == null)
            {
                Debug.LogError("ClappingGame requires ClappingGameSettleScript in settlePrefab!");
            }
            settleScript.gameObject.SetActive(true);
            settleScript.SetScores(mainScript.Player1Score, mainScript.Player2Score);
            InputManager.RegisterHandler(settleScript);
            await settleScript.OnStartGameAsync();
        }
        public override void EndGame()
        {

        }
        public override void ResetGame()
        {

        }
        public override void OnClap(int idx)
        {
        }

        public override int MinPlayers()
        {
            return 2;
        }

        public override int MaxPlayers()
        {
            return 2;
        }
    }
    
}
