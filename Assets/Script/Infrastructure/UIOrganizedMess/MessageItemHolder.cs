using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.Infrastructure.UIOrganizedMess {

    /// <summary>
    /// ��װmessageItem������ʹ�����������������Ŀ�б�
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    //public class MessageItemHolder<TValue> : INetworkSerializable {

    //    public Dictionary<string, MessageItem> MesDic { get; private set; }

    //    public MessageItem this[string key] {
    //        get {
    //            MesDic.TryGetValue(key, out var Value);
    //            return Value;
    //        }
    //        set {
    //            if (MesDic.ContainsKey(key)) {
    //                MesDic[key] = value;
    //            }
    //        }
    //    }

    //    public MessageItemHolder() {
    //        MesDic = new Dictionary<string, MessageItem>();
    //    }

    //    public void AddMesItem(string Desc, float Value) {
    //        if(MesDic == null) {
    //            MesDic = new Dictionary<string, MessageItem>();
    //        }

    //        if (MesDic.ContainsKey(Desc)) {
    //            MesDic[Desc].MesValue = Value;
    //        } else {
    //            MessageItem item = new MessageItem(Desc, Value);
    //            MesDic.Add(Desc, item);
    //        }
            
    //    }

    //    /*/// <summary>
    //    /// �����ֵ��Ϊ0�� ��Ϣ��Ŀ
    //    /// </summary>
    //    /// <returns></returns>
    //    public MessageItemList GetValidMessageList() {
    //        List<MessageItem<float>> messageItems = new List<MessageItem<float>>();

    //        foreach (var pair in messageItemDic)
    //        {
    //            if(pair.Value.itemValue != 0) {
    //                messageItems.Add(pair.Value);
    //            }
    //        }

    //        return messageItems;
    //    }*/

    //    public void ClearItem() {
    //        MesDic.Clear();
    //    }

    //    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //        //serializer.SerializeValue
    //    }
    //}
}