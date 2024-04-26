using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace WarGame_True.Infrastructure.HexagonGrid.DataStruct {
    public class PerlinGenerator {

        public Texture2D mapTex;

        float[,] noise;

        int width;
        int height;

        public PerlinGenerator() { }

        /// <summary>
        /// 生成柏林噪声数据
        /// </summary>
        public float[,] GeneratePerlinNoise(int width, int height, float scale) {
            noise = new float[width, height];

            if (scale <= 0) {
                scale = 0.0001f;
            }

            //Debug.Log(Mathf.PerlinNoise(0.5f, 0.5f));
            //Debug.Log(Mathf.PerlinNoise(1.5f, 1.0f));
            //Debug.Log(Mathf.PerlinNoise(2.5f, 2.5f));
            //Debug.Log(Mathf.PerlinNoise(3.5f, 3.5f));

            scale = scale * Mathf.PI / 3;

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    float x = i * scale;
                    float y = j * scale;
                    noise[i, j] = Mathf.PerlinNoise(x, y);
                    //Debug.Log(Mathf.PerlinNoise(x, y));
                }
            }

            Debug.Log($"成功生成柏林噪声, width:{width}, height:{height}");
            return noise;
        }

        public void GenerateTexture(Renderer render) {

            int width = noise.GetLength(0);
            int height = noise.GetLength(1);

            mapTex = new Texture2D(width, height);

            Color[] colorMap = new Color[width * height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noise[x, y]);
                    //Debug.Log(noise[x, y]);
                }
            }

            mapTex.SetPixels(colorMap);
            mapTex.Apply();

            // 设置render的mat的纹理为生成的噪声纹理
            render.sharedMaterial.mainTexture = mapTex;
            render.transform.localScale = new Vector3(width, 1, height);
        }

        public const float noiseScale = 0.01f;

        public Vector4 SampleNosie(Vector3 position) {
            if (mapTex == null) {
                Debug.LogError("noise map texture 未生成");
                return Vector4.zero;
            }
            return mapTex.GetPixelBilinear(position.x * noiseScale, position.y * noiseScale);
        }
    }
}