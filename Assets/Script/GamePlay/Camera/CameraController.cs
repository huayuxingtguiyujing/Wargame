using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.GamePlay.WarCamera {
    /// <summary>
    /// ����Ŀ�������ʵ�������wasd�ƶ������������Ұ����ק�ƶ����������ڱ�Ե�ƶ�etc
    /// ��Ҫ�ҽ���MainCamera��
    /// </summary>
    public class CameraController : MonoBehaviour {

        public static CameraController Instance;

        [Header("�����ƶ���Ե")]
        CameraTarget cameraTarget;
        [SerializeField] private Transform target;

        
        [Header("ʵ����Ұ����")]
        [SerializeField] private float minDistance = 2.5f;
        [SerializeField] private float maxDistance = 12.0f;
        [Tooltip("�����Ұ����ϵ��")]
        [SerializeField] private float scrollSpeed = 1.8f;
        [Tooltip("�����Ŀ��ľ���")]
        [SerializeField] private float distance;
        //[SerializeField] private float addDistanceOffset = 0;

        //λ��ƫ��
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
                    //���ж�x y����,����֮

                    //NOTICE:������ ��ȥ distance��ע�ⳡ����
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
            /*//ʵ�ֻ�������Ч��
            if (addDistanceOffset > 0) {
                addDistanceOffset--;
            } else if (addDistanceOffset < 0) {
                addDistanceOffset++;
            }

            //û��������ֹͣ����ʱ ����
            if (Input.GetAxis("Mouse ScrollWheel") == 0 && addDistanceOffset == 0) {
                return;
            } else if(Input.GetAxis("Mouse ScrollWheel") != 0) {
                addDistanceOffset = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            } else if(addDistanceOffset != 0) {
                //addDistanceOffset = 0;
            }*/

            //�õ�ƫ�������ĳ���
            distance = offsetPosition.magnitude;
            distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            //distance -= addDistanceOffset;

            distance = Mathf.Clamp(
                distance, minDistance, maxDistance
            );

            offsetPosition = offsetPosition.normalized * distance;

            //��������
            Vector3 targetPos = new Vector3(
                transform.position.x,
                transform.position.y,
                target.position.z - distance
            );
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
        }

        /// <summary>
        /// Ϊ�����λ
        /// </summary>
        public void SetCameraPos(Vector3 pos) {
            target.transform.position = pos;
            //Debug.Log("move camera");
        }

    }
}