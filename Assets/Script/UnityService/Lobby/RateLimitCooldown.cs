using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.UGS.LobbyPack {
    /// <summary>
    /// 用于Lobby的计时器，以满足lobby 请求限制发送频率的要求
    /// </summary>
    public class RateLimitCooldown {
        public float CooldownTimeLength => m_CooldownTimeLength;

        // 冷却时间
        readonly float m_CooldownTimeLength;
        private float m_CooldownFinishedTime;

        public RateLimitCooldown(float cooldownTimeLength) {
            m_CooldownTimeLength = cooldownTimeLength;
            m_CooldownFinishedTime = -1f;
        }

        public bool CanCall => Time.unscaledTime > m_CooldownFinishedTime;

        /// <summary>
        /// 调用该方法来进入计时
        /// </summary>
        public void PutOnCooldown() {
            m_CooldownFinishedTime = Time.unscaledTime + m_CooldownTimeLength;
        }
    }
}