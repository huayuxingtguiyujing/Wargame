using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
//using static Unity.VisualScripting.Member;
//using static UnityEditor.Progress;

namespace WarGame_True.Infrastructure.Audio {
    ///<summary>
    /// 音频管理器 存储所有的音频 并且可以播放和停止
    ///</summary>
    public class AudioManager : MonoBehaviour {
        public AudioSourceHolder audioSourceHolder;

        //每一个音频的名称与音频组件的映射
        private Dictionary<string, AudioSource> audioDic;

        //
        private Dictionary<string, AudioSource> combatAudioDic;
        private AudioSource mainThemeSource;
        private Dictionary<string, AudioSource> mainThemeDic;
        private Dictionary<string, AudioSource> environmentDic;

        // 主题曲队列
        private Sound curThemeName;
        private Queue<Sound> ThemeQueue;

        #region 初始化
        private static AudioManager instance;
        public static AudioManager Instance { get => instance; set => instance = value; }

        ///<summary>
        /// 初始化音频管理器
        ///</summary>
        public void InitAudioManager() {
            audioDic = new Dictionary<string, AudioSource>();
            combatAudioDic = new Dictionary<string, AudioSource>();
            mainThemeDic = new Dictionary<string, AudioSource>();
            environmentDic = new Dictionary<string, AudioSource>();
            instance = this;

            foreach (Sound sound in audioSourceHolder.combatSounds) {
                CreateSourceObj(sound, SoundType.Combat);
            }

            // 初始化 主题曲队列
            ThemeQueue = new Queue<Sound>();
            foreach (Sound sound in audioSourceHolder.mainThemeSounds) {
                //CreateSourceObj(sound, SoundType.MainTheme);
                ThemeQueue.Enqueue(sound);
            }
            // 仅创建一个AudioSource用于播放主题曲
            if(audioSourceHolder.mainThemeSounds.Count >= 1) {
                mainThemeSource = CreateSourceObj(audioSourceHolder.mainThemeSounds[0], SoundType.MainTheme);
            }

            //environmentSounds
            foreach (Sound sound in audioSourceHolder.environmentSounds) {
                CreateSourceObj(sound, SoundType.Environment);
            }

            // 播放音乐
            StartCoroutine(NextMainTheme());
        }

        private AudioSource CreateSourceObj(Sound sound, SoundType soundType) {
            GameObject obj = new GameObject(sound.clip.name);
            obj.transform.SetParent(audioSourceHolder.transform);

            //为音频创建AudioSource类
            AudioSource source = obj.AddComponent<AudioSource>();
            Sound.SoundToAudioSource(sound, ref source);

            if (sound.PlayOnAwake) {
                source.Play();
            }

            //加入到映射中
            audioDic.Add(sound.clip.name, source);

            switch (soundType) {
                case SoundType.Combat:
                    combatAudioDic.Add(sound.clip.name, source);
                    break;
                case SoundType.MainTheme:
                    mainThemeDic.Add(sound.clip.name, source);
                    break;
                case SoundType.Environment:
                    environmentDic.Add(sound.clip.name, source);
                    break;
            }

            return source;
        }
        #endregion

        private void Update() {

            // 自动切换背景音乐
            //if (!mainThemeSource.isPlaying) {
            //    StartCoroutine(NextMainTheme());
            //}
        }

        #region 外部接口
        /// <summary>
        /// 开始播放主题音乐
        /// </summary>
        public static void PlayMainTheme(string name) {
            int countOfMainTheme = instance.ThemeQueue.Count;
            countOfMainTheme++;
            Sound soundRec = instance.ThemeQueue.Peek();
            while (soundRec.clip.name != name && countOfMainTheme >= 0) {
                countOfMainTheme--;
                instance.ThemeQueue.Enqueue(soundRec);
                soundRec = instance.ThemeQueue.Peek();
                instance.ThemeQueue.Dequeue();
            }

            instance.mainThemeSource.Stop();
            instance.mainThemeSource.clip = soundRec.clip;
            instance.mainThemeSource.name = soundRec.clip.name;
            instance.mainThemeSource.Play();
        }

        /// <summary>
        /// 切换到下一首主题曲
        /// </summary>
        public static IEnumerator NextMainTheme() {
            yield return new WaitForSeconds(0.1f);
            //Debug.Log("play next mainTheme!");
            if (instance.curThemeName != null) {
                instance.ThemeQueue.Enqueue(instance.curThemeName);
            }
            instance.curThemeName = instance.ThemeQueue.Peek();
            instance.ThemeQueue.Dequeue();
            Sound.SoundToAudioSource(instance.curThemeName, ref instance.mainThemeSource);
            Debug.Log("now play the mainTheme: " + instance.mainThemeSource.clip.name);
            instance.mainThemeSource.Play();
        }

        public static void PauseMainTheme() {
            instance.mainThemeSource.Pause();
        }

        ///<summary>
        /// 播放某个音频 iswait为是否等待
        ///</summary>
        public static void PlayAudio(string name, bool iswait = false) {
            SoundType soundType = SoundType.MainTheme;
            if (instance.combatAudioDic.ContainsKey(name)) {
                soundType = SoundType.Combat;
            } else if (instance.mainThemeDic.ContainsKey(name)) {
                soundType = SoundType.MainTheme;
            } else if(instance.environmentDic.ContainsKey(name)) {
                soundType = SoundType.Environment;
            }

            if (!instance.audioDic.ContainsKey(name)) {
                //不存在音频
                Debug.Log("不存在" + name + "音频");
                return;
            }

            if (iswait) {
                if (!instance.audioDic[name].isPlaying) {
                    //如果需要等待 则不会播放
                    instance.PlayAudio(name, soundType);
                    //instance.audioDic[name].Play();
                }
            }
            else {
                instance.PlayAudio(name, soundType);
                //instance.audioDic[name].Play();
            }
        }

        private void PlayAudio(string name, SoundType soundType) {
            if (soundType == SoundType.Combat) {
                instance.combatAudioDic[name].Stop();
                instance.combatAudioDic[name].Play();
            } else if (soundType == SoundType.MainTheme) {
                //instance.mainThemeDic[name].Play();
                //AudioManager.PlayMainTheme(name);
                // 暂时不支持通过该函数控制主题曲的播放
            } else if (soundType == SoundType.Environment) {
                instance.environmentDic[name].Play();
            }
        }

        ///<summary>
        /// 停止音频的播放
        ///</summary>
        public static void StopAudio(string name) {
            if (!instance.audioDic.ContainsKey(name)) {
                Debug.LogError("不存在" + name + "音频");
                return;
            }
            else {
                instance.audioDic[name].Stop();
            }
        }

        public static void PauseAudio(string name) {
            if (!instance.audioDic.ContainsKey(name)) {
                Debug.LogError("不存在" + name + "音频");
                return;
            } else {
                instance.audioDic[name].Pause();
            }
        }

        ///<summary>
        /// 停止所有音频的播放
        ///</summary>
        public static void StopAllAudio() {
            foreach (KeyValuePair<string, AudioSource> keyValue in instance.audioDic)
            {
                keyValue.Value.Stop();
            }
        }
        
        #endregion

    }

    public class AudioEffectName {
        // 资源（经济、粮草）
        public static string GainResource = "gain_gold";

        public static string CostResource = "lose_gold";

        // 军队相关
        public static string ChooseArmy = "army_click";
    
        public static string RecruitArmyUnit = "lose_gold";

        public static string ClickMoveArmy = "army_move";

        public static string MergeArmy = "merge_army_fleet";

        public static string SplitArmy = "split_army_fleet";

        // 战斗相关 TODO: 找到合适的资源
        public static string OpenCombat = "";


        // 将领相关 TODO: 找到合适的资源
        public static string ChangeGeneral = "";

        public static string CallbackGeneral = "";

        public static string LocateGeneral = "";

        // 外交相关 TODO: 附加上
        public static string DeclareWar = "declare_war";

        public static string NewDipOffer = "diplomatic_offer";

        public static string DipSuccess = "diplomaticsuccess";

        public static string DipFailure = "diplomaticsuccess";

        // 通用
        public static string ButtonClick01 = "tab_click";

        public static string CloseWindow = "close_window";

    }
}