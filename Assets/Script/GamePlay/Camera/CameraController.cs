using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.GamePlay.WarCamera {
    /// <summary>
    /// 相机的控制器，实现相机的wasd移动、鼠标缩放视野、拖拽移动、鼠标放置在边缘移动etc
    /// 需要挂接在MainCamera上
    /// </summary>
    public class CameraController : MonoBehaviour {

        public static CameraController Instance;

        [Header("限制移动边缘")]
        CameraTarget cameraTarget;
        [SerializeField] private Transform target;

        
        [Header("实现视野缩放")]
        [SerializeField] private float minDistance = 2.5f;
        [SerializeField] private float maxDistance = 12.0f;
        [Tooltip("相机视野缩放系数")]
        [SerializeField] private float scrollSpeed = 1.8f;
        [Tooltip("相机到目标的距离")]
        [SerializeField] private float distance;
        //[SerializeField] private float addDistanceOffset = 0;

        //位置偏移
        private Vector3 offsetPosition;

        [SerializeField] private float smoothing = 10.0f;

        [SerializeField] private float moveSpeed = 10.0f;
         

        private void Start() {
            Instance = this;
            cameraTarget = target.GetComponent<CameraTarget>() ;
            offsetPosition = transform.position - target.position;
        }

        private void Update() {
            cameraTarget.MoveWithImport(moveSpeed);
            ScrollView();
        }

        private void LateUpdate() {
            if (target != null) {

                if ((target.position.x != transform.position.x) 
                    || (target.position.y != transform.position.y)) {
                    //仅判断x y方向,跟随之

                    //NOTICE:这里是 减去 distance，注意场景！
                    Vector3 targetPos = new Vector3(
                        target.position.x, 
                        target.position.y, 
                        target.position.z - distance
                    );
                    transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
                }
            }
        }

        void ScrollView() {
            /*//实现滑动缩放效果
            if (addDistanceOffset > 0) {
                addDistanceOffset--;
            } else if (addDistanceOffset < 0) {
                addDistanceOffset++;
            }

            //没有输入且停止滑动时 返回
            if (Input.GetAxis("Mouse ScrollWheel") == 0 && addDistanceOffset == 0) {
                return;
            } else if(Input.GetAxis("Mouse ScrollWheel") != 0) {
                addDistanceOffset = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            } else if(addDistanceOffset != 0) {
                //addDistanceOffset = 0;
            }*/

            //得到偏移向量的长度
            distance = offsetPosition.magnitude;
            distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            //distance -= addDistanceOffset;

            distance = Mathf.Clamp(
                distance, minDistance, maxDistance
            );

            offsetPosition = offsetPosition.normalized * distance;

            //更新缩放
            Vector3 targetPos = new Vector3(
                transform.position.x,
                transform.position.y,
                target.position.z - distance
            );
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
        }

        /// <summary>
        /// 为相机定位
        /// </summary>
        public void SetCameraPos(Vector3 pos) {
            target.transform.position = pos;
            //Debug.Log("move camera");
        }

    }
}