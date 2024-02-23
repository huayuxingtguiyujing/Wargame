using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {
    /// <summary>
    /// 单例类，用于存储本地的连接信息、和其他本地的配置信息
    /// 封装了 PlayerPrefs
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
            // 本地不存在guid，则创建之
            string guid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(clientGUIDKey, guid);
            return guid;
        }

    }
}