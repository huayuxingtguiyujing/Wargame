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

        #region 与屏幕上的点 互相转换
        public virtual Point Hex_To_Pixel(Layout layout, _Hex<Number, T> hex) { return null; }

        public virtual Hex Pixel_To_Hex(Layout layout, Point point) { return null; }

        public virtual Point Hex_Corner_Offset(Layout layout, int corner) { return null; }

        public virtual List<Point> Polygon_Corners(Layout layout, Hex hex) { return null; }
        #endregion
    }

    /// <summary>
    /// 支持 浮点型 的六边形坐标结构
    /// </summary>
    public class HexagonFraction : HexFraction {
        public HexagonFraction(float q, float r, float s) : base(q, r, s) {
        }

        #region 与屏幕上的点 互相转换
        public override Point Hex_To_Pixel(Layout layout, HexFraction hex) {
            throw new NotImplementedException();
        }

        public override Hex Pixel_To_Hex(Layout layout, Point point) {
            throw new NotImplementedException();
        }
        #endregion
    }


    /// <summary>
    /// 支持 整型 的六边形坐标结构 有q r s三个维度
    /// </summary>
    public class Hexagon : Hex {

        public Hexagon(int _q, int _r, int  _s): base(_q, _r, _s){
        }

        #region 绘制边
        /// <summary>
        /// 将 HexagonFraction 类 转换 为 Hexagon
        /// </summary>
        public Hexagon Hex_Round(HexagonFraction h) {
            int q = Convert.ToInt32(h.q);
            int r = Convert.ToInt32(h.r);
            int s = Convert.ToInt32(h.s);

            double q_diff = Mathf.Abs(q - h.q);
            double r_diff = Mathf.Abs(r - h.r);
            double s_diff = Mathf.Abs(s - h.s);

            //修正值，不可少
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
        /// 六边形 插值
        /// </summary>
        public HexagonFraction Hex_Lerp(Hex a, Hex b, float t) {
            return new HexagonFraction(
                Lerp(a.q, b.q, t), 
                Lerp(a.r, b.r, t), 
                Lerp(a.s, b.s, t)
            );
        }

        /// <summary>
        /// 绘制六边形
        /// </summary>
        public List<Hexagon> Hex_LineDraw(Hex a, Hex b, float t) {
            int distance = Hex_Distance(a, b);
            List<Hexagon> results = new List<Hexagon>();

            //根据跳数 生成六边形
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

            //根据跳数 生成六边形
            double step = 1.0 / Mathf.Max(N, 1);
            for (int i = 0; i <= N; i++) {
                //报错
                //results.Add( Hex_Round(Hex_Lerp(a_nudge, b_nudge, (float)(step * i))));
            }
            return results;
        }
        
        
        #endregion

        #region 与屏幕上的点 互相转换
        /// <summary>
        /// 将 六边形数据类 转化为 像素点
        /// </summary>
        /// <returns></returns>
        public override Point Hex_To_Pixel(Layout layout, Hex hex) {
            Orientation O = layout.orientation;

            //使用 矩阵 将 Hex坐标 转为 屏幕二维坐标点
            double x = (O.f0 * hex.q + O.f1 * hex.r) * layout.Size.x;
            double y = (O.f2 * hex.q + O.f3 * hex.r) * layout.Size.y;
            return new Point(x + layout.Origin.x, y + layout.Origin.y);
        }

        /// <summary>
        /// 将 像素点 转化为 六边形数据类
        /// </summary>
        public override Hex Pixel_To_Hex(Layout layout, Point point) {
            Orientation O = layout.orientation;

            //减去原点 除去偏移值
            Point offsetPoint = new Point(
                (point.x - layout.Origin.x) / layout.Size.x,
                (point.y - layout.Origin.y) / layout.Size.y
            );

            //使用 逆矩阵 将 屏幕二维坐标点 转为 Hex坐标
            double q = O.b0 * offsetPoint.x + O.b1 * offsetPoint.y;
            double r = O.b2 * offsetPoint.x + O.b3 * offsetPoint.y;

            //类型转换
            return new Hex(Convert.ToInt32(q), Convert.ToInt32(r), Convert.ToInt32(-q - r));
        }

        /// <summary>
        /// 获取 六边形的 指定顶点
        /// </summary>
        public override Point Hex_Corner_Offset(Layout layout, int corner) {
            Point size = layout.Size;

            //获取该顶点 的 角度
            double angle = 2.0 * Mathf.PI * (layout.orientation.Start_Angle + corner) / 6;

            return new Point(
                size.x * Mathf.Cos(Convert.ToSingle(angle)),
                size.y * Mathf.Sin(Convert.ToSingle(angle))
            );
        }

        /// <summary>
        /// 获得 六边形 六个顶点的坐标
        /// </summary>
        public override List<Point> Polygon_Corners(Layout layout, Hex hex) {
            List<Point> corners = new List<Point>();

            //获取 六边形 的中心点
            Point center = Hex_To_Pixel(layout, hex);

            //生成 六边形 六个顶点的坐标
            for (int i = 0; i < 6; i++) {
                Point offset = Hex_Corner_Offset(layout, i);
                corners.Add(new Point(center.x + offset.x, center.y + offset.y));
            }
            return corners;
        }
        #endregion

        #region 坐标算术、类型转换、运算符重载
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

        #region 距离
        public int Hex_Length(Hex hex) {
            return (Mathf.Abs(hex.q) + Mathf.Abs(hex.r) + Mathf.Abs(hex.s)) / 2;
        }

        public int Hex_Distance(Hex h1, Hex h2) {
            return Hex_Length(Hex_Subtract(h1, h2));
        }
        #endregion

        #region 判断、获取邻居
        readonly List<Vector3>  Hex_Directions = new List<Vector3>{
            new Vector3(1, 0, -1), new Vector3(1, -1, 0), new Vector3(0, -1, 1),
            new Vector3(-1, 0, 1), new Vector3(-1, 1, 0), new Vector3(0, 1, -1)
        };

        public Hex Hex_Direction(int direction) {
#if UNITY_EDITOR
            Assert.IsTrue(0 <= direction && direction < 6);
#endif
            //方向需要模6
            direction = direction % 6;
            Debug.Log(direction);
            return (Hexagon)Hex_Directions[direction];
        }

        /// <summary>
        /// 获取该六边形 其中 一个方向的 邻居
        /// </summary>
        public Hex Hex_Neighbor(Hex hex, int direction) {
            return Hex_Add(hex, Hex_Direction(direction));
        }
        #endregion

        #region 判断相等
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
                //检查对象类型
                return false;
            }
            return this == (Hex)obj;
        }
        #endregion


    }



}