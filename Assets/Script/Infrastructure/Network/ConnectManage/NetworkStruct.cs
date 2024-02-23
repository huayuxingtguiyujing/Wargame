using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Infrastructure.NetworkPackage {

    // Types implementing INetworkSerializable are supported by NetworkSerializer, RPC s and NetworkVariable s.
    [System.Serializable]
    public struct NetworkString : INetworkSerializable {

        FixedString32Bytes Value;

        public override string ToString() {
            return Value.Value.ToString();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref Value);
        }

        public static bool operator ==(NetworkString NS1, NetworkString NS2) {
            return NS1.ToString() == NS2.ToString();
        }

        public static bool operator !=(NetworkString NS1, NetworkString NS2) {
            return !(NS1 == NS2);
        }

        public static implicit operator string(NetworkString NS) {
            return NS.ToString();
        }

        public static implicit operator NetworkString(string s) {
            return new NetworkString() { 
                Value = new FixedString32Bytes(s) 
            };
        }

    }

    public struct NetworkGuid : INetworkSerializeByMemcpy {
        public ulong FirstHalf;
        public ulong SecondHalf;
    }

    public static class NetworkGuidExtensions {
        public static NetworkGuid ToNetworkGuid(this Guid id) {
            var networkId = new NetworkGuid();
            networkId.FirstHalf = BitConverter.ToUInt64(id.ToByteArray(), 0);
            networkId.SecondHalf = BitConverter.ToUInt64(id.ToByteArray(), 8);
            return networkId;
        }

        public static Guid ToGuid(this NetworkGuid networkId) {
            var bytes = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(networkId.FirstHalf), 0, bytes, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(networkId.SecondHalf), 0, bytes, 8, 8);
            return new Guid(bytes);
        }
    }

}