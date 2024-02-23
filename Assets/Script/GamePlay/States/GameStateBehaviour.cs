using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.GameState {

    public enum GameState {
        GameStartMenu,
        FactionSelect,
        SingleWarGame,
        MultiWarGame
    }

    /// <summary>
    /// 所有场景管理类的基类，每个场景只允许存在一个这样的物体
    /// </summary>
    public abstract class GameStateBehaviour : MonoBehaviour {

        /// <summary>
        /// 是否可以在多个场景间保存
        /// </summary>
        public virtual bool Persists {
            get { return false; }
        }

        /// <summary>
        /// 该State代表的场景
        /// </summary>
        public abstract GameState ActiveState { get; }

        /// <summary>
        /// 单例，当前激活的State gameObject
        /// </summary>
        private static GameObject CurActiveStateGO;


        protected virtual void Start() {
            if (CurActiveStateGO != null) {
                if (CurActiveStateGO == gameObject) {
                    return;
                }

                //on the host, this might return either the client or server version,
                //but it doesn't matter which;
                //we are only curious about its type, and its persist state.
                var previousState = CurActiveStateGO.GetComponent<GameStateBehaviour>();

                if (previousState.Persists && previousState.ActiveState == ActiveState) {
                    //we need to make way for the DontDestroyOnLoad state that already exists.
                    Destroy(gameObject);
                    return;
                }

                //otherwise, the old state is going away.
                //Either it wasn't a Persisting state, or it was,
                //but we're a different kind of state.
                //In either case, we're going to be replacing it.
                Destroy(CurActiveStateGO);
            }

            CurActiveStateGO = gameObject;
            if (Persists) {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy() {
            if (!Persists) {
                CurActiveStateGO = null;
            }
        }
    }
}