using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LandMarkProcess
{
    public class Rectangle
    {
        public float x, y, width, height;

        public Rectangle(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public Vector2 GetPosition()
        {
            return new Vector2(x, y); 
        }

        // 判断一个点是否落在矩形中（包含边界）
        public bool Contains(float px, float py)
        {
            return px >= x &&
                   px <= x + width &&
                   py >= y &&
                   py <= y + height;
        }

        // 判断与另一个矩形是否相交
        public bool Intersects(Rectangle other)
        {
            // 分离轴原则：只要有一个轴不重叠，即不相交
            if (x + width < other.x) return false;      // 本矩形在左边
            if (other.x + other.width < x) return false; // 本矩形在右边
            if (y + height < other.y) return false;     // 在下方
            if (other.y + other.height < y) return false; // 在上方

            return true;
        }
        public Rectangle Pad(float dx, float dy)
        {
            return new(x - dx, y - dy, width + dx, height + dy);
            
        }
        public Vector2 Middle() { return new Vector2(x + width / 2, y + height / 2); }
    }
    public class PlayerContext
    {
        public int PlayerID;
        public bool HasFaceRecognition = false;

        public float LastSeenTime;
        public Vector3 CurrentCenter;     // 用 pelvis / shoulder center 做 anchor
        public Vector3 PredictedCenter;   // 用于下一帧匹配
        public int MissingCount = 0;
    }

    // MediaPipe Pose 输出的单帧数据结构
    public class PoseLandmark
    {
        public Vector3[] Landmarks;    // 33 个点的 world 坐标
        public Vector3?[] GetValidLandmarks()
        {
            if (Landmarks == null)
                return null;

            Vector3?[] valid = new Vector3?[Landmarks.Length];

            for (int i = 0; i < Landmarks.Length; i++)
            {
                Vector3 p = Landmarks[i];

                // 判断 x, y 是否在屏幕内
                bool inScreen =
                    p.x >= 0f && p.x <= 1f &&
                    p.y >= 0f && p.y <= 1f;

                // z 一般 mediapipe 给的是相对深度，不用判断
                valid[i] = inScreen ? p : (Vector3?)null;
            }

            return valid;
        }
    }

    // 统一人物状态（未来会组合 face + hand）
    public class PersonState
    {
        public PlayerContext Context;
        public PoseLandmark Pose;
        public Rectangle GetRoughLeftHandBound()
        {
            // 左手关键点: wrist(15), pinky(17), index(19), thumb(21)
            int[] ids = { 15, 17, 19, 21 };
            return ComputeBound(ids);
        }

        public Rectangle GetRoughRightHandBound()
        {
            // 右手关键点: wrist(16), pinky(18), index(20), thumb(22)
            int[] ids = { 16, 18, 20, 22 };
            return ComputeBound(ids);
        }

        // 核心bound计算
        private Rectangle ComputeBound(int[] indices)
        {
            if (Pose == null || Pose.Landmarks == null || Pose.Landmarks.Length < 33)
                return default;

            bool valid = false;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (int id in indices)
            {
                Vector3 p = Pose.Landmarks[id];

                // 忽略无效数据（某些情况下 mp 返回 (0,0,0) 表示未检测）
                if (p == Vector3.zero)
                    continue;

                valid = true;

                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            if (!valid)
                return default;

            float width = maxX - minX;
            float height = maxY - minY;

            // bound: [xMin, yMin, width, height]
            return new Rectangle(minX, minY, width, height);
        }


        public Vector3 Center => Context.CurrentCenter;
    }


    /**
     * \class PoseLandmarkProcessor
     * 
     * 核心处理器：接收 pose，维持 PlayerID
     * 可处理手部信息、脸部信息，但检测基准是pose
     */

    public class LandmarkProcessor
    {
        // 当前追踪到的玩家
        private readonly List<PersonState> trackedPlayers = new();

        // 匹配距离阈值（可调）
        private const float MATCH_THRESHOLD = 0.3f;
        private const int MISSING_THRESHOLD = 40;

        public readonly DetectorManager<ClapDetector> claps = new(d => new ClapDetector(d));
        public readonly DetectorManager<HandGestureDetector> hands = new(d => new HandGestureDetector(d));

        // 更新 pose
        public void UpdateInfo(List<Vector3[]> poseList)
        {
            // 第一步：构造成 PoseLandmark
            List<PoseLandmark> newPoses = new();
            foreach (var p in poseList)
            {
                if (p != null && p.Length >= 33)
                    newPoses.Add(new PoseLandmark { Landmarks = p });
            }

            // 匹配或创建玩家
            AssignPosesToPlayers(newPoses);
            var tp = trackedPlayers.ToHashSet();
            claps.SyncWith(tp);
            hands.SyncWith(tp);
        }
       
        public (PlayerContext, bool) DispatchHands(Vector3[] hands)
        {
            // 返回对应的上下文和左右手；返回值的第二个值中，false为左
            if (hands == null || hands.Length < 21) return (null, false);

            Vector2 center_pos = (hands[0] + hands[5] + hands[9] + hands[13] + hands[17]) / 5.0f;
            foreach (var state in trackedPlayers)
            {
                if (state.GetRoughLeftHandBound().Contains(center_pos.x,center_pos.y))
                {
                    return (state.Context, false);
                } else if (state.GetRoughRightHandBound().Contains(center_pos.x, center_pos.y))
                {
                    return (state.Context, true);
                }


            }
            return (null, false);

        }


        // 用 pose anchor（身体中心）进行匹配
        private void AssignPosesToPlayers(List<PoseLandmark> poses)
        {
            float time = Time.time;

            // 为快速匹配，先计算每个 pose 的中心点（肩膀 + 髋部）
            List<(PoseLandmark pose, Vector3 center)> poseCenters = new();
            foreach (var pose in poses)
            {
                Vector3 center = ComputePoseCenter(pose);
                poseCenters.Add((pose, center));
            }

            HashSet<int> usedPoseIndex = new();

            // 第一轮：正常匹配
            foreach (var player in trackedPlayers)
            {
                float bestDist = float.MaxValue;
                int bestIndex = -1;

                for (int i = 0; i < poseCenters.Count; i++)
                {
                    if (usedPoseIndex.Contains(i))
                        continue;

                    float d = Vector3.Distance(player.Context.PredictedCenter, poseCenters[i].center);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestIndex = i;
                    }
                }

                if (bestIndex >= 0 && bestDist < MATCH_THRESHOLD)
                {
                    // 成功匹配
                    var (pose, center) = poseCenters[bestIndex];

                    player.Pose = pose;
                    player.Context.CurrentCenter = center;
                    player.Context.PredictedCenter = center;
                    player.Context.LastSeenTime = time;
                    player.Context.MissingCount = 0;  // 重置未匹配计数

                    usedPoseIndex.Add(bestIndex);
                }
                else
                {
                    // 没匹配上
                    player.Context.MissingCount++;
                }
            }

            // 第二轮：处理“未匹配的players”和“未使用的pose”
            // 优先把新的 pose 抢救式地分配给 MissingCount >0 的旧玩家
            foreach (var player in trackedPlayers)
            {
                if (player.Context.MissingCount > 0)   // 有需要抢救的旧玩家
                {
                    int bestIndex = -1;
                    float bestDist = float.MaxValue;

                    for (int i = 0; i < poseCenters.Count; i++)
                    {
                        if (usedPoseIndex.Contains(i))
                            continue;

                        float d = Vector3.Distance(player.Context.PredictedCenter, poseCenters[i].center);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            bestIndex = i;
                        }
                    }

                    if (bestIndex >= 0)
                    {
                        // 抢救匹配成功，不需要满足 MATCH_THRESHOLD
                        var (pose, center) = poseCenters[bestIndex];

                        player.Pose = pose;
                        player.Context.CurrentCenter = center;
                        player.Context.PredictedCenter = center;
                        player.Context.LastSeenTime = time;
                        player.Context.MissingCount = 0;

                        usedPoseIndex.Add(bestIndex);
                    }
                }
            }

            // 第三轮：剩下的 pose 创建新玩家
            for (int i = 0; i < poseCenters.Count; i++)
            {
                if (usedPoseIndex.Contains(i))
                    continue;

                var (pose, center) = poseCenters[i];

                int newID = AllocateNewID();
                trackedPlayers.Add(new PersonState
                {
                    Context = new PlayerContext
                    {
                        PlayerID = newID,
                        CurrentCenter = center,
                        PredictedCenter = center,
                        LastSeenTime = time,
                        MissingCount = 0
                    },
                    Pose = pose
                });
            }

            // 第四轮：删除超过 MissingCount 阈值的玩家
            trackedPlayers.RemoveAll(p => p.Context.MissingCount > MISSING_THRESHOLD);
        }

        // 身体中心
        // 左肩 11，右肩 12，左髋 23，右髋 24
        private Vector3 ComputePoseCenter(PoseLandmark pose)
        {
            return (
                pose.Landmarks[11] +
                pose.Landmarks[12] +
                pose.Landmarks[23] +
                pose.Landmarks[24]
            ) * 0.25f;
        }

        private int AllocateNewID()
        {
            HashSet<int> used = new();
            foreach (var p in trackedPlayers)
                used.Add(p.Context.PlayerID);

            int id = 0;
            while (used.Contains(id))
                id++;

            return id;
        }

        public List<PersonState> GetPlayers()
        {
            return trackedPlayers;
        }
    }


}
