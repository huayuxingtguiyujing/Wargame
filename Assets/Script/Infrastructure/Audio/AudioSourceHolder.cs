using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WarGame_True.Infrastructure.Audio {
    public class AudioSourceHolder : MonoBehaviour {
        [Header("单次播放音频")]
        [Tooltip("仅播放一次")]
        public List<Sound> combatSounds;

        [Header("主题曲")]
        [Tooltip("主题曲，默认情况下会循环播放，与其他音频兼容出现")]
        public List<Sound> mainThemeSounds;

        [Header("环境音")]
        [Tooltip("环境音，默认情况下会循环播放，与其他音频兼容出现")]
        public List<Sound> environmentSounds;
    }

    [Serializable]
    public class Sound {
        [Header("音频剪辑")]
        public AudioClip clip;

        [Header("音频分组")]
        public AudioMixerGroup outputGroup = null;

        [Header("音频音量")]
        [Range(0, 1)]
        public float volume = 0.5f;

        [Header("自动启动")]
        public bool PlayOnAwake;

        [Header("循环播放")]
        public bool loop;

        public Sound(AudioClip clip) {
            this.clip = clip;
            volume = 0.5f;
            PlayOnAwake = false;
            loop = false;
        }

        public static void SoundToAudioSource(Sound sound, ref AudioSource source) {
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.playOnAwake = sound.PlayOnAwake;
            source.loop = sound.loop;
            source.outputAudioMixerGroup = sound.outputGroup;
        }
    }

    public enum SoundType {
        MainTheme,
        Combat,
        Environment
    }

}