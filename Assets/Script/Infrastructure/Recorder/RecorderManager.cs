using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.Infrastructure.Recorder {
    public class RecorderManager {

        public List<BaseRecorder> recorderList;



        public void RegisterNewRecorder(BaseRecorder baseRecorder) {
            if(recorderList == null) {
                recorderList = new List<BaseRecorder>();
            }

            recorderList.Add(baseRecorder);
        }

        public void CountRecorder() {
            List<BaseRecorder> overRecorders = new List<BaseRecorder>();
            // 遍历每个recorder 对其进行计时
            foreach (BaseRecorder recorder in recorderList)
            {
                if (!recorder.IsOver()) {
                    recorder.CountRecorder();
                } else {
                    // 已经结束 从队列里移除
                    overRecorders.Add(recorder);
                }
            }

            // 从队列里移除 结束的recorder
            foreach (var recorder in overRecorders)
            {
                if (recorderList.Contains(recorder)) {
                    recorderList.Remove(recorder);
                }
            }
        }

    }
}