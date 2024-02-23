using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WarGame_True.GamePlay.UI {
    public class GeneralProfileHelper{

        /*// AssetDatabase.LoadAssetAtPath只能在editor文件夹下使用
        public static Sprite GetProfile(string generalName) {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/General/" + generalName + "_General.jpg");
            if (sprite == null) {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/General/Noleader2.png");
            }
            return sprite;
        }*/

        public static async Task<Sprite> GetProfileAsync(string generalName) { 
            string path = "Assets/Sprite/General/" + generalName + "_General.jpg";
            Sprite sprite = await Addressables.LoadAssetAsync<Sprite>(path).Task;
            return sprite;
        }

        //Assets/Sprite/General/LKY_General.jpg
        //Assets/Resources/General/Noleader2.png
        //Assets/Resources/General/王师范.jpg

        public static async Task<Sprite> GetLeaderProfileAsync(string leaderName) {
            string path = "Assets/Sprite/Leaders/" + leaderName + "_Leader.jpg";
            Sprite sprite = await Addressables.LoadAssetAsync<Sprite>(path).Task;
            return sprite;
        }

        //Assets/Sprite/Leaders/
        //Assets/Sprite/Leaders/ZW_Leader.jpg
        //Assets/Sprite/Leaders/ZW_Leaders.jpg
    }
}