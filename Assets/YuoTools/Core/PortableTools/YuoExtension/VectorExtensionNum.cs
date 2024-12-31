using UnityEngine;

namespace YuoTools
{
    public static partial class VectorExtension
    {
        public static Vector3 _0xy(this Vector2 v) => new Vector3(0, v.x, v.y);
        public static Vector3 _0yx(this Vector2 v) => new Vector3(0, v.y, v.x);
        public static Vector3 _1xy(this Vector2 v) => new Vector3(1, v.x, v.y);
        public static Vector3 _1yx(this Vector2 v) => new Vector3(1, v.y, v.x);

        // Vector3 to Vector3

        public static Vector3 x0y(this Vector3 v) => new Vector3(v.x, 0, v.y);

        public static Vector3 x0z(this Vector3 v) => new Vector3(v.x, 0, v.z);

        public static Vector3 y0x(this Vector3 v) => new Vector3(v.y, 0, v.x);

        public static Vector3 y0z(this Vector3 v) => new Vector3(v.y, 0, v.z);

        public static Vector3 z0x(this Vector3 v) => new Vector3(v.z, 0, v.x);

        public static Vector3 z0y(this Vector3 v) => new Vector3(v.z, 0, v.y);

        public static Vector3 xy0(this Vector3 v) => new Vector3(v.x, v.y, 0);

        public static Vector3 xz0(this Vector3 v) => new Vector3(v.x, v.z, 0);

        public static Vector3 yx0(this Vector3 v) => new Vector3(v.y, v.x, 0);

        public static Vector3 yz0(this Vector3 v) => new Vector3(v.y, v.z, 0);

        public static Vector3 zx0(this Vector3 v) => new Vector3(v.z, v.x, 0);

        public static Vector3 zy0(this Vector3 v) => new Vector3(v.z, v.y, 0);

        // Vector3 to Vector3 with leading numbers
        public static Vector3 _0xy(this Vector3 v) => new Vector3(0, v.x, v.y);
        public static Vector3 _0xz(this Vector3 v) => new Vector3(0, v.x, v.z);
        public static Vector3 _0yx(this Vector3 v) => new Vector3(0, v.y, v.x);
        public static Vector3 _0yz(this Vector3 v) => new Vector3(0, v.y, v.z);
        public static Vector3 _0zx(this Vector3 v) => new Vector3(0, v.z, v.x);
        public static Vector3 _0zy(this Vector3 v) => new Vector3(0, v.z, v.y);

        public static Vector3 _1xy(this Vector3 v) => new Vector3(1, v.x, v.y);
        public static Vector3 _1xz(this Vector3 v) => new Vector3(1, v.x, v.z);
        public static Vector3 _1yx(this Vector3 v) => new Vector3(1, v.y, v.x);
        public static Vector3 _1yz(this Vector3 v) => new Vector3(1, v.y, v.z);
        public static Vector3 _1zx(this Vector3 v) => new Vector3(1, v.z, v.x);
        public static Vector3 _1zy(this Vector3 v) => new Vector3(1, v.z, v.y);


        // Vector3 to Vector4
        public static Vector4 xyz0(this Vector3 v) => new Vector4(v.x, v.y, v.z, 0);

        public static Vector4 xyz1(this Vector3 v) => new Vector4(v.x, v.y, v.z, 1);

        // Vector2 to Vector2
        public static Vector2 x0(this Vector2 v) => new Vector2(v.x, 0);

        public static Vector2 x1(this Vector2 v) => new Vector2(v.x, 1);

        public static Vector2 _0x(this Vector2 v) => new Vector2(0, v.x);

        public static Vector2 _1x(this Vector2 v) => new Vector2(1, v.x);

        public static Vector2 y0(this Vector2 v) => new Vector2(0, v.y);

        public static Vector2 y1(this Vector2 v) => new Vector2(1, v.y);

        public static Vector2 _0y(this Vector2 v) => new Vector2(0, v.y);

        public static Vector2 _1y(this Vector2 v) => new Vector2(1, v.y);

        // Vector2 to Vector3
        public static Vector3 xy0(this Vector2 v) => new Vector3(v.x, v.y, 0);

        public static Vector3 xy1(this Vector2 v) => new Vector3(v.x, v.y, 1);

        public static Vector3 xz0(this Vector2 v, float yValue) => new Vector3(v.x, yValue, v.y);

        public static Vector3 yz0(this Vector2 v, float xValue) => new Vector3(xValue, v.x, v.y);

        public static Vector3 yx0(this Vector2 v, float zValue) => new Vector3(v.y, v.x, zValue);

        public static Vector3 zx0(this Vector2 v, float yValue) => new Vector3(v.y, yValue, v.x);

        public static Vector3 zy0(this Vector2 v, float xValue) => new Vector3(xValue, v.y, v.x);

        // Vector2 to Vector4
        public static Vector4 xy00(this Vector2 v) => new Vector4(v.x, v.y, 0, 0);

        public static Vector4 xy01(this Vector2 v) => new Vector4(v.x, v.y, 0, 1);

        public static Vector4 xy10(this Vector2 v) => new Vector4(v.x, v.y, 1, 0);

        public static Vector4 xy11(this Vector2 v) => new Vector4(v.x, v.y, 1, 1);

        public static Vector4 xz00(this Vector2 v, float yValue, float wValue) => new Vector4(v.x, yValue, v.y, wValue);

        public static Vector4 yz00(this Vector2 v, float xValue, float wValue) => new Vector4(xValue, v.x, v.y, wValue);

        public static Vector4 zw00(this Vector2 v, float xValue, float yValue) => new Vector4(xValue, yValue, v.x, v.y);
    }
}