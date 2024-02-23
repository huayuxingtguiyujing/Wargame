using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WarGame_True.Infrastructure.Audio {
    public class AudioSourceHolder : MonoBehaviour {
        [Header("���β�����Ƶ")]
        [Tooltip("������һ��")]
        public List<Sound> combatSounds;

        [Header("������")]
        [Tooltip("��������Ĭ������»�ѭ�����ţ���������Ƶ���ݳ���")]
        public List<Sound> mainThemeSounds;

        [Header("������")]
        [Tooltip("��������Ĭ������»�ѭ�����ţ���������Ƶ���ݳ���")]
        public List<Sound> environmentSounds;
    }

    [Serializable]
    public class Sound {
        [Header("��Ƶ����")]
        public AudioClip clip;

        [Header("��Ƶ����")]
        public AudioMixerGroup outputGroup = null;

        [Header("��Ƶ����")]
        [Range(0, 1)]
        public float volume = 0.5f;

        [Header("�Զ�����")]
        public bool PlayOnAwake;

        [Header("ѭ������")]
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