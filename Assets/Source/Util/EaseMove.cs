using System;
using UnityEngine;

public class EaseMove : Gear {
    private Body body;
    private Ease ease;

    public Action OnExit;
    public Vector3 direction;
    public bool safe;
    public float Power {
        get {
            return this.ease.current;
        }
        set {
            this.ease.current = value;
        }
    }

    public float Speed {
        get {
            return this.ease.speed;
        }
        set {
            this.ease.speed = value;
        }
    }

    public float Progress {
        get {
            return this.ease.Progress;
        }
    }

    public EaseMove(Body body) {
        this.body = body;
        this.ease = new Ease();
    }

    public void Update(float rate=1) {
        if (!this.IsRunning) {
            return;
        }

        this.ease.Update(rate);

        if (this.ease.IsRunning) {
            var fwd = this.GetForward(rate);
            this.body.Move(fwd);
        }
        else {
            this.Exit();
        }
    }

    public void Enter(float power, float speed, Vector3 direction, bool safe=false, Action OnExit=null) {
        base.Enter();

        this.ease.Enter(power, 0, speed);
        this.direction = direction.normalized;
        this.safe = safe;
        this.OnExit = OnExit == null ? this.OnExit : OnExit;
    }

    public override void Exit() {
        base.Exit();

        if (this.OnExit != null) {
            this.OnExit();
        }
    }

    public void Exit(bool slient) {
        base.Exit();

        if (!slient && this.OnExit != null) {
            this.OnExit();
        }
    }

    private Vector3 GetForward(float rate) {
        return this.direction * this.ease.current * rate;
    }
}