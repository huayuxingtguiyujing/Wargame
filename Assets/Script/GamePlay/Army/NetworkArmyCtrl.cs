using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.GamePlay.ArmyPart {
    public class NetworkArmyCtrl : NetworkBehaviour {

        public NetworkList<bool> allArmyObj = new NetworkList<bool>();

        #region �������ṩ���ͻ��˵� RPC �ص�

        /// <summary>
        /// ���ɿͻ��� ���ʹ��� �¾�������ͬ���������ͻ���
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CreateArmyServerRpc(ServerRpcParams serverRpcParams = default) {
            allArmyObj.Add(true);
            Debug.Log("Excute In Server! The sender clientId is:" + serverRpcParams.Receive.SenderClientId);
            // �ͻ��˳ɹ��������Ӻ󣬵��ø÷������������󵽷�����

            // ������֮������ClientRpc�����������Ĳ��ӣ�ͬ�������еĿͻ�����

            // ��clientRpc�е��������ͻ���(��������)�� ArmyController�ķ���
            
        }
        #endregion

        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                StartButtons();
            } else {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        static void StartButtons() {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels() {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition() {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "RpcTest" : "Client CreateArmy")) {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient) {
                    //foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    //NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                } else {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    //var player = playerObject.GetComponent<HelloWorldPlayer>();
                    //player.Move();
                }
            }
        }

    }
}