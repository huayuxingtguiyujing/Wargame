using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace WarGame_True.Infrastructure.HexagonGrid.DataStruct {
    
    /// <summary>
    /// ���ڽ�Cube Coordinateת��Ϊ��Ļ���꣬�洢2x2������� 2x2�������ʼ�Ƕ�
    /// </summary>
    public class Orientation {
        // 2x2�����������任����
        public readonly double f0, f1, f2, f3;
        // 2x2�����
        public readonly double b0, b1, b2, b3;
        //�÷������ʼ��ת�Ƕ�
        public readonly double Start_Angle;

        public Orientation(double f0, double f1, double f2, double f3,
            double b0, double b1, double b2, double b3,
            double start_Angle) {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
            this.f3 = f3;
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
            Start_Angle = start_Angle;
        }

        //��������ĳ���
        public static readonly Orientation Layout_Pointy = new Orientation(
            Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0, 0.0, 3.0 / 2.0,
            Mathf.Sqrt(3.0f) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0,
            0.5);
        public static readonly Orientation Layout_Flat = new Orientation(
            3.0 / 2.0, 0.0, Mathf.Sqrt(3.0f) / 2.0, Mathf.Sqrt(3.0f),
            2.0 / 3.0, 0.0, -1.0 / 3.0, Mathf.Sqrt(3.0f) / 3.0,
            0.0);
    }

    /// <summary>
    /// ����
    /// </summary>
    public class Point {
        public readonly double x, y;

        public Point(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(Point point) {
            return new Vector2(Convert.ToSingle(point.x), Convert.ToSingle(point.y));
        }

        public static explicit operator Point(Vector2 vector) {
            return new Point(vector.x, vector.y);
        }

        public static implicit operator Vector3(Point point) {
            return new Vector3(Convert.ToSingle(point.x), Convert.ToSingle(point.y), 0);
        }

        public static explicit operator Point(Vector3 vector) {
            return new Point(vector.x, vector.y);
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    public class Layout {
        //�任���� ����ת�� Hex���� �� ��Ļ����
        public readonly Orientation orientation;
        //��Ļ �������α� �Ĵ�С
        public readonly Point Size;
        //��Ļ���ֵ�ԭ��
        public readonly Point Origin;

        public Layout(Orientation orientation, Point size, Point origin) {
            this.orientation = orientation;
            Size = size;
            Origin = origin;
        }

    }


}