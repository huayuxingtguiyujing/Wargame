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
        [Header("可选剧本表")]
        public string placeHolder;

        [Header("剧本描述")]
        [SerializeField] TMP_Text BookMarkDescription;

        [Header("可选势力/推荐势力")]
        [SerializeField] Transform countryItemsHolder;
        [SerializeField] GameObject countryItemPrefab;
        [SerializeField] Transform recommandCountryHolder;
        [SerializeField] GameObject recommandCountryPrefab;

        [Header("当前所有在线玩家")]
        [SerializeField] GameObject curPlayerPanel;
        [SerializeField] Transform curPlayersHolder;
        [SerializeField] GameObject playerStatuPrefab;

        [Header("旗下所有可选的国家势力")]
        public List<CountryItem> countryItems;

        [Header("势力描述")]
        [SerializeField] FactionDetail factionDetail;

        [Header("按钮组件")]
        [SerializeField] Button StartGameButton;
        [SerializeField] Button ReturnMenuButton;

        [Header("面板提示")]
        [SerializeField] FadeText fadeText;

        //[SerializeField] TMP_Text noticeText;
        //private float noticeWaitTime = 2.0f;
        //private float noticeWaitTimeRec = 2.0f;
        //private float noticeFadeTime = 2.0f;
        //private float noticeFadeTimeRec = 2.0f;

        public delegate void ChooseFactionDelegate(string factionTag);
        public ChooseFactionDelegate chooseFactionCallback;

        // 当前玩家的选择
        // TODO: 因为要兼容联网模块，所以这部分不能用单一的currentCountry来记录！
        CountryItem currentCountry;
        public CountryItem LocalCurrentCountry { get => currentCountry; set => currentCountry = value; }

        public FactionInfo currentChoosenFaction { 
            get => LocalCurrentCountry.ItemFaction; 
        }

        public bool HasChooseFaction {
            get => (LocalCurrentCountry != null) && (LocalCurrentCountry.ItemFaction != null);
        }

        public void InitChoosePanel(BookMarks currentBookMarks, UnityAction startGameAction, UnityAction returnMenuAction) {
            
            // 根据当前剧本展示可选国家
            countryItems = new List<CountryItem>();
            foreach (FactionInfo info in currentBookMarks.BookMarkFactionInfo) {
                GameObject countryItemObject = Instantiate(countryItemPrefab, countryItemsHolder);
                CountryItem countryItem = countryItemObject.GetComponent<CountryItem>();
                countryItem.InitCountryItem(info);
                countryItem.chooseCountryItemCall = OnChooseCountryItemCall;
                countryItems.Add(countryItem);
            }

            // 加载当前剧本的提示信息
            BookMarkDescription.text = currentBookMarks.bookMarkNotice;
            //noticeText.text = "";
            fadeText.InitText();

            // 根据当前剧本展示推荐国家
            if (currentBookMarks.RecommandFactionInfo != null) {
                foreach (var factionInfo in currentBookMarks.RecommandFactionInfo) {
                    GameObject itemObject = Instantiate(recommandCountryPrefab, recommandCountryHolder);
                    CountryItem_Icon countryItem = itemObject.GetComponent<CountryItem_Icon>();
                    // 设置推荐国家表
                    countryItem.InitCountryItem(factionInfo);
                    countryItem.chooseCountryItemCall = OnChooseCountryItemCall;
                }
            }

            // 根据已选择国家展示国家详细信息
            //if (currentChoosenFaction != null) {
            //    ShowFactionDetail(currentChoosenFaction);
            //} else {
                factionDetail.Hide();
            //}

            curPlayerPanel.SetActive(false);

            // 按钮事件
            StartGameButton.onClick.AddListener(startGameAction);
            ReturnMenuButton.onClick.AddListener(returnMenuAction);
        }

        public void ShowCurPlayers(List<PlayerChooseState> rec) {
            curPlayerPanel.SetActive(true);
            curPlayersHolder.gameObject.ClearObjChildren();
            // 根据 玩家选择状态列表 更新当前玩家的面板
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
            // 等待中
            while (noticeWaitTimeRec > 0) {
                noticeWaitTimeRec -= Time.deltaTime;
                yield return null;
            }

            
            while (noticeFadeTimeRec > 0) {
                noticeFadeTimeRec -= Time.deltaTime;
                // 开始fade
                Color curColor = noticeText.color;
                noticeText.color = new Color(curColor.r, curColor.g, curColor.b, noticeFadeTimeRec / noticeFadeTime);
                yield return null;
            }

            // fade结束 清空字符
            noticeText.text = "";
            noticeWaitTimeRec = noticeWaitTime;
            noticeFadeTimeRec = noticeFadeTime;
        }*/

        /// <summary>
        /// 展示Faction的细节
        /// </summary>
        /// <param name="factionInfo">势力的详细信息</param>
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
        /// 选择CountryItem后的回调-适用于单机模式
        /// </summary>
        /// <param name="choosenItem">被选择的对象</param>
        public void OnChooseCountryItemCall(CountryItem choosenItem) {
            if (LocalCurrentCountry != null) {
                LocalCurrentCountry.SetUnchoosen();
            }
            LocalCurrentCountry = choosenItem;
            // 唤醒factionDetail ( TODO: 应该设置为等待)
            ShowFactionDetail(choosenItem.ItemFaction);
            chooseFactionCallback.Invoke(choosenItem.ItemFaction.FactionTag);
            LocalCurrentCountry.SetChoosen();
        }

        /// <summary>
        /// 更新当前所有国家选择状态-适用于多人模式
        /// </summary>
        public void UpdateChooseState(NetworkList<PlayerChooseState> PlayerChooseStates) {
            //List<PlayerChooseState> rec = PlayerChooseStates;

            
        }

    }
}