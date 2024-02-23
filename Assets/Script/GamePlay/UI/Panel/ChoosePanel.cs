using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WarGame_True.GamePlay.Application;
using WarGame_True.GamePlay.GameState;
using WarGame_True.GamePlay.Politic;
using WarGame_True.Utils;

namespace WarGame_True.GamePlay.UI {
    public class ChoosePanel : MonoBehaviour {
        [Header("��ѡ�籾��")]
        public string placeHolder;

        [Header("�籾����")]
        [SerializeField] TMP_Text BookMarkDescription;

        [Header("��ѡ����/�Ƽ�����")]
        [SerializeField] Transform countryItemsHolder;
        [SerializeField] GameObject countryItemPrefab;
        [SerializeField] Transform recommandCountryHolder;
        [SerializeField] GameObject recommandCountryPrefab;

        [Header("��ǰ�����������")]
        [SerializeField] GameObject curPlayerPanel;
        [SerializeField] Transform curPlayersHolder;
        [SerializeField] GameObject playerStatuPrefab;

        [Header("�������п�ѡ�Ĺ�������")]
        public List<CountryItem> countryItems;

        [Header("��������")]
        [SerializeField] FactionDetail factionDetail;

        [Header("��ť���")]
        [SerializeField] Button StartGameButton;
        [SerializeField] Button ReturnMenuButton;

        [Header("�����ʾ")]
        [SerializeField] FadeText fadeText;

        //[SerializeField] TMP_Text noticeText;
        //private float noticeWaitTime = 2.0f;
        //private float noticeWaitTimeRec = 2.0f;
        //private float noticeFadeTime = 2.0f;
        //private float noticeFadeTimeRec = 2.0f;

        public delegate void ChooseFactionDelegate(string factionTag);
        public ChooseFactionDelegate chooseFactionCallback;

        // ��ǰ��ҵ�ѡ��
        // TODO: ��ΪҪ��������ģ�飬�����ⲿ�ֲ����õ�һ��currentCountry����¼��
        CountryItem currentCountry;
        public CountryItem LocalCurrentCountry { get => currentCountry; set => currentCountry = value; }

        public FactionInfo currentChoosenFaction { 
            get => LocalCurrentCountry.ItemFaction; 
        }

        public bool HasChooseFaction {
            get => (LocalCurrentCountry != null) && (LocalCurrentCountry.ItemFaction != null);
        }

        public void InitChoosePanel(BookMarks currentBookMarks, UnityAction startGameAction, UnityAction returnMenuAction) {
            
            // ���ݵ�ǰ�籾չʾ��ѡ����
            countryItems = new List<CountryItem>();
            foreach (FactionInfo info in currentBookMarks.BookMarkFactionInfo) {
                GameObject countryItemObject = Instantiate(countryItemPrefab, countryItemsHolder);
                CountryItem countryItem = countryItemObject.GetComponent<CountryItem>();
                countryItem.InitCountryItem(info);
                countryItem.chooseCountryItemCall = OnChooseCountryItemCall;
                countryItems.Add(countryItem);
            }

            // ���ص�ǰ�籾����ʾ��Ϣ
            BookMarkDescription.text = currentBookMarks.bookMarkNotice;
            //noticeText.text = "";
            fadeText.InitText();

            // ���ݵ�ǰ�籾չʾ�Ƽ�����
            if (currentBookMarks.RecommandFactionInfo != null) {
                foreach (var factionInfo in currentBookMarks.RecommandFactionInfo) {
                    GameObject itemObject = Instantiate(recommandCountryPrefab, recommandCountryHolder);
                    CountryItem_Icon countryItem = itemObject.GetComponent<CountryItem_Icon>();
                    // �����Ƽ����ұ�
                    countryItem.InitCountryItem(factionInfo);
                    countryItem.chooseCountryItemCall = OnChooseCountryItemCall;
                }
            }

            // ������ѡ�����չʾ������ϸ��Ϣ
            //if (currentChoosenFaction != null) {
            //    ShowFactionDetail(currentChoosenFaction);
            //} else {
                factionDetail.Hide();
            //}

            curPlayerPanel.SetActive(false);

            // ��ť�¼�
            StartGameButton.onClick.AddListener(startGameAction);
            ReturnMenuButton.onClick.AddListener(returnMenuAction);
        }

        public void ShowCurPlayers(List<PlayerChooseState> rec) {
            curPlayerPanel.SetActive(true);
            curPlayersHolder.gameObject.ClearObjChildren();
            // ���� ���ѡ��״̬�б� ���µ�ǰ��ҵ����
            foreach (PlayerChooseState playerState in rec)
            {
                GameObject playerObject = Instantiate(playerStatuPrefab, curPlayersHolder);
                PlayerStatuItem playerStatuItem = playerObject.GetComponent<PlayerStatuItem>();
                playerStatuItem.SetPlayerStatuItem(playerState.PlayerId, playerState.PlayerFactionTag, playerState.PlayerColor);
            }
            
        }

        public void ShowNotice(string content, int colorType) {

            fadeText.ShowNotice(content, colorType);

            //Color textColor = Color.white;
            ////noticeText.gameObject.SetActive(true);
            //switch (colorType) {
            //    case 0:
            //        textColor = Color.white;
            //        break;
            //    case 1:
            //        textColor = Color.black;
            //        break;
            //    case 2:
            //        textColor = Color.red;
            //        break;
            //    default: 
            //        textColor = Color.black;
            //        break;
            //}
            //noticeText.text = content;
            //noticeText.color = textColor;

            //Debug.Log(content);
            //StartCoroutine(NoticeTextFade());
        }

        /*private IEnumerator NoticeTextFade() {
            // �ȴ���
            while (noticeWaitTimeRec > 0) {
                noticeWaitTimeRec -= Time.deltaTime;
                yield return null;
            }

            
            while (noticeFadeTimeRec > 0) {
                noticeFadeTimeRec -= Time.deltaTime;
                // ��ʼfade
                Color curColor = noticeText.color;
                noticeText.color = new Color(curColor.r, curColor.g, curColor.b, noticeFadeTimeRec / noticeFadeTime);
                yield return null;
            }

            // fade���� ����ַ�
            noticeText.text = "";
            noticeWaitTimeRec = noticeWaitTime;
            noticeFadeTimeRec = noticeFadeTime;
        }*/

        /// <summary>
        /// չʾFaction��ϸ��
        /// </summary>
        /// <param name="factionInfo">��������ϸ��Ϣ</param>
        public void ShowFactionDetail(FactionInfo factionInfo) {
            factionDetail.SetFactionDetail(factionInfo);
            factionDetail.Show();
        }

        public CountryItem GetCountryItemByTag(string FactionTag) {
            CountryItem countryItem = countryItems.Find((country) => {
                return country.ItemFaction.FactionTag == FactionTag;
            });
            return countryItem;
        }

        /// <summary>
        /// ѡ��CountryItem��Ļص�-�����ڵ���ģʽ
        /// </summary>
        /// <param name="choosenItem">��ѡ��Ķ���</param>
        public void OnChooseCountryItemCall(CountryItem choosenItem) {
            if (LocalCurrentCountry != null) {
                LocalCurrentCountry.SetUnchoosen();
            }
            LocalCurrentCountry = choosenItem;
            // ����factionDetail ( TODO: Ӧ������Ϊ�ȴ�)
            ShowFactionDetail(choosenItem.ItemFaction);
            chooseFactionCallback.Invoke(choosenItem.ItemFaction.FactionTag);
            LocalCurrentCountry.SetChoosen();
        }

        /// <summary>
        /// ���µ�ǰ���й���ѡ��״̬-�����ڶ���ģʽ
        /// </summary>
        public void UpdateChooseState(NetworkList<PlayerChooseState> PlayerChooseStates) {
            //List<PlayerChooseState> rec = PlayerChooseStates;

            
        }

    }
}