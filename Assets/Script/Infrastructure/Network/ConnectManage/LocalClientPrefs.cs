using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    /// <summary>
    /// �����࣬���ڴ洢���ص�������Ϣ�����������ص�������Ϣ
    /// ��װ�� PlayerPrefs
    /// </summary>
    public static class LocalClientPrefs {

        const string musicVolumeKey = "musicVolumeKey";
        const string clientGUIDKey = "clientGUIDKey";
        const string availableProfileKey = "availableProfileKey";

        const float defaultMusicVolume = 0.5f;

        public static float GetMusicVolume() {
            return PlayerPrefs.GetFloat(musicVolumeKey, defaultMusicVolume);
        }

        public static void SetMusicVolume(float volume) {
            PlayerPrefs.SetFloat(musicVolumeKey, volume);
        }

        public static string GetAvailableProfile() {
            return PlayerPrefs.GetString(availableProfileKey, "");
        }

        public static void SetAvailableProfile(string profile) {
            PlayerPrefs.SetString(availableProfileKey, profile);
        }

        /// <summary>
        /// loads a Guid string from Unity preferences
        /// </summary>
        public static string GetGuid() {
            if (PlayerPrefs.HasKey(clientGUIDKey)) {
                return PlayerPrefs.GetString(clientGUIDKey);
            }
            // ���ز�����guid���򴴽�֮
            string guid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(clientGUIDKey, guid);
            return guid;
        }

    }
}