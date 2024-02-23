using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WarGame_True.GamePlay.ArmyPart;

namespace WarGame_True.Infrastructure.DebugPack {
    /// <summary>
    /// debug 指令指示器, 用于执行输入的debug命令
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
        /// 执行指令
        /// </summary>
        /// <param name="commandStr"></param>
        /// <returns></returns>
        public bool ExcuteCommand(string commandStr) {

            // 根据输入 匹配命令
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
                // 无效输入
                SetErrorLog("not valid input!");
                return null;
            }

            // 区分命令类别
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
            //1、对于值类型，== 和equals等价，都是比较存储信息的内容。
            //2、对于引用类型，== 比较的是引用类型在栈中的地址，equals方法则比较的是引用类型在托管堆中的存储信息的内容。
            //3、对于string类要特殊处理，
            //它是一个内部已经处理好了equals方法和 == 的类，故 == 和equals等价，都是比较存储信息的内容。
            //4、对于一些自定义的类，我们有必要重载equals方法，否则它默认为基类的equals方法
            //(基类没有重载Equals方法则为 Object类中的Equals方法)，比较方式也为地址
            //而不是引用类型在托管堆中的存储信息的内容。故我们就不难理解
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
                        // 获取创建军队的必要输入参数
                        int createNum = int.Parse(CommandParts[2]);
                        int createProvinceID = int.Parse(CommandParts[4]);
                        string createUnitName = "";
                        // 若输入了创建军队的类型 则加入参数中
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
                            // NOTICE: 移除指定id的军队 仅在联网模式可以使用
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
                        // 获取招募军队的必要输入参数
                        int createNum = int.Parse(CommandParts[2]);
                        int createProvinceID = int.Parse(CommandParts[4]);
                        string createUnitName = "";
                        // 若输入了招募军队的类型 则加入参数中
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

