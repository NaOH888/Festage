using Assets.Source.Games.ClappingGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Managers
{
    public class GameManager : MonoBehaviour
    {
        public GameObject clappingGamePrefab;

        public void PostStart()
        {
            var game_parent = Instantiate(clappingGamePrefab);
            var game = game_parent.GetComponent<ClappingGame>();
            
            game.StartGame();
        }

    }
}
