using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame_True.GamePlay.Politic {
    public enum FactionType {
        //�����ͣ�ϲ���⽻���裬�������丯�ܣ��⽻û���߼�
        Corruption,
        //�����ͣ����ᷢ������ս���������ڽ�������
        Defender,
        //Ͷ���ߣ�������������ǿ��ʹ���⽻���ԣ���������
        Speculators,
        //��ȡ�ͣ����������ţ��ñ��˳�Ϊ�Լ��ĸ�ӹ�������ں���������
        Enterprise,
        //Ұ���ͣ���ǿ���һ�־����������̲�ȫ����ͼ
        Ambitious
    }
}