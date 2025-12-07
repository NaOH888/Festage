using System.Collections;
using UnityEngine;

namespace Assets.Source.Managers
{
    public class DebugInputSimulator : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // 修复 CS0234: 应使用 UnityEngine.Input.GetKeyDown 而不是 Assets.Source.Input.GetKeyDown
            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                InputManager.Clap(0, 0, 0); // player 0
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                InputManager.Clap(0, 0, 1); // player 1
            }
        }
    }
}