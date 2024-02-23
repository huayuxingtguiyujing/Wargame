using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using WarGame_True.GamePlay.UI;
using WarGame_True.Infrastructure.NetworkPackage;
using static WarGame_True.GamePlay.Application.TimeSimulator;
//using static WarGame_True.GamePlay.Application.TimeSimulator;

namespace WarGame_True.GamePlay.Application {
    /// <summary>
    /// 联网状态下开启的时间管理器，用于同步游戏时间、所有客户端的时间结算事件
    /// </summary>
    public class TimeNetworkSimulator : NetworkBehaviour {

        // 游戏时间 所有加入的玩家 均保持一致
        NetworkVariable<SerializedGameTime> networkGameTime = new NetworkVariable<SerializedGameTime>();

        [Header("时间模拟器")]
        [SerializeField] TimeSimulator timeSimulator;       // NOTICE: timesimulator是单例，这里的内部字段为该单例

        [Header("时间模拟器的UI")]
        [SerializeField] TimeRecorderUI timeRecorderUI;

        // TODO: 测试服务器是否能正常控制所有客户端的时间
        public void InitTimeNetwork() {
            
            if (NetworkManager.Singleton.IsServer) {
                // 游戏时间以服务器端时间为准
                networkGameTime.Value = timeSimulator.CurGameTime.Time;

                // 是服务端, 为服务器端ui挂接额外事件
                UnityAction addSpeedServerEvent = () => {
                    ModifyTimeLevelClientRpc(1);
                };
                UnityAction reduceSpeedServerEvent = () => {
                    ModifyTimeLevelClientRpc(-1);
                };
                timeRecorderUI.AddEventToTimeButtons(addSpeedServerEvent, reduceSpeedServerEvent);

                timeSimulator.TimePassComplete += ServerTimePassComplete;

            } else if(NetworkManager.Singleton.IsClient){
                // 是客户端,关闭客户端时间管理ui
                timeRecorderUI.DisableCtrlTime();
            }

        }


        #region 同步时间所有客户端的时间

        private void ServerTimePassComplete() {
            networkGameTime.Value = timeSimulator.CurGameTime.Time;
            // 发送给其他客户端令他们结算时间
            TimePassCompleteClientRpc();
        }

        [ClientRpc]
        public void TimePassCompleteClientRpc(ClientRpcParams clientRpcParams = default) {
            if (NetworkManager.Singleton.IsServer) {
                // 不必在服务器(主机)上再执行一遍
                return;
            }
            timeSimulator.SetGameTime(networkGameTime.Value);
            timeSimulator.CompleteTime();

            // 同步ui
            timeRecorderUI.UpdateTimeRecoder(networkGameTime.Value);
        }

        [ClientRpc]
        public void ModifyTimeLevelClientRpc(int modify, ClientRpcParams clientRpcParams = default) {
            if (NetworkManager.Singleton.IsServer) {
                return;
            }
            //Debug.Log("server modify time" + modify);
            if (modify > 0) {
                timeRecorderUI.AddTimeSpeed();
            } else if(modify < 0) {
                timeRecorderUI.ReduceTimeSpeed();
            }

        }

        #endregion

        // NOTICE: 已验证，注册时间结事件无法通过下述方法同步
        /*[ServerRpc(RequireOwnership = false)]
        public void RegisterEventServerRpc(TimeCallback callback, EventType eventType, ServerRpcParams serverRpcParams = default) {
            switch(eventType) {
                case EventType.Hour:
                    break;
                case EventType.Day:
                    break; 
                case EventType.Month:
                    break;
                case EventType.Year:
                    break;
                default:
                    break;
            }
            SyncRegisterEventClientRpc(callback, eventType);
        }

        [ClientRpc]
        public void SyncRegisterEventClientRpc(TimeCallback callback, EventType eventType, ClientRpcParams clientRpcParams = default) {
            switch (eventType) {
                case EventType.Hour:
                    timeSimulator.SubscribeHourCall(callback);
                    break;
                case EventType.Day:
                    timeSimulator.SubscribeDayCall(callback);
                    break;
                case EventType.Month:
                    timeSimulator.SubscribeMonthCall(callback);
                    break;
                case EventType.Year:
                    timeSimulator.SubscribeYearCall(callback);
                    break;
                default:
                    break;
            }
        }

        public enum EventType {
            Hour,
            Day,
            Month,
            Year
        }*/

    }

}