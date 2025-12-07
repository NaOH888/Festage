using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Games
{
    // 框架还有点问题，先放着
    public class GameFSM<T> where T : GameInterface
    {

        private FSM<IGameSubscript<T>> gameSubscriptsFSM = new();

        public void RegisterSubscript(IGameSubscript<T> subscript)
        {
            
        }




    }
}
