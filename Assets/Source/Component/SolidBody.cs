using System;
using UnityEngine;

public class SolidBody : Body {
    private CharacterController controller;
    private Solid solid;
    private EaseMove gravityMove;
    private Timer rebornTimer;

    public float gravityRate = 1;

    public Vector3 GroundPosition {
        get {
            var pos = this.Position;

            return this.InGround ? pos : new Vector3(pos.x, this.solid.GroundY, pos.z);
        }
    }

    public Vector3 RebornPosition {
        get;
        private set;
    }

    public float Height {
        get {
            return this.controller.height;
        }
    }

    public float High {
        get {
            return this.InGround ? 0 : this.Position.y - this.solid.GroundY;
        }
    }

    public bool InGround {
        get {
            return this.solid.InGround;
        }
    }

    public float Gravity {
        get {
            return this.gravityMove.Power;
        }
    }

    public Vector3 Velocity {
        get {
            return this.solid.velocity;
        }
    }
    
    public SolidBody(Transform transform, CharacterController controller) : base(transform) {
        this.controller = controller;
        this.solid = new Solid(transform, controller);
        this.gravityMove = new EaseMove(this);
        this.rebornTimer = new Timer();
    }

    public override void Start() {
        base.Start();

        this.solid.FallEvent += this.Fall;
        this.solid.GroundEvent += this.OnGround;
        this.SetPositionEvent += this.Flush;

        this.solid.Start();     
        this.rebornTimer.Enter(1, this.MarkRebornPosition, true);
    }

    public override void Update(float dt) {
        this.gravityMove.Update(this.gravityRate);
        this.rebornTimer.Update(dt);
        
        if (this.solid.InGround && !this.solid.InLegal && this.solid.velocity.y <= 0) {
            var direction = Vector3.ProjectOnPlane(Vector3.down, this.solid.GroundHit.normal);
            this.Move(direction * 0.5f);
        }

        this.solid.Update();

        base.Update(dt);
    }

    public override void Move(Vector3 velocity) {
        this.solid.velocity += velocity;
    }

    public void BindFallEvent(Action OnFall) {
        this.solid.FallEvent += OnFall;
    }

    public void UnbindFallEvent(Action OnFall) {
        this.solid.FallEvent -= OnFall;
    }

    public void ClearGravity() {
        this.gravityMove.Power = 0;
    }

    private void Fall() {
        this.gravityMove.Enter(0, -0.035f, Vector3.down);
    }

    private void OnGround(RaycastHit hit) {
        this.gravityMove.Exit();
        this.gravityMove.Power = 0;
    }

    private void MarkRebornPosition() {
        if (this.solid.InGround && this.solid.InLegal) {
            this.RebornPosition = this.Position;
        }
    }

    private void Flush(Vector3 position) {
        this.solid.velocity = Vector3.zero;
        this.solid.Flush();
    }
}