using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WarGame_True.WarGameEditor {
    public class TestUI  {

        [MenuItem("Assets/Build AssetBundles")]
        static void BuildAllAssetBundles() {
            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory)) {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                            BuildAssetBundleOptions.None,
                                            BuildTarget.StandaloneWindows);

            AssetBundle myLoadedAssetBundle
                = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "myassetBundle"));
            if (myLoadedAssetBundle == null) {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            var prefab = myLoadedAssetBundle.LoadAsset<GameObject>("MyObject");
            //Instantiate(prefab);
        }


    }
}