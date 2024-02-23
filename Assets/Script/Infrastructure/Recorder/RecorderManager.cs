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
            // ����ÿ��recorder ������м�ʱ
            foreach (BaseRecorder recorder in recorderList)
            {
                if (!recorder.IsOver()) {
                    recorder.CountRecorder();
                } else {
                    // �Ѿ����� �Ӷ������Ƴ�
                    overRecorders.Add(recorder);
                }
            }

            // �Ӷ������Ƴ� ������recorder
            foreach (var recorder in overRecorders)
            {
                if (recorderList.Contains(recorder)) {
                    recorderList.Remove(recorder);
                }
            }
        }

    }
}