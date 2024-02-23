
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.GamePlay.Application {

    /// <summary>
    /// 游戏中的时间
    /// </summary>
    public class GameTime {

        public SerializedGameTime Time {  get; private set; }

        public void SetGameTime(SerializedGameTime gameTime) {
            this.Time = gameTime;
        }

        #region 当前的时间 年 月 日
        //private int year;
        public int Year { get => Time.Year; set => Time.Year = value; }
        //private MonthEnum month;
        public MonthEnum Month { get => Time.Month; set => Time.Month = value; }
        //private int day;
        public int Day { get => Time.Day; set => Time.Day = value; }
        //private int hour;
        public int Hour { get => Time.Hour; set => Time.Hour = value; }
        #endregion


        #region 计时完成后 的回调
        public delegate void TimeCompleteCallback();
        //小时的回调
        TimeCompleteCallback hourCompleteCallback;
        //day的回调
        TimeCompleteCallback dayCompleteCallback;
        //月回调
        TimeCompleteCallback monthCompleteCallback;
        //年回调
        TimeCompleteCallback yearCompleteCallback; 
        #endregion

        public GameTime(int year, MonthEnum month, int day, int hour) {
            Time = new SerializedGameTime(year, month, day, hour);
            //gameTime.Year = year;
            //gameTime.Month = month;
            //gameTime.Day = day;
            //gameTime.Hour = hour;
        }

        public void RegisterCallback(TimeCompleteCallback hourCompleteCallback, TimeCompleteCallback dayCompleteCallback, 
            TimeCompleteCallback monthCompleteCallback, TimeCompleteCallback yearCompleteCallback) {
            this.hourCompleteCallback = hourCompleteCallback;
            this.dayCompleteCallback = dayCompleteCallback;
            this.monthCompleteCallback = monthCompleteCallback;
            this.yearCompleteCallback = yearCompleteCallback;
        }


        //整个游戏的 起始时间
        public readonly static GameTime GameStartTime = new GameTime(755, MonthEnum.December, 24, 0);

        /// <summary>
        /// 模拟 Time 的流逝
        /// </summary>
        public void TimePass() {
            hourCompleteCallback();

            Hour++;
            int dayAdd = Hour / 24;
            Hour %= 24;
            if(dayAdd > 0) {
                //触发日结事件
                dayCompleteCallback();
            }

            //TODO:天数有第0天！
            Day += dayAdd;
            int monthAdd = Day / TimeEnum.MonthDayDic[Month];
            Day %= TimeEnum.MonthDayDic[Month];
            if (Day == 0) Day = 1;
            if (monthAdd > 0) {
                //触发月结事件
                monthCompleteCallback();
            }

            //计算月份
            int monthRec = MonthEnumToInt(Month) + monthAdd;
            int yearAdd = monthRec / 12;
            monthRec %= 12;
            Month = IntToMonthEnum(monthRec);
            if (yearAdd > 0) {
                //触发年结事件
                yearCompleteCallback();
            }

            Year += yearAdd;
        }


        #region 重载运算符、算术操作、转化操作
        public static int MonthEnumToInt(MonthEnum month) {
            return (int)month;
        }

        public static MonthEnum IntToMonthEnum(int month) {
            month %= 12;
            return (MonthEnum)month;
        }

        public static bool operator ==(GameTime time1, GameTime time2) {
            return (time1.Time == time2.Time);
        }
        public static bool operator !=(GameTime time1, GameTime time2) {
            return !(time1 == time2);
        }

        #endregion
    }

    /// <summary>
    /// 专门用于网络同步的 游戏时间类
    /// </summary>
    public class SerializedGameTime : INetworkSerializable, IComparable<SerializedGameTime> {

        #region 当前的时间 年 月 日
        public int Year;

        public MonthEnum Month;

        public int Day;

        public int Hour;
        #endregion

        public SerializedGameTime() { }

        public SerializedGameTime(int year, MonthEnum month, int day, int hour) {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
        }


        #region 重载运算符、算术操作、序列化操作etc

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref Year);
            serializer.SerializeValue(ref Month);
            serializer.SerializeValue(ref Day);
            serializer.SerializeValue(ref Hour);
        }

        public int CompareTo(SerializedGameTime other) {
            // 比较年份
            int result = Year.CompareTo(other.Year);
            // 年份相同，比较月份
            if (result == 0) {
                result = Month.CompareTo(other.Month);
                // 月份相同，比较日期
                if (result == 0) {
                    result = Day.CompareTo(other.Day);
                    // 日期相同，比较小时
                    if (result == 0) {
                        result = Hour.CompareTo(other.Hour);
                    }
                }
            }
            return result;
        }

        public static bool operator ==(SerializedGameTime time1, SerializedGameTime time2) {
            return (time1.Year == time2.Year)
                && (time1.Month == time2.Month)
                && (time1.Day == time2.Day)
                && (time1.Hour == time2.Hour);
        }
        
        public static bool operator !=(SerializedGameTime time1, SerializedGameTime time2) {
            return !(time1 == time2);
        }

        public static bool operator >(SerializedGameTime time1, SerializedGameTime time2) {
            return time1.CompareTo(time2) < 0;
        }

        public static bool operator <(SerializedGameTime time1, SerializedGameTime time2) {
            return time1.CompareTo(time2) > 0;
        }
        #endregion
    }

    public class TimeEnum {

        /// <summary>
        /// 十二个月 及对应它们的天数的 字典枚举
        /// </summary>
        public static readonly Dictionary<MonthEnum, int> MonthDayDic = new Dictionary<MonthEnum, int>() {
            { MonthEnum.January, 31},
            { MonthEnum.February, 28}, // 闰年时为29
            { MonthEnum.March, 31},
            { MonthEnum.April, 30},
            { MonthEnum.May, 31},
            { MonthEnum.June, 30},
            { MonthEnum.July, 31},
            { MonthEnum.August, 31},
            { MonthEnum.September, 30},
            { MonthEnum.October, 31},
            { MonthEnum.November, 30},
            { MonthEnum.December, 31}
        };

        /// <summary>
        /// 时间流逝速度 及对应它们的速度的 字典枚举
        /// </summary>
        public static readonly Dictionary<TimeSpeedLevel, float> LevelSpeedDic = new Dictionary<TimeSpeedLevel, float>() {
            { TimeSpeedLevel.Level1, 1.8f},
            { TimeSpeedLevel.Level2, 1.4f},
            { TimeSpeedLevel.Level3, 1.0f},
            { TimeSpeedLevel.Level4, 0.6f},
            { TimeSpeedLevel.Level5, 0.2f},
        };

        /// <summary>
        /// 十二个月 及对应它们的中文名称的 字典枚举
        /// </summary>
        public static readonly Dictionary<MonthEnum, string> MonthChineseDic = new Dictionary<MonthEnum, string>() {
            { MonthEnum.January, "一月"},
            { MonthEnum.February, "二月"},
            { MonthEnum.March, "三月"},
            { MonthEnum.April, "四月"},
            { MonthEnum.May, "五月"},
            { MonthEnum.June, "六月"},
            { MonthEnum.July, "七月"},
            { MonthEnum.August, "八月"},
            { MonthEnum.September, "九月"},
            { MonthEnum.October, "十月"},
            { MonthEnum.November, "十一月"},
            { MonthEnum.December, "十二月"}
        };
    }

    public enum MonthEnum {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum TimeSpeedLevel {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
    }
}