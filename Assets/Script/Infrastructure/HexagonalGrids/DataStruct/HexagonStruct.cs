using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace WarGame_True.Infrastructure.HexagonGrid.DataStruct {
    using Hex = _Hex<int, int>;
    using HexFraction = _Hex<float, int>;
    using Vector3 = UnityEngine.Vector3;

    public class _Hex<Number, T> {
        public readonly Number q, r, s;

        public _Hex(Number q, Number r, Number s) {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        #region ����Ļ�ϵĵ� ����ת��
        public virtual Point Hex_To_Pixel(Layout layout, _Hex<Number, T> hex) { return null; }

        public virtual Hex Pixel_To_Hex(Layout layout, Point point) { return null; }

        public virtual Point Hex_Corner_Offset(Layout layout, int corner) { return null; }

        public virtual List<Point> Polygon_Corners(Layout layout, Hex hex) { return null; }
        #endregion
    }

    /// <summary>
    /// ֧�� ������ ������������ṹ
    /// </summary>
    public class HexagonFraction : HexFraction {
        public HexagonFraction(float q, float r, float s) : base(q, r, s) {
        }

        #region ����Ļ�ϵĵ� ����ת��
        public override Point Hex_To_Pixel(Layout layout, HexFraction hex) {
            throw new NotImplementedException();
        }

        public override Hex Pixel_To_Hex(Layout layout, Point point) {
            throw new NotImplementedException();
        }
        #endregion
    }


    /// <summary>
    /// ֧�� ���� ������������ṹ ��q r s����ά��
    /// </summary>
    public class Hexagon : Hex {

        public Hexagon(int _q, int _r, int  _s): base(_q, _r, _s){
        }

        #region ���Ʊ�
        /// <summary>
        /// �� HexagonFraction �� ת�� Ϊ Hexagon
        /// </summary>
        public Hexagon Hex_Round(HexagonFraction h) {
            int q = Convert.ToInt32(h.q);
            int r = Convert.ToInt32(h.r);
            int s = Convert.ToInt32(h.s);

            double q_diff = Mathf.Abs(q - h.q);
            double r_diff = Mathf.Abs(r - h.r);
            double s_diff = Mathf.Abs(s - h.s);

            //����ֵ��������
            if (q_diff > r_diff && q_diff > s_diff) {
                q = -r - s;
            } else if (r_diff > s_diff) {
                r = -q - s;
            } else {
                s = -q - r;
            }
            return new Hexagon(q, r, s);
        }

        public float Lerp(float a, float b, float t) {
            return a * (1 - t) + b * t;
        }

        /// <summary>
        /// ������ ��ֵ
        /// </summary>
        public HexagonFraction Hex_Lerp(Hex a, Hex b, float t) {
            return new HexagonFraction(
                Lerp(a.q, b.q, t), 
                Lerp(a.r, b.r, t), 
                Lerp(a.s, b.s, t)
            );
        }

        /// <summary>
        /// ����������
        /// </summary>
        public List<Hexagon> Hex_LineDraw(Hex a, Hex b, float t) {
            int distance = Hex_Distance(a, b);
            List<Hexagon> results = new List<Hexagon>();

            //�������� ����������
            float step = 1.0f / Mathf.Max(distance, 1);

            for (int i = 0; i <= distance; i++) {
                results.Add(Hex_Round(Hex_Lerp(a, b, step * i)));
            }

            return results;
        }

        public List<Hexagon> Hex_Linedraw(Hex a, Hex b) {
            int N = Hex_Distance(a, b);
            //
            HexagonFraction a_nudge = new HexagonFraction((float)(a.q +1e-6), (float)(a.r + 1e-6), (float)(a.s - 2e-6));
            HexagonFraction b_nudge = new HexagonFraction((float)(b.q +1e-6), (float)(b.r + 1e-6), (float)(b.s - 2e-6));
            
            List<Hexagon> results = new List<Hexagon>();

            //�������� ����������
            double step = 1.0 / Mathf.Max(N, 1);
            for (int i = 0; i <= N; i++) {
                //����
                //results.Add( Hex_Round(Hex_Lerp(a_nudge, b_nudge, (float)(step * i))));
            }
            return results;
        }
        
        
        #endregion

        #region ����Ļ�ϵĵ� ����ת��
        /// <summary>
        /// �� ������������ ת��Ϊ ���ص�
        /// </summary>
        /// <returns></returns>
        public override Point Hex_To_Pixel(Layout layout, Hex hex) {
            Orientation O = layout.orientation;

            //ʹ�� ���� �� Hex���� תΪ ��Ļ��ά�����
            double x = (O.f0 * hex.q + O.f1 * hex.r) * layout.Size.x;
            double y = (O.f2 * hex.q + O.f3 * hex.r) * layout.Size.y;
            return new Point(x + layout.Origin.x, y + layout.Origin.y);
        }

        /// <summary>
        /// �� ���ص� ת��Ϊ ������������
        /// </summary>
        public override Hex Pixel_To_Hex(Layout layout, Point point) {
            Orientation O = layout.orientation;

            //��ȥԭ�� ��ȥƫ��ֵ
            Point offsetPoint = new Point(
                (point.x - layout.Origin.x) / layout.Size.x,
                (point.y - layout.Origin.y) / layout.Size.y
            );

            //ʹ�� ����� �� ��Ļ��ά����� תΪ Hex����
            double q = O.b0 * offsetPoint.x + O.b1 * offsetPoint.y;
            double r = O.b2 * offsetPoint.x + O.b3 * offsetPoint.y;

            //����ת��
            return new Hex(Convert.ToInt32(q), Convert.ToInt32(r), Convert.ToInt32(-q - r));
        }

        /// <summary>
        /// ��ȡ �����ε� ָ������
        /// </summary>
        public override Point Hex_Corner_Offset(Layout layout, int corner) {
            Point size = layout.Size;

            //��ȡ�ö��� �� �Ƕ�
            double angle = 2.0 * Mathf.PI * (layout.orientation.Start_Angle + corner) / 6;

            return new Point(
                size.x * Mathf.Cos(Convert.ToSingle(angle)),
                size.y * Mathf.Sin(Convert.ToSingle(angle))
            );
        }

        /// <summary>
        /// ��� ������ �������������
        /// </summary>
        public override List<Point> Polygon_Corners(Layout layout, Hex hex) {
            List<Point> corners = new List<Point>();

            //��ȡ ������ �����ĵ�
            Point center = Hex_To_Pixel(layout, hex);

            //���� ������ �������������
            for (int i = 0; i < 6; i++) {
                Point offset = Hex_Corner_Offset(layout, i);
                corners.Add(new Point(center.x + offset.x, center.y + offset.y));
            }
            return corners;
        }
        #endregion

        #region ��������������ת�������������
        private Hex Hex_Add(Hex h1, Hex h2) {
            return new Hex(h1.q + h2.q, h1.r + h2.r, h1.s + h2.s);
        }

        public static Hex operator +(Hexagon h1, Hex h2) {
            return new Hex(h1.q + h2.q, h1.r + h2.r, h1.s + h2.s);
        }

        public Hex Hex_Subtract(Hex h1, Hex h2) {
            return new Hex(h1.q - h2.q, h1.r - h2.r, h1.s - h2.s);
        }

        public static Hex operator -(Hexagon h1, Hex h2) {
            return new Hex(h1.q - h2.q, h1.r - h2.r, h1.s - h2.s);
        }

        public Hex Hex_Multiply(Hex h1, Hex h2) {
            return new Hex(h1.q * h2.q, h1.r * h2.r, h1.s * h2.s);
        }

        public static Hex operator *(Hexagon h1, Hex h2) {
            return new Hex(h1.q * h2.q, h1.r * h2.r, h1.s * h2.s);
        }

        public static implicit operator Vector3(Hexagon hex) {
            return new Vector3(hex.q, hex.r, hex.s);
        }

        public static explicit operator Hexagon(Vector3 vector) {
            return new Hexagon((int)vector.x, (int)vector.y, (int)vector.z);
        }
        #endregion

        #region ����
        public int Hex_Length(Hex hex) {
            return (Mathf.Abs(hex.q) + Mathf.Abs(hex.r) + Mathf.Abs(hex.s)) / 2;
        }

        public int Hex_Distance(Hex h1, Hex h2) {
            return Hex_Length(Hex_Subtract(h1, h2));
        }
        #endregion

        #region �жϡ���ȡ�ھ�
        readonly List<Vector3>  Hex_Directions = new List<Vector3>{
            new Vector3(1, 0, -1), new Vector3(1, -1, 0), new Vector3(0, -1, 1),
            new Vector3(-1, 0, 1), new Vector3(-1, 1, 0), new Vector3(0, 1, -1)
        };

        public Hex Hex_Direction(int direction) {
#if UNITY_EDITOR
            Assert.IsTrue(0 <= direction && direction < 6);
#endif
            //������Ҫģ6
            direction = direction % 6;
            Debug.Log(direction);
            return (Hexagon)Hex_Directions[direction];
        }

        /// <summary>
        /// ��ȡ�������� ���� һ������� �ھ�
        /// </summary>
        public Hex Hex_Neighbor(Hex hex, int direction) {
            return Hex_Add(hex, Hex_Direction(direction));
        }
        #endregion

        #region �ж����
        public static bool operator ==(Hexagon h1, Hex h2) {
            return (h1.q == h2.q) && (h1.r == h2.r) && (h1.s == h2.s);
        }

        public static bool operator !=(Hexagon h1, Hex h2) {
            return !(h1 == h2);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is Hex)) {
                //����������
                return false;
            }
            return this == (Hex)obj;
        }
        #endregion


    }



}