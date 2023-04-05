using System;
using UnityEngine;

public class Body {
    protected Transform transform;
    protected Turn turn;
    protected int movedFrame;
    protected bool setPositionTick;
    protected Vector3 nextPosition;

    public Vector3 Position {
        get {
            return this.transform.position;
        }
        set {
            this.setPositionTick = true;
            this.nextPosition = value;
        }
    }

    public Vector3 LatePosition {
        get;
        protected set;
    }

    public Vector3 OriginPosition {
        get;
        set;
    }

    public Quaternion Rotation {
        get {
            return this.transform.rotation;
        }
    }

    public Vector3 Direction {
        get {
            return this.turn.Direction;
        }
        set {
            this.turn.Direction = value;
        }
    }

    // Idle描述的是客观上的无位移，与是否发生过Move无关
    public bool IsIdle {
        get {
            return (this.LatePosition - this.Position).magnitude <= 0.01f;
        }
    }

    // 上一帧曾发生过Move
    public bool IsMoved {
        get {
            return Time.frameCount - this.movedFrame <= 1;
        }
    }

    public event Action<Vector3> SetPositionEvent;

    public Body(Transform transform) {
        this.transform = transform;
        this.turn = new Turn(transform);
    }

    public virtual void Start() {
        this.OriginPosition = this.Position;
        this.LatePosition = this.Position;
    }

    public virtual void Update(float dt) {
        if (this.setPositionTick) {
            this.LatePosition = this.transform.position;
            this.transform.position = this.nextPosition;
            this.SetPositionEvent?.Invoke(this.Position);
            this.setPositionTick = false;
        }
    }

    public virtual void Move(Vector3 velocity) {
        this.Position += velocity;
        this.movedFrame = Time.frameCount;
    }
}