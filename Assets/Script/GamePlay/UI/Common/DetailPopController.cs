using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    /// <summary>
    /// ���ڿ���չʾϸ�����ԣ�չʾ��ui��Ŀ���������ڣ�����ֵ +20%
    /// </summary>
    public class DetailPopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler {

        [SerializeField] DetailTextPop detailTextPop;

        [SerializeField] DetailMessage detailMessage;

        public void InitPopController(DetailTextPop detailTextPop, DetailMessage detailTextMessage) {
            this.detailTextPop = detailTextPop;
            this.detailMessage = detailTextMessage;
        }

        private void OnMouseEnter() {
            EnterEvent();
        }

        private void OnMouseOver() {
            OverEvent();
        }

        private void OnMouseExit() {
            ExitEvent();
        }

        private void EnterEvent() {
            // �����uiʱ����ʾdetailTextPop
            detailTextPop.Show();
            //Debug.Log("enter pop event! ");
            float widthOfController = GetComponent<RectTransform>().sizeDelta.x;
            float widthOfPop = detailTextPop.GetComponent<RectTransform>().sizeDelta.x;

            //float x = transform.position.x + widthOfController / 2 + widthOfPop / 2;
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            float z = transform.position.z;

            // ����Popλ�õ���controller�ı�Ե
            detailTextPop.transform.position = new Vector3(x, y, z);

            // ����Pop������
            detailTextPop.InitDetailPop(detailMessage);

            //Debug.Log("show!!!: " + detailMessage.Count);
        }

        private void OverEvent() {
            //float x = detailTextPop.transform.position.x;
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            float z = transform.position.z;
            detailTextPop.transform.position = new Vector3(x, y, z);
        }

        private void ExitEvent() {
            detailTextPop.ClearDetailPop();
            detailTextPop.Hide();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            EnterEvent();
        }

        public void OnPointerExit(PointerEventData eventData) {
            ExitEvent();
        }

        public void OnPointerMove(PointerEventData eventData) {
            OverEvent();
        }
    }
}