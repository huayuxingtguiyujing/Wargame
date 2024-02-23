using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.GamePlay.Politic {
    public class PoliticNetworkLoader : NetworkBehaviour {

        [Header("��������������")]
        [SerializeField] PoliticLoader politicLoader;


        [SerializeField] GameObject networkFactionPrefab;

        // old method: use a middle layer(networkfaction) to hanlder change in faction
        // but it's not convenient
        //public NetworkList<NetworkObjectReference> networkFactions = new NetworkList<NetworkObjectReference>();

        public NetworkVariable<bool> dirtySign = new NetworkVariable<bool>(false);

        // NOTICE: ���ʼ���������PoliticLoader
        public void InitPoliticNetwork() {

            //if (NetworkManager.Singleton.IsServer) {
            //    //// ���ݵ�ǰ�������ѡ�������� networkobj, networkObj ����ָ���Ӧfaction������
            //    //for(int i = 0; i < politicLoader.BookMarkFactions.Count; i++) {
            //    //    GameObject networkFactionObj = Instantiate(networkFactionPrefab);
            //    //    // ����networkObject
            //    //    NetworkObject networkObject = networkFactionObj.GetComponent<NetworkObject>();
            //    //    networkObject.Spawn();
            //    //    networkFactions.Add(networkObject);
            //    //}
            //}
            foreach (var faction in politicLoader.BookMarkFactions) {
                // ����Faction����ͬ���¼�
                faction.ChangeLevelEvent += ChangeLevelEvent;
            }

            // NOTICE: �������� �ض��Ƿ����������������networkfaction�������ٵ��ͻ���ִ�е���һ��

            // �� ���еĿͻ��� ͬ��networkfaction ��faction
            //SyncNetworkFactions();
        }

        /*      networkFactions ��ط��� ��ɾ
                /// <summary>
                /// ͬ�����пͻ��˵� NetworkFaction ��������
                /// </summary>
                /// <param name="rpcParams"></param>
                public void SyncNetworkFactions() {
                    Debug.Log("SyncFaction����: " + NetworkManager.LocalClientId);
                    // Ϊÿ�����ص�faction ������networkFaction, ��ʼ��

                    Debug.Log("networkFactions num: " + networkFactions.Count);
                    foreach (var localFaction in politicLoader.BookMarkFactions)
                    {
                        NetworkFaction networkFaction = GetAvailableNetworkFaction();
                        if (networkFaction != null) {
                            //Debug.Log("����Faction����: " + localFaction.FactionInfo.FactionTag);
                            networkFaction.SetNetworkFaction(localFaction, this);
                        } else {
                            //Debug.Log("û��ƥ���networkFaction");
                        }
                    }
                    DebugAllNetworkFactions();
                }

                /// <summary>
                /// ��ȡ��ǰ �ճ���NetworkFaction
                /// </summary>
                private NetworkFaction GetAvailableNetworkFaction() {
                    // ERROR: ��ǰ�ڿͻ��ˣ�networkFactions�ǿյ�

                    foreach (NetworkObject networkObject in networkFactions)
                    {
                        NetworkFaction networkFaction = networkObject.gameObject.GetComponent<NetworkFaction>();
                        if (!networkFaction.ExistFaction)
                        {
                            return networkFaction;
                        }
                    }
                    return null;
                }

                private void DebugAllNetworkFactions() {
                    foreach (NetworkObject networkObject in networkFactions) {
                        NetworkFaction networkFaction = networkObject.gameObject.GetComponent<NetworkFaction>();
                        Debug.Log(networkFaction.NetworkFactionTag);
                    }
                }

                public NetworkFaction GetNetworkFactionByTag(string tag) {
                    foreach (NetworkObject networkObject in networkFactions) {
                        NetworkFaction networkFaction = networkObject.gameObject.GetComponent<NetworkFaction>();
                        if (networkFaction.ExistFaction && networkFaction.NetworkFactionTag == tag) {
                            return networkFaction;
                        }
                    }
                    return null;
                }

                public Faction GetFactionByTag(string tag) {
                    NetworkFaction networkFaction = GetNetworkFactionByTag(tag);
                    if(networkFaction != null) {
                        return networkFaction.faction;
                    }
                    return null;
                }
        */



        /// <summary>
        /// ����˰�ʡ�������ά�����õĻص���һ����ҵı������ͬ���������ͻ�����
        /// </summary>
        private void ChangeLevelEvent(string factionTagStr, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel) {
            NetworkString factionTag = factionTagStr;
            ChangeLevelServerRpc(factionTag, taxLevel, grainLevel, maintenanceLevel);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeLevelServerRpc(NetworkString factionTag, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel, ServerRpcParams rpcParams = default) {

            Debug.Log($"{factionTag.ToString()} ��������, ��������ˮƽ: TaxLevel-${taxLevel},GrainLevel-${grainLevel},MaintLevel-${maintenanceLevel}");

            ulong changeClientId = rpcParams.Receive.SenderClientId;
            // ֪ͨ���з��������и���
            ChangeLevelClientRpc(changeClientId, factionTag, taxLevel, grainLevel, maintenanceLevel);

            // ��֤�ɹ�: ͬ��networkObj�ϵı����ǿ��е�
            dirtySign.Value = true;
        }

        [ClientRpc]
        private void ChangeLevelClientRpc(ulong changeClientId, NetworkString factionTag, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel, ClientRpcParams rpcParams = default) {
            if (changeClientId == NetworkManager.LocalClientId) {
                // �����ϲ���Ҫ���ڶ����޸�
                return;
            }

            Faction faction = politicLoader.GetFactionByTag(factionTag.ToString());
            if (faction != null) {
                faction.SetLevelDirectly(taxLevel, grainLevel, maintenanceLevel);
                Debug.Log($"{changeClientId}�ĸ���˰������{factionTag} �ɹ�����!");
            } else {
                Debug.Log("δ���ҵ���Ӧ��faction: " + factionTag);
            }
        }

    }
}