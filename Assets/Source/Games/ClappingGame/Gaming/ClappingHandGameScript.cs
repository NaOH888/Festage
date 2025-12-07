using Assets.Source.Games;
using Assets.Source.Games.ClappingGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ClappingHandGameScript : MonoBehaviour, InputHandler
{
    public DesicionPointSimulator desi_point_prefab;
    public VideoPlayer player;
    public AudioSource audio_source;

    public List<ClapTimeStampsAsset> songs = new List<ClapTimeStampsAsset>();
    private List<ClapTimeStampInfo> current_song_stamps = null;
    private int current_song_idx = 0;

    private List<Tuple<ClapTimeStampInfo, DesicionPointSimulator>>[] playerStampArr = new List<Tuple<ClapTimeStampInfo, DesicionPointSimulator>>[2];
    private Queue<ClapTimeStampInfo> pendingStamps = new Queue<ClapTimeStampInfo>();
    private double songStartTime_ms = 0;

    public int Player1Score
    {
        get { return player1Score;} set
        {
            player1Score = value;
            player1Score_UI.text = "P1: " + player1Score.ToString();
        }
    }
    public int Player2Score
    {
        get { return player2Score; }
        set
        {
            player2Score = value;
            player2Score_UI.text = "P1: " + player2Score.ToString();
        }
    }

    public ClappingGame Game { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private int player1Score = 0;
    private int player2Score = 0;
    public TextMeshProUGUI player1Score_UI;
    public TextMeshProUGUI player2Score_UI;

    public Color player1Color = Color.red;
    public Color player2Color = Color.blue;

    public Canvas canvas;

    public bool isPlaying = false;

    private float timer_after_all_down_s = 0;

    /**
     在加载后主Game类应该注册此脚本为InputManager的Handler。
     
     */
    void InputHandler.OnClap(float x, float y, int idx)
    {
        if (!isPlaying)
        {
            return;
        }
        if(idx >= 2) {
            Debug.LogWarning("ClappingHandGameScript only supports two players!");
            return;
        }
        double currentTime_ms = GetCurrentSongTime_ms();
        var player_stamp = playerStampArr[idx];
        foreach(var tp in player_stamp.ToArray()) {
            var stamp = tp.Item1;
            var point = tp.Item2;
            if(stamp.IsPerfectHit(currentTime_ms)) {
                // Perfect
                PerfectOn(idx);
                point.TriggerPerfect();
                player_stamp.Remove(tp);
                return;
            }
        }
        foreach (var s in songs)
        {
            if(!s.Check())
            {
                Debug.Assert(false);
            }
        }
    }
    private double _internal_dsp_start_s = 0;
    private async void ResetSong(ClapTimeStampsAsset song)
    {
        if(song == null || !song.Check())
        {
            Debug.LogError("ClappingHandGameScript ResetSong received invalid song asset!");
            return;
        }
        player.Stop();
        audio_source.Stop();

        player.clip = song.video;
        song.audio.LoadAudioData();
        audio_source.clip = song.audio;
        

        player.Prepare();
        
        _internal_dsp_start_s = AudioSettings.dspTime;



        current_song_stamps = new List<ClapTimeStampInfo>(song.timeStamps);
        current_song_stamps.Sort();

        pendingStamps.Clear();
        for (int i = 0; i < current_song_stamps.Count; i++)
        {
            pendingStamps.Enqueue(current_song_stamps[i]);
        }
        for (int i = 0; i < 2; i++)
        {
            playerStampArr[i].Clear();
        }

        audio_source.PlayScheduled(_internal_dsp_start_s);
        player.Play();
        player.time = 0;
        player.playbackSpeed = 0;

        songStartTime_ms = _internal_dsp_start_s * 1000;
    }
    private double GetCurrentSongTime_ms()
    {
        return AudioSettings.dspTime * 1000.0 - songStartTime_ms;
    }

    /**
        在开始正式游戏（不是前面的开场）时被调用，此时其所在的prefab也同时被实例化。
        目前只支持两个玩家！！！
     */
    async void Start()
    {
        for(int i = 0;i < 2; i++) {
            playerStampArr[i] = new ();
        }



        // 添加或获取 CanvasScaler
        CanvasScaler scaler = canvas.gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 宽高平均匹配，让 Canvas 不会超屏
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.0f;

        player.audioOutputMode = VideoAudioOutputMode.None;
        audio_source.spatialBlend = 0.0f; // 2D 音频


    }

    
    async void Update()
    {

        if (AudioSettings.dspTime >= _internal_dsp_start_s)
            player.playbackSpeed = 1;
        // 音符判断
        double currentTime_ms = GetCurrentSongTime_ms();


        //先检测队列，如果时间到了该“显示”的时候，就放入对应玩家的stamp列表中
        if(pendingStamps.Count != 0)
        {
            ClapTimeStampInfo inf = pendingStamps.Peek();
            if (inf.judgeTimestamp_ms - currentTime_ms < inf.showing_duration_ms)
            {
                for (int i = 0; i < 2; i++)
                {
                    var lst = playerStampArr[i];
                    
                    DesicionPointSimulator d = Instantiate(desi_point_prefab,canvas.transform);
                    d.SetRGB(i == 0 ? player1Color : player2Color);
                    d.SetPosition(
                        i == 0 ? 0.3f : 0.7f + UnityEngine.Random.Range(-0.2f, 0.2f),
                        0.5f + UnityEngine.Random.Range(-0.2f, 0.2f),
                        0,
                        0);
                    d.duration_ms = (float)inf.showing_duration_ms;
                    d.Play();

                    lst.Add(Tuple.Create(inf, d));
                }
                pendingStamps.Dequeue();
            }
        }
        
        // 每次循环检测，对于每个stamp，如果时间已经达到TooLate程度，就直接判定为Miss
        
        for(int i = 0; i < 2; i++) {
            var stamps = playerStampArr[i];
            for(int idx = 0; idx < stamps.Count; idx++) {
                var stamp = stamps[idx].Item1;
                if(stamp.TooLate(currentTime_ms)) {
                    // Missed
                    MissOn(i);
                    stamps.RemoveAt(idx);
                    idx--;
                }
            }
        }

        // 结束判断

        bool end_game_tick = true;
        end_game_tick &= pendingStamps.Count == 0;
        for (int i = 0; i < 2; i++)
        {
            end_game_tick &= playerStampArr[i].Count == 0;
        }
        end_game_tick &= !player.isPlaying;
        end_game_tick &= !audio_source.isPlaying;
        if (end_game_tick)
        {
            timer_after_all_down_s += Time.deltaTime;
        }
        if (timer_after_all_down_s > 2)
        {
            if (current_song_idx + 1 < songs.Count)
            {
                current_song_idx += 1;
                bool res = await FadeUtility.FadeOutAsync(canvas, 0.5f);
                if (!res) return;
                ResetSong(songs[current_song_idx]);
                timer_after_all_down_s = 0f; // 重置，避免立即再次触发
                await FadeUtility.FadeInAsync(canvas, 0.5f);
            }
            else
            {
                GetComponentInParent<ClappingGame>().ChangeToSettleGame();
            }
        }


    

    }
    private void MissOn(int idx)
    {
        if (idx >= 2 || idx < 0) return;
        if (idx == 0)
        {
            Player1Score -= 1;
        }
        else
        {
            Player2Score -= 1;
        }
        Debug.Log("Player " + (idx + 1) + " Missed!");
    }
    private void PerfectOn(int idx)
    {
        if (idx >= 2 || idx < 0) return;
        Debug.Log("Player " + (idx + 1) + " Perfect!");
        if (idx == 0)
        {
            Player1Score += 1;
        }
        else
        {
            Player2Score += 1;
        }
    }

    public async Task<bool> OnStartGameAsync()
    {
        FadeUtility.BlackImmediate(canvas);
        await Task.Delay(2000);
        bool res = await FadeUtility.FadeInAsync(canvas, 0.5f);
        // 歌曲加载
        ResetSong(songs[current_song_idx]);
        if (res) isPlaying = true;
        return res;
    }

    public async Task<bool> OnStopGame()
    {
        return await FadeUtility.FadeOutAsync(canvas, 0.5f);
    }
}
