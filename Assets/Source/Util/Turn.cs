using System;
using UnityEngine;

public class Turn {
    private Transform transform;
    private Timer timer;
    private float angle;
    
    public event Action<Quaternion> TickEvent;

    public float Angle {
        get {
            return this.angle;
        }
        set {
            this.angle = value;
            this.AdjustDirection();
        }
    }

    public Vector3 Direction {
        get {
            return Math.AngleToDirection(this.angle);
        }
        set {
            this.Angle = Math.DirectionToAngle(value);
        }
    }

    public Turn(Transform transform) {
        this.transform = transform;
        this.SyncAngle();
    }

    public void AdjustDirection() {
        var eulerAngles = this.transform.rotation.eulerAngles;
        eulerAngles.y = this.angle;
        this.transform.rotation = Quaternion.Euler(eulerAngles);
        this.TickEvent?.Invoke(this.transform.rotation);
    }

    public void SyncAngle() {
        var eulerAngles = this.transform.rotation.eulerAngles;
        this.angle = (int)eulerAngles.y;
    }
}