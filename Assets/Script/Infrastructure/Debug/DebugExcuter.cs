using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.Infrastructure.DebugPack {
    /// <summary>
    /// debug ָ��ָʾ��, ����ִ�������debug����
    /// </summary>
    public class DebugExcuter {

        string curError = "";

        public string GetErrorLog() {
            return curError;
        }

        public void SetErrorLog(string theValue) {
            curError = theValue;
        }

        /// <summary>
        /// ִ��ָ��
        /// </summary>
        /// <param name="commandStr"></param>
        /// <returns></returns>
        public bool ExcuteCommand(string commandStr) {

            // �������� ƥ������
            Command commands = GetCommands(commandStr);
            if (commands != null) {
                commands.Excute();
                return true;
            } else {
                return false;
            }

        }

        private Command GetCommands(string commandStr) {

            string[] CommandParts = commandStr.Split(" ");
            SetErrorLog(null);
            
            if(CommandParts.Length < 1) {
                // ��Ч����
                SetErrorLog("not valid input!");
                return null;
            }

            // �����������
            switch(CommandParts[0]) {
                case "army":
                    return GetArmyCommand(CommandParts);
                case "province":
                    return GetProvinceCommand(CommandParts);
                case "faction":
                    return GetFactionCommand(CommandParts);
                default:
                    SetErrorLog("not a valid command!");
                    return null;
            }

        }

        private Command GetArmyCommand(string[] CommandParts) {
            //1������ֵ���ͣ�== ��equals�ȼۣ����ǱȽϴ洢��Ϣ�����ݡ�
            //2�������������ͣ�== �Ƚϵ�������������ջ�еĵ�ַ��equals������Ƚϵ��������������йܶ��еĴ洢��Ϣ�����ݡ�
            //3������string��Ҫ���⴦��
            //����һ���ڲ��Ѿ��������equals������ == ���࣬�� == ��equals�ȼۣ����ǱȽϴ洢��Ϣ�����ݡ�
            //4������һЩ�Զ�����࣬�����б�Ҫ����equals������������Ĭ��Ϊ�����equals����
            //(����û������Equals������Ϊ Object���е�Equals����)���ȽϷ�ʽҲΪ��ַ
            //�����������������йܶ��еĴ洢��Ϣ�����ݡ������ǾͲ������
            if (!CommandParts[0].Equals("army")) {
                SetErrorLog("not a army command!");
                return null;
            }

            int inputNum = CommandParts.Length;
            
            switch (CommandParts[1]) {
                case "create":
                    if(inputNum < 5) {
                        SetErrorLog("create army command: not enough parameter!");
                        return null;
                    }
                    try {
                        // ��ȡ�������ӵı�Ҫ�������
                        int createNum = int.Parse(CommandParts[2]);
                        int createProvinceID = int.Parse(CommandParts[4]);
                        string createUnitName = "";
                        // �������˴������ӵ����� ����������
                        if (inputNum > 6) createUnitName = CommandParts[5];
                        return new ArmyCommand(0, createNum, createProvinceID, ArmyCommandType.Create, createUnitName);
                    } catch {
                        SetErrorLog("error when creating army!");
                        return null;
                    }
                case "remove":
                    if(inputNum < 4) {
                        SetErrorLog("remove army command: not enough parameter!");
                        return null;
                    }
                    try {
                        string removeMethod = CommandParts[2];
                        ulong handleID = ulong.Parse(CommandParts[3]);
                        if(removeMethod == "-id") {
                            // NOTICE: �Ƴ�ָ��id�ľ��� ��������ģʽ����ʹ��
                            if (!NetworkManager.Singleton.IsClient) {
                                SetErrorLog("this command can only be excute in online!");
                                return null;
                            }
                            return new ArmyCommand(handleID, -1, -1, ArmyCommandType.Remove, "");
                        } else if (removeMethod == "-province") {
                            return new ArmyCommand(0, -1, (int)handleID, ArmyCommandType.Remove, "");
                        } else {
                            return null;
                        }
                        
                    } catch {
                        SetErrorLog("error when removing army!");
                        return null;
                    }
                    break;
                case "recruit":
                    if (inputNum < 5) {
                        SetErrorLog("recruit army command: not enough parameter!");
                        return null;
                    }
                    try {
                        // ��ȡ��ļ���ӵı�Ҫ�������
                        int createNum = int.Parse(CommandParts[2]);
                        int createProvinceID = int.Parse(CommandParts[4]);
                        string createUnitName = "";
                        // ����������ļ���ӵ����� ����������
                        if (inputNum > 6) createUnitName = CommandParts[5];
                        return new ArmyCommand(
                            0, createNum, createProvinceID,
                            ArmyCommandType.Recruit, createUnitName
                        );
                    } catch {
                        SetErrorLog("error when recruiting army!");
                        return null;
                    }
            }
            return null;

        }

        private Command GetProvinceCommand(string[] CommandParts) {
            if (!CommandParts[0].Equals("province")) {
                SetErrorLog("not a province command!");
                return null;
            }
            return null;
        }

        private Command GetFactionCommand(string[] CommandParts) {
            if (!CommandParts[0].Equals("faction")) {
                SetErrorLog("not a faction command!");
                return null;
            }
            return null;
        }

    }
}

