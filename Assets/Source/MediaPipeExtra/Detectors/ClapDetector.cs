


using Assets.Source.Managers;
using UnityEngine;
namespace LandMarkProcess
{
    public class ClapDetector : IDetector
    {
        public class ClapContext
        {
            public Vector2 Position { get; set; }
            public float TimeMS { get; set; }
        }
        private readonly PersonState personState;
        private bool clap_this_update = false;
        private float last_clap_time_ms = -999;
        private float last_update_time_ms = 0;
        private bool started = false;

        public bool ClapThisUpdate { get { return clap_this_update; } }
        public float LastClapTimeMS { get { return last_clap_time_ms; } }
        public float ClapMinIntervalMS { get; set; } = 200;
        public float ClapMinSpeedS { get; set; } = 1f; //每秒的屏幕百分比速度

        public delegate void ClapHandler(ClapContext context);
        public event ClapHandler OnClap;

        public ClapDetector(PersonState _personState)
        {
            personState = _personState;
        }
        public void Start()// 可能还得写一个pause之类的
        {
            started = true;
            last_clap_time_ms = -999;
        }
        private Vector2 lastLeft, lastRight;
        public void Update()
        {
            clap_this_update = false;

            if (!started) return;
            if (personState == null || personState.Pose.Landmarks == null) return;
            if (personState.Pose.Landmarks.Length < 33) return;

            float last_update_interval_ms = Time.time * 1000.0f - last_update_time_ms;
            last_update_time_ms = Time.time * 1000.0f;
            //Debug.Log("last_update_interval_ms : " + last_update_interval_ms);
            // 考虑到当检测时间间隔太长，检测依然不准确
            if (last_update_interval_ms > 1000) return;


            var left = personState.GetRoughLeftHandBound();
            var right = personState.GetRoughRightHandBound();

            if(left == null || right == null || left.Equals(default) || right.Equals(default)) return;

            Vector2 cur_left_pos = left.GetPosition();
            Vector2 cur_right_pos = right.GetPosition();

            float speed_ms = ((lastLeft - lastRight).magnitude - (cur_left_pos - cur_right_pos).magnitude) / last_update_interval_ms;
            //Debug.Log("speed_ms : " + speed_ms);
            if (speed_ms >= ClapMinSpeedS / 1000.0f && left.Pad(0.2f, 0.2f).Intersects(right.Pad(0.2f, 0.2f)) && Time.time * 1000.0f - last_clap_time_ms > ClapMinIntervalMS)
            {
                clap_this_update = true;
                last_clap_time_ms = Time.time * 1000.0f;
                OnClap?.Invoke(new ClapContext { Position = (cur_left_pos + cur_right_pos) / 2.0f, TimeMS = last_clap_time_ms });
                InputManager.Clap(0, 0, 0);
            }
            lastLeft = cur_left_pos;
            lastRight = cur_right_pos;

            
        }

    }


}
