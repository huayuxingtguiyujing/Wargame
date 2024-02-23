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
    /// ��Ƶ������ �洢���е���Ƶ ���ҿ��Բ��ź�ֹͣ
    ///</summary>
    public class AudioManager : MonoBehaviour {
        public AudioSourceHolder audioSourceHolder;

        //ÿһ����Ƶ����������Ƶ�����ӳ��
        private Dictionary<string, AudioSource> audioDic;

        //
        private Dictionary<string, AudioSource> combatAudioDic;
        private AudioSource mainThemeSource;
        private Dictionary<string, AudioSource> mainThemeDic;
        private Dictionary<string, AudioSource> environmentDic;

        // ����������
        private Sound curThemeName;
        private Queue<Sound> ThemeQueue;

        #region ��ʼ��
        private static AudioManager instance;
        public static AudioManager Instance { get => instance; set => instance = value; }

        ///<summary>
        /// ��ʼ����Ƶ������
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

            // ��ʼ�� ����������
            ThemeQueue = new Queue<Sound>();
            foreach (Sound sound in audioSourceHolder.mainThemeSounds) {
                //CreateSourceObj(sound, SoundType.MainTheme);
                ThemeQueue.Enqueue(sound);
            }
            // ������һ��AudioSource���ڲ���������
            if(audioSourceHolder.mainThemeSounds.Count >= 1) {
                mainThemeSource = CreateSourceObj(audioSourceHolder.mainThemeSounds[0], SoundType.MainTheme);
            }

            //environmentSounds
            foreach (Sound sound in audioSourceHolder.environmentSounds) {
                CreateSourceObj(sound, SoundType.Environment);
            }

            // ��������
            StartCoroutine(NextMainTheme());
        }

        private AudioSource CreateSourceObj(Sound sound, SoundType soundType) {
            GameObject obj = new GameObject(sound.clip.name);
            obj.transform.SetParent(audioSourceHolder.transform);

            //Ϊ��Ƶ����AudioSource��
            AudioSource source = obj.AddComponent<AudioSource>();
            Sound.SoundToAudioSource(sound, ref source);

            if (sound.PlayOnAwake) {
                source.Play();
            }

            //���뵽ӳ����
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

            // �Զ��л���������
            //if (!mainThemeSource.isPlaying) {
            //    StartCoroutine(NextMainTheme());
            //}
        }

        #region �ⲿ�ӿ�
        /// <summary>
        /// ��ʼ������������
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
        /// �л�����һ��������
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
        /// ����ĳ����Ƶ iswaitΪ�Ƿ�ȴ�
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
                //��������Ƶ
                Debug.Log("������" + name + "��Ƶ");
                return;
            }

            if (iswait) {
                if (!instance.audioDic[name].isPlaying) {
                    //�����Ҫ�ȴ� �򲻻Ქ��
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
                // ��ʱ��֧��ͨ���ú��������������Ĳ���
            } else if (soundType == SoundType.Environment) {
                instance.environmentDic[name].Play();
            }
        }

        ///<summary>
        /// ֹͣ��Ƶ�Ĳ���
        ///</summary>
        public static void StopAudio(string name) {
            if (!instance.audioDic.ContainsKey(name)) {
                Debug.LogError("������" + name + "��Ƶ");
                return;
            }
            else {
                instance.audioDic[name].Stop();
            }
        }

        public static void PauseAudio(string name) {
            if (!instance.audioDic.ContainsKey(name)) {
                Debug.LogError("������" + name + "��Ƶ");
                return;
            } else {
                instance.audioDic[name].Pause();
            }
        }

        ///<summary>
        /// ֹͣ������Ƶ�Ĳ���
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
        // ��Դ�����á����ݣ�
        public static string GainResource = "gain_gold";

        public static string CostResource = "lose_gold";

        // �������
        public static string ChooseArmy = "army_click";
    
        public static string RecruitArmyUnit = "lose_gold";

        public static string ClickMoveArmy = "army_move";

        public static string MergeArmy = "merge_army_fleet";

        public static string SplitArmy = "split_army_fleet";

        // ս����� TODO: �ҵ����ʵ���Դ
        public static string OpenCombat = "";


        // ������� TODO: �ҵ����ʵ���Դ
        public static string ChangeGeneral = "";

        public static string CallbackGeneral = "";

        public static string LocateGeneral = "";

        // �⽻��� TODO: ������
        public static string DeclareWar = "declare_war";

        public static string NewDipOffer = "diplomatic_offer";

        public static string DipSuccess = "diplomaticsuccess";

        public static string DipFailure = "diplomaticsuccess";

        // ͨ��
        public static string ButtonClick01 = "tab_click";

        public static string CloseWindow = "close_window";

    }
}