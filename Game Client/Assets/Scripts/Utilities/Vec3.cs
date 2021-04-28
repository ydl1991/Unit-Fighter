using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vec3
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    // -----------------------------------------------------------------
    // Summary:
    //     Constructors
    //
    public Vec3(float nx, float ny) { x = nx; y = ny; z = 0; }
    public Vec3(float nx, float ny, float nz) { x = nx; y = ny; z = nz; }

    // -----------------------------------------------------------------
    // Summary:
    //     Static Getters
    //
    public static Vec3 zero { get { return new Vec3(); } }
    public static Vec3 one { get { return new Vec3(1, 1, 1); } }
    public static Vec3 up { get { return new Vec3(0, 1, 0); } }
    public static Vec3 down { get { return new Vec3(0, -1, 0); } }
    public static Vec3 left { get { return new Vec3(-1, 0, 0); } }
    public static Vec3 right { get { return new Vec3(1, 0, 0); } }
    public static Vec3 forward { get { return new Vec3(0, 0, 1); } }
    public static Vec3 back { get { return new Vec3(0, 0, -1); } }

    // -----------------------------------------------------------------
    // Summary:
    //     Casting Function
    //
    public static Vec3 ToVec3(Vector3 vec) { return new Vec3(vec.x, vec.y, vec.z); }
    public static Vector3 ToVector3(Vec3 vec) { return new Vector3(vec.x, vec.y, vec.z); }
    public Vector3 ToVector3() { return new Vector3(x, y, z); }

    // -----------------------------------------------------------------
    // Summary:
    //     Magnitude Function
    //
    public float SqrMagnitude() { return x * x + y * y + z * z; }
    public float Magnitude() { return Mathf.Sqrt(SqrMagnitude()); }

    // -----------------------------------------------------------------
    // Summary:
    //     Normalize Function
    //
    public void Normalize() 
    {
        float mag = Magnitude();
        x /= mag;
        y /= mag;
        z /= mag;
    }

    public Vec3 Normalized()
    {
        float mag = Magnitude();
        return new Vec3(x / mag, y / mag, z / mag);
    }

    // -----------------------------------------------------------------
    // Summary:
    //     Dot Product Function
    //
    public float Dot(Vec3 vec) { return x * vec.x + y * vec.y + z * vec.z; }
    public float Dot(Vector3 vec) { return x * vec.x + y * vec.y + z * vec.z; }

    // -----------------------------------------------------------------
    // Summary:
    //     Cross Product Function
    // 
    public Vec3 Cross(Vec3 vec) { return new Vec3(y * vec.z - z * vec.y, z * vec.x - x * vec.z, x * vec.y - y * vec.x); }
    public Vec3 Cross(Vector3 vec) { return new Vec3(y * vec.z - z * vec.y, z * vec.x - x * vec.z, x * vec.y - y * vec.x); }

    // -----------------------------------------------------------------
    // Summary:
    //     Angle Function
    //
    public static float Angle(Vec3 from, Vec3 to) { return Mathf.Acos(from.Dot(to) / (from.Magnitude() * to.Magnitude())); }
    public static float CosAngle(Vec3 from, Vec3 to) { return from.Dot(to) / (from.Magnitude() * to.Magnitude()); }

    // -----------------------------------------------------------------
    // Summary:
    //     Distance Function
    //
    public static float SqrDistance(Vec3 v1, Vec3 v2) 
    { 
        return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
    }
    public static float Distance(Vec3 v1, Vec3 v2) { return Mathf.Sqrt(SqrDistance(v1, v2)); }

    // -----------------------------------------------------------------
    // Summary:
    //     Lerp Function
    //
    public static Vec3 Lerp(Vec3 start, Vec3 end, float t) { return start * t + end * (1 - t); }

    // -----------------------------------------------------------------
    // Summary:
    //     Operation Overloading
    //
    public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z); }
    public static Vec3 operator -(Vec3 a) { return -1 * a; }
    public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z); }
    public static Vec3 operator *(float d, Vec3 a) { return new Vec3(a.x * d, a.y * d, a.z * d); }
    public static Vec3 operator *(Vec3 a, float d) { return new Vec3(a.x * d, a.y * d, a.z * d); }
    public static Vec3 operator *(Vec3 a, Vec3 b) { return new Vec3(a.x * b.x, a.y * b.y, a.z * b.z); }
    public static Vec3 operator /(Vec3 a, float d) { return new Vec3(a.x / d, a.y / d, a.z / d); }
    public static bool operator ==(Vec3 lhs, Vec3 rhs) { return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z; }
    public static bool operator !=(Vec3 lhs, Vec3 rhs) { return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z; }

    public override bool Equals(object other) { return (other is Vec3) && Equals((Vec3)other); }
    public bool Equals(Vec3 other) { return other.x == x && other.y == y && other.z == z; }
    public override int GetHashCode() { return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2); }
}
