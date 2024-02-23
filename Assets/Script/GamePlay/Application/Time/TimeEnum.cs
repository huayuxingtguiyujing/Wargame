
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WarGame_True.GamePlay.Application {

    /// <summary>
    /// ��Ϸ�е�ʱ��
    /// </summary>
    public class GameTime {

        public SerializedGameTime Time {  get; private set; }

        public void SetGameTime(SerializedGameTime gameTime) {
            this.Time = gameTime;
        }

        #region ��ǰ��ʱ�� �� �� ��
        //private int year;
        public int Year { get => Time.Year; set => Time.Year = value; }
        //private MonthEnum month;
        public MonthEnum Month { get => Time.Month; set => Time.Month = value; }
        //private int day;
        public int Day { get => Time.Day; set => Time.Day = value; }
        //private int hour;
        public int Hour { get => Time.Hour; set => Time.Hour = value; }
        #endregion


        #region ��ʱ��ɺ� �Ļص�
        public delegate void TimeCompleteCallback();
        //Сʱ�Ļص�
        TimeCompleteCallback hourCompleteCallback;
        //day�Ļص�
        TimeCompleteCallback dayCompleteCallback;
        //�»ص�
        TimeCompleteCallback monthCompleteCallback;
        //��ص�
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


        //������Ϸ�� ��ʼʱ��
        public readonly static GameTime GameStartTime = new GameTime(755, MonthEnum.December, 24, 0);

        /// <summary>
        /// ģ�� Time ������
        /// </summary>
        public void TimePass() {
            hourCompleteCallback();

            Hour++;
            int dayAdd = Hour / 24;
            Hour %= 24;
            if(dayAdd > 0) {
                //�����ս��¼�
                dayCompleteCallback();
            }

            //TODO:�����е�0�죡
            Day += dayAdd;
            int monthAdd = Day / TimeEnum.MonthDayDic[Month];
            Day %= TimeEnum.MonthDayDic[Month];
            if (Day == 0) Day = 1;
            if (monthAdd > 0) {
                //�����½��¼�
                monthCompleteCallback();
            }

            //�����·�
            int monthRec = MonthEnumToInt(Month) + monthAdd;
            int yearAdd = monthRec / 12;
            monthRec %= 12;
            Month = IntToMonthEnum(monthRec);
            if (yearAdd > 0) {
                //��������¼�
                yearCompleteCallback();
            }

            Year += yearAdd;
        }


        #region ���������������������ת������
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
    /// ר����������ͬ���� ��Ϸʱ����
    /// </summary>
    public class SerializedGameTime : INetworkSerializable, IComparable<SerializedGameTime> {

        #region ��ǰ��ʱ�� �� �� ��
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


        #region ������������������������л�����etc

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref Year);
            serializer.SerializeValue(ref Month);
            serializer.SerializeValue(ref Day);
            serializer.SerializeValue(ref Hour);
        }

        public int CompareTo(SerializedGameTime other) {
            // �Ƚ����
            int result = Year.CompareTo(other.Year);
            // �����ͬ���Ƚ��·�
            if (result == 0) {
                result = Month.CompareTo(other.Month);
                // �·���ͬ���Ƚ�����
                if (result == 0) {
                    result = Day.CompareTo(other.Day);
                    // ������ͬ���Ƚ�Сʱ
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
        /// ʮ������ ����Ӧ���ǵ������� �ֵ�ö��
        /// </summary>
        public static readonly Dictionary<MonthEnum, int> MonthDayDic = new Dictionary<MonthEnum, int>() {
            { MonthEnum.January, 31},
            { MonthEnum.February, 28}, // ����ʱΪ29
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
        /// ʱ�������ٶ� ����Ӧ���ǵ��ٶȵ� �ֵ�ö��
        /// </summary>
        public static readonly Dictionary<TimeSpeedLevel, float> LevelSpeedDic = new Dictionary<TimeSpeedLevel, float>() {
            { TimeSpeedLevel.Level1, 1.8f},
            { TimeSpeedLevel.Level2, 1.4f},
            { TimeSpeedLevel.Level3, 1.0f},
            { TimeSpeedLevel.Level4, 0.6f},
            { TimeSpeedLevel.Level5, 0.2f},
        };

        /// <summary>
        /// ʮ������ ����Ӧ���ǵ��������Ƶ� �ֵ�ö��
        /// </summary>
        public static readonly Dictionary<MonthEnum, string> MonthChineseDic = new Dictionary<MonthEnum, string>() {
            { MonthEnum.January, "һ��"},
            { MonthEnum.February, "����"},
            { MonthEnum.March, "����"},
            { MonthEnum.April, "����"},
            { MonthEnum.May, "����"},
            { MonthEnum.June, "����"},
            { MonthEnum.July, "����"},
            { MonthEnum.August, "����"},
            { MonthEnum.September, "����"},
            { MonthEnum.October, "ʮ��"},
            { MonthEnum.November, "ʮһ��"},
            { MonthEnum.December, "ʮ����"}
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