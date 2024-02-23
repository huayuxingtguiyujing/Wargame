using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.WarCamera {
    public class CameraTarget : MonoBehaviour {

        private float horizontalinput;
        private float Verticalinput;

        [Header("�����ƶ���Ե")]
        [SerializeField] private Transform LeftDownCorner;
        [SerializeField] private Transform RightUpCorner;


        public void MoveWithImport(float moveSpeed = 100.0f) {
            //�����ƶ���Χ
            transform.position = new Vector3(
                Mathf.Clamp(
                    transform.position.x,
                    LeftDownCorner.transform.position.x,
                    RightUpCorner.transform.position.x
                    ),
                Mathf.Clamp(
                    transform.position.y,
                    LeftDownCorner.transform.position.y,
                    RightUpCorner.transform.position.y
                    ),
                transform.position.z
            );

            //��ȡ�ƶ�����
            horizontalinput = Input.GetAxis("Horizontal");
            Verticalinput = Input.GetAxis("Vertical");

            transform.Translate(Vector3.right * horizontalinput * Time.deltaTime * moveSpeed);
            transform.Translate(Vector3.up * Verticalinput * Time.deltaTime * moveSpeed);
        }
    }
}