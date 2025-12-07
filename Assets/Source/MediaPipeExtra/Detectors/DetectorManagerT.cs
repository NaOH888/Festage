using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;

namespace LandMarkProcess
{
    public class DetectorManager<T> where T : IDetector
    {
        private readonly Func<PersonState, T> factory;
        private readonly Dictionary<PersonState, T> detectors = new();

        public DetectorManager(Func<PersonState, T> factory)
        {
            this.factory = factory;
        }

        public T Get(PersonState ps)
        {
            detectors.TryGetValue(ps, out var d);
            return d;
        }

        public void SyncWith(HashSet<PersonState> tracked)
        {
            var existing = detectors.Keys.ToHashSet();

            var toRemove = existing.Except(tracked);
            var toAdd = tracked.Except(existing);

            foreach (var p in toRemove) detectors.Remove(p);
            foreach (var p in toAdd) detectors[p] = factory(p);

            foreach (var d in detectors.Values)
            {
                d.Update();
                d.Start();
            }
        }
    }
}
