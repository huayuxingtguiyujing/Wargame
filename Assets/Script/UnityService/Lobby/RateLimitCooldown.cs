using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// ����Lobby�ļ�ʱ����������lobby �������Ʒ���Ƶ�ʵ�Ҫ��
    /// </summary>
    public class RateLimitCooldown {
        public float CooldownTimeLength => m_CooldownTimeLength;

        // ��ȴʱ��
        readonly float m_CooldownTimeLength;
        private float m_CooldownFinishedTime;

        public RateLimitCooldown(float cooldownTimeLength) {
            m_CooldownTimeLength = cooldownTimeLength;
            m_CooldownFinishedTime = -1f;
        }

        public bool CanCall => Time.unscaledTime > m_CooldownFinishedTime;

        /// <summary>
        /// ���ø÷����������ʱ
        /// </summary>
        public void PutOnCooldown() {
            m_CooldownFinishedTime = Time.unscaledTime + m_CooldownTimeLength;
        }
    }
}