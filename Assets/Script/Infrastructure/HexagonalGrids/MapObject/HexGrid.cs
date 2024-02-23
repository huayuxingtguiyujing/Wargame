using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using WarGame_True.GamePlay.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace WarGame_True.Infrastructure.HexagonGrid.MapObject {
    /// <summary>
    /// HexGrid 地图网格的UI控制类， 仅支持x y平面
    /// </summary>
    public class HexGrid : MonoBehaviour {

        public uint HexID;

        public Vector3 hexPosition;
        private Vector3Int hexIntPosition = Vector3Int.zero;
        public Vector3Int HexIntPosition {
            get {
                hexIntPosition.x = Mathf.RoundToInt(hexPosition.x);
                hexIntPosition.y = Mathf.RoundToInt(hexPosition.y);
                hexIntPosition.z = Mathf.RoundToInt(hexPosition.z);
                return hexIntPosition;
            }
        }

        [SerializeField] SpriteRenderer BGSpriteRenderer;           // 地图网格的最底层
        [SerializeField] SpriteRenderer interactSpriteBaseRenderer; // 部分交互功能（如显示补给线）
        [SerializeField] SpriteRenderer interactSpriteRenderer;     // 部分交互功能（如移动）
        [SerializeField] SpriteRenderer occupySpriteRenderer;       // 表示占领的地图网格
        [SerializeField] SpriteRenderer mouseInteractSpriteRenderer;// 鼠标悬浮效果

        [Header("不同状态下的网格图片")]
        [SerializeField] Sprite normalGridSprite;
        [SerializeField] Sprite mouseOnGridSprite;
        [SerializeField] Sprite choosenGridSprite;
        [SerializeField] Sprite movingGridSprite;
        [SerializeField] Sprite withdrawingGridSprite;
        [SerializeField] Sprite supplyLineGridSprite;

        [Header("坐标位置")]
        [SerializeField] GameObject CanvasPrefab;

        [Header("网格ui")]
        public HexCanvas hexCanvas;

        public void InitHexGird(float size, uint hexID, Vector3 coordinate) {
            HexID = hexID;
            hexPosition = coordinate;
            //适应 六边形 的大小,修改宽高
            float xSize = size * Mathf.Sqrt(3);
            float ySize = size * 2;

            InitHexGrid(xSize, ySize, coordinate);
        }

        private void InitHexGrid(float xSize, float ySize, Vector3 coordinate) {

            //确保在x-y平面上
            transform.rotation = Quaternion.identity;

            Vector2 spriteSize = BGSpriteRenderer.sprite.bounds.size;
            Vector3 currentScale = transform.localScale;
            uint hexID = (uint)HexID;

            float xScale = xSize / spriteSize.x * currentScale.x;
            float yScale = ySize / spriteSize.y * currentScale.y;

            transform.localScale = new Vector3(xScale, yScale, 0);

            // 初始化 省份格子ui
            hexCanvas = Instantiate(CanvasPrefab, transform.parent).GetComponent<HexCanvas>();
            // 让网格显示自己的ID
            hexCanvas.InitHexCanvas(xSize, ySize, hexID, transform.position);
            // 坐标文字,让网格显示自己的位置
            //hexCanvas.InitHexCanvas(xSize, ySize, coordinate, transform.position);

        }


        #region 网格 场景改变
        public void SetHexGridActive() {
            mouseInteractSpriteRenderer.sprite = mouseOnGridSprite;
        }

        public void SetHexGridChoose() {
            mouseInteractSpriteRenderer.sprite = choosenGridSprite;
        }

        public void SetHexGridNormal() {
            mouseInteractSpriteRenderer.sprite = normalGridSprite;
        }

        public void SetHexGridAsMovePath() {
            //Debug.Log("you have set it!");
            interactSpriteRenderer.sprite = movingGridSprite;
        }

        public void SetHexGridAsWithdrawPath() {
            //Debug.Log("you have set it!");
            interactSpriteRenderer.sprite = withdrawingGridSprite;
        }

        public void SetHexGridNoInteract() {
            interactSpriteRenderer.sprite = normalGridSprite;
        }

        public void SetHexGridOccupied(Color controllerColor) {
            controllerColor.a = 0.6f;
            occupySpriteRenderer.color = controllerColor;
            occupySpriteRenderer.gameObject.SetActive(true);
        }
        
        public void SetHexGridUnoccupied() {
            occupySpriteRenderer.gameObject.SetActive(false);
        }

        public void SetHexGridBG(Color girdBGColor, bool activeBG = true) {
            BGSpriteRenderer.gameObject.SetActive(activeBG);
            BGSpriteRenderer.color = girdBGColor;
        }

        public void SetHexGridSupplyLine() {
            interactSpriteBaseRenderer.sprite = supplyLineGridSprite;
        }

        public void SetHexGridNoInterBase() {
            // NOTICE: interactSpriteBaseRenderer 是在 interactSprite 的 下一层
            interactSpriteBaseRenderer.sprite = normalGridSprite;
        }

        #endregion


        #region 省份ui显示
        public void ShowProvinceName(string name) {
            if (hexCanvas == null) {
                Debug.LogError("没有连接到hexcanvas");
                return;
            }
            hexCanvas.ShowProvinceName(name);
        }

        public void ShowOccupyProcess(int maxDay, int currentProcess) {
            //Debug.Log(maxDay + "_" + currentProcess);
            //Debug.Log(hexCanvas == null);
            hexCanvas.ShowOccupyProcess(maxDay, currentProcess);
        }

        public void HideOccupyProcess() {
            hexCanvas.HideOccupyProcess();
        }

        public void ShowRecruitProcess(uint maxProcess, uint lastProcess, string curTaskNum) {
            int curProcess = (int)maxProcess - (int)lastProcess;
            hexCanvas.ShowRecruitProcess((int)maxProcess, curProcess, curTaskNum);
        }

        public void HideRecruitProcess() {
            hexCanvas.HideRecruitProcess();
        }

        public void ShowBuildProcess(uint maxProcess, uint lastProcess, string curTaskNum) {
            int curProcess = (int)maxProcess - (int)lastProcess;
            hexCanvas.ShowBuildProcess((int)maxProcess, curProcess, curTaskNum);
        }

        public void HideBuildProcess() {
            hexCanvas.HideBuildProcess();
        }

        #endregion

    }
}