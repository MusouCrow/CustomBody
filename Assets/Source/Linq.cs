using UnityEngine;

public static class Linq {
    public static Vector3 Mul(this Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static bool Equal(this float a, float b) {
        return Mathf.Approximately(a, b);
    }

    public static bool Equal(this Vector3 a, Vector3 b) {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
    }
}