using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Managers
{
    public static class InputManager
    {

        private static List<InputHandler> inputHandlers = new ();
        private static List <InputHandler> to_delete = new ();
        private static List <InputHandler> to_add = new ();
        private static int clap_time = 0;
        public static void RegisterHandler(InputHandler handler)
        {
            if(inputHandlers.Contains(handler)) return;
            to_add.Add(handler);
        }
        public static void UnregisterHandler(InputHandler handler)
        {
            if(inputHandlers.Contains(handler)) to_delete.Add(handler);

        }
        public static void Clap(float x, float y, int idx)
        {
            clap_time += 1;
            
        }
        public static void Dispatch_exec_main_thread()
        {
            inputHandlers.AddRange(to_add);
            to_add.Clear();
            while (clap_time > 0)
            {
                foreach (var handler in inputHandlers)
                {
                    if (to_delete.Contains(handler)) continue;
                    handler.OnClap(0, 0, 0);
                }
                clap_time--;
            }
                if (to_delete.Count > 0)
            {
                inputHandlers.RemoveAll(h => to_delete.Contains(h));
                to_delete.Clear();
            }
        }
    }
}