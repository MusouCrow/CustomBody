using System;
using UnityEngine;

public class EaseMove : IGear {
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

    public bool IsRunning {
        get;
        protected set;
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
        this.IsRunning = true;
        
        this.ease.Enter(power, 0, speed);
        this.direction = direction.normalized;
        this.safe = safe;
        this.OnExit = OnExit == null ? this.OnExit : OnExit;
    }

    public void Exit() {
        this.IsRunning = false;
        this.OnExit?.Invoke();
    }

    private Vector3 GetForward(float rate) {
        var fwd = this.direction * this.ease.current * rate;
        /*
        if (this.safe && this.direction.y.Equal(0)) {
            var posA = this.mover.GroundPosition;
            var posB = posA + fwd + this.direction * 0.5f;
            UnityEngine.AI.NavMeshHit hit;

            bool bound = UnityEngine.AI.NavMesh.Raycast(posA, posB, out hit, UnityEngine.AI.NavMesh.AllAreas);

            if (bound) {
                var distance = hit.distance - 0.5f;
                distance = distance < 0 ? 0 : distance;

                fwd = this.direction * distance;
                this.ease.Exit();
            }
        }
        */
        return fwd;
    }
}