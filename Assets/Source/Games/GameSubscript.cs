using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Games
{
    public interface IGameSubscript<T> : IFSMState where T : GameInterface 
    {

        public T Game { get; set; }


    }
}
