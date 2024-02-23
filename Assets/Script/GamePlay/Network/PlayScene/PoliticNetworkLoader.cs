using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.Infrastructure.Map.Provinces;
using WarGame_True.Infrastructure.NetworkPackage;

namespace WarGame_True.GamePlay.Politic {
    public class PoliticNetworkLoader : NetworkBehaviour {

        [Header("国家势力管理器")]
        [SerializeField] PoliticLoader politicLoader;


        [SerializeField] GameObject networkFactionPrefab;

        // old method: use a middle layer(networkfaction) to hanlder change in faction
        // but it's not convenient
        //public NetworkList<NetworkObjectReference> networkFactions = new NetworkList<NetworkObjectReference>();

        public NetworkVariable<bool> dirtySign = new NetworkVariable<bool>(false);

        // NOTICE: 其初始化必须后于PoliticLoader
        public void InitPoliticNetwork() {

            //if (NetworkManager.Singleton.IsServer) {
            //    //// 根据当前各玩家所选势力创建 networkobj, networkObj 存有指向对应faction的引用
            //    //for(int i = 0; i < politicLoader.BookMarkFactions.Count; i++) {
            //    //    GameObject networkFactionObj = Instantiate(networkFactionPrefab);
            //    //    // 启动networkObject
            //    //    NetworkObject networkObject = networkFactionObj.GetComponent<NetworkObject>();
            //    //    networkObject.Spawn();
            //    //    networkFactions.Add(networkObject);
            //    //}
            //}
            foreach (var faction in politicLoader.BookMarkFactions) {
                // 设置Faction联网同步事件
                faction.ChangeLevelEvent += ChangeLevelEvent;
            }

            // NOTICE: 按理来讲 必定是服务器完成了上述的networkfaction创建，再到客户端执行到这一步

            // 在 所有的客户端 同步networkfaction 与faction
            //SyncNetworkFactions();
        }

        /*      networkFactions 相关方法 勿删
                /// <summary>
                /// 同步所有客户端的 NetworkFaction 国家势力
                /// </summary>
                /// <param name="rpcParams"></param>
                public void SyncNetworkFactions() {
                    Debug.Log("SyncFaction操作: " + NetworkManager.LocalClientId);
                    // 为每个本地的faction 附加上networkFaction, 初始化

                    Debug.Log("networkFactions num: " + networkFactions.Count);
                    foreach (var localFaction in politicLoader.BookMarkFactions)
                    {
                        NetworkFaction networkFaction = GetAvailableNetworkFaction();
                        if (networkFaction != null) {
                            //Debug.Log("设置Faction操作: " + localFaction.FactionInfo.FactionTag);
                            networkFaction.SetNetworkFaction(localFaction, this);
                        } else {
                            //Debug.Log("没有匹配的networkFaction");
                        }
                    }
                    DebugAllNetworkFactions();
                }

                /// <summary>
                /// 获取当前 空出的NetworkFaction
                /// </summary>
                private NetworkFaction GetAvailableNetworkFaction() {
                    // ERROR: 当前在客户端，networkFactions是空的

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
        /// 更改税率、征粮、维护费用的回调，一个玩家的变更，会同步到其他客户端上
        /// </summary>
        private void ChangeLevelEvent(string factionTagStr, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel) {
            NetworkString factionTag = factionTagStr;
            ChangeLevelServerRpc(factionTag, taxLevel, grainLevel, maintenanceLevel);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeLevelServerRpc(NetworkString factionTag, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel, ServerRpcParams rpcParams = default) {

            Debug.Log($"{factionTag.ToString()} 发来请求, 变更其费用水平: TaxLevel-${taxLevel},GrainLevel-${grainLevel},MaintLevel-${maintenanceLevel}");

            ulong changeClientId = rpcParams.Receive.SenderClientId;
            // 通知所有服务器进行更改
            ChangeLevelClientRpc(changeClientId, factionTag, taxLevel, grainLevel, maintenanceLevel);

            // 验证成功: 同步networkObj上的变量是可行的
            dirtySign.Value = true;
        }

        [ClientRpc]
        private void ChangeLevelClientRpc(ulong changeClientId, NetworkString factionTag, TaxLevel taxLevel, ExpropriateGrainLevel grainLevel, MaintenanceLevel maintenanceLevel, ClientRpcParams rpcParams = default) {
            if (changeClientId == NetworkManager.LocalClientId) {
                // 本机上不需要做第二次修改
                return;
            }

            Faction faction = politicLoader.GetFactionByTag(factionTag.ToString());
            if (faction != null) {
                faction.SetLevelDirectly(taxLevel, grainLevel, maintenanceLevel);
                Debug.Log($"{changeClientId}的更改税率请求，{factionTag} 成功更改!");
            } else {
                Debug.Log("未能找到对应的faction: " + factionTag);
            }
        }

    }
}