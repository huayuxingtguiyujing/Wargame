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
    /// ����״̬�¿�����ʱ�������������ͬ����Ϸʱ�䡢���пͻ��˵�ʱ������¼�
    /// </summary>
    public class TimeNetworkSimulator : NetworkBehaviour {

        // ��Ϸʱ�� ���м������� ������һ��
        NetworkVariable<SerializedGameTime> networkGameTime = new NetworkVariable<SerializedGameTime>();

        [Header("ʱ��ģ����")]
        [SerializeField] TimeSimulator timeSimulator;       // NOTICE: timesimulator�ǵ�����������ڲ��ֶ�Ϊ�õ���

        [Header("ʱ��ģ������UI")]
        [SerializeField] TimeRecorderUI timeRecorderUI;

        // TODO: ���Է������Ƿ��������������пͻ��˵�ʱ��
        public void InitTimeNetwork() {
            
            if (NetworkManager.Singleton.IsServer) {
                // ��Ϸʱ���Է�������ʱ��Ϊ׼
                networkGameTime.Value = timeSimulator.CurGameTime.Time;

                // �Ƿ����, Ϊ��������ui�ҽӶ����¼�
                UnityAction addSpeedServerEvent = () => {
                    ModifyTimeLevelClientRpc(1);
                };
                UnityAction reduceSpeedServerEvent = () => {
                    ModifyTimeLevelClientRpc(-1);
                };
                timeRecorderUI.AddEventToTimeButtons(addSpeedServerEvent, reduceSpeedServerEvent);

                timeSimulator.TimePassComplete += ServerTimePassComplete;

            } else if(NetworkManager.Singleton.IsClient){
                // �ǿͻ���,�رտͻ���ʱ�����ui
                timeRecorderUI.DisableCtrlTime();
            }

        }


        #region ͬ��ʱ�����пͻ��˵�ʱ��

        private void ServerTimePassComplete() {
            networkGameTime.Value = timeSimulator.CurGameTime.Time;
            // ���͸������ͻ��������ǽ���ʱ��
            TimePassCompleteClientRpc();
        }

        [ClientRpc]
        public void TimePassCompleteClientRpc(ClientRpcParams clientRpcParams = default) {
            if (NetworkManager.Singleton.IsServer) {
                // �����ڷ�����(����)����ִ��һ��
                return;
            }
            timeSimulator.SetGameTime(networkGameTime.Value);
            timeSimulator.CompleteTime();

            // ͬ��ui
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

        // NOTICE: ����֤��ע��ʱ����¼��޷�ͨ����������ͬ��
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