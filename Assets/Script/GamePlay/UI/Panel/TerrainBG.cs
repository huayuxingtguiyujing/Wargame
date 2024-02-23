using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarGame_True.Infrastructure.Map.Provinces;
using Terrain = WarGame_True.Infrastructure.Map.Provinces.Terrain;

namespace WarGame_True.GamePlay.UI {
    [RequireComponent(typeof(Image))]
    public class TerrainBG : MonoBehaviour {
        [SerializeField] Image m_Image;
        [Header("不同地形下对应的贴图")]
        [SerializeField] Sprite PlainSprite;
        [SerializeField] Sprite HillSprite;
        [SerializeField] Sprite MountainousSprite;
        [SerializeField] Sprite DesertSprite;
        [SerializeField] Sprite CitySprite;
        [SerializeField] Sprite CoastSprite;
        [SerializeField] Sprite OceanSprite;

        public void SetTerrainBG(Terrain terrain) {
            switch (terrain) {
                case Terrain.Plain:
                    m_Image.sprite = PlainSprite;
                    break;
                case Terrain.Desert:
                    m_Image.sprite = DesertSprite;
                    break;
                case Terrain.Hill:
                    m_Image.sprite = HillSprite;
                    break;
                case Terrain.Mountainous:
                    m_Image.sprite = MountainousSprite;
                    break;
                case Terrain.City:
                    m_Image.sprite = CitySprite;
                    break;
                case Terrain.Coast:
                    m_Image.sprite = CoastSprite;
                    break;
                case Terrain.Ocean:
                    m_Image.sprite = OceanSprite;
                    break;
            }
        }

    }
}