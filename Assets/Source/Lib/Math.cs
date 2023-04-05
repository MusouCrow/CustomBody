using UnityEngine;

public static class Math {
    public static int DirectionToAngle(Vector3 direction) {
        var rot = Quaternion.LookRotation(direction);
        int angle = (int)rot.eulerAngles.y;
        
        return angle;
    }

    public static Vector3 AngleToDirection(float angle) {
        Vector3 direction = Vector3.zero;
        angle = (360 + angle) % 360;
        direction = new Vector3(0, 0, 1);
        
        var rot = Quaternion.Euler(0, angle, 0);

        return rot * direction;
    }
}