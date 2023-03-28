using UnityEngine;

public class Entity : MonoBehaviour {    
    private EaseMove gravityMove;
    private ControlMove controlMove;
    private Timer groundTimer;

    public KinematicBody Body {
        get;
        private set;
    }

    public Vector3 RebornPosition {
        get;
        private set;
    }

    protected void Awake() {
        var collider = this.GetComponent<CapsuleCollider>();
        this.Body = new KinematicBody(this.transform, collider);

        this.gravityMove = new EaseMove(this.Body);
        this.controlMove = new ControlMove(this.Body, 0.1f);
        this.groundTimer = new Timer();
    }

    protected void Start() {
        this.groundTimer.Enter(1, this.FlushRebornPosition, true);
        this.FlushRebornPosition();
    }

    protected void FixedUpdate() {
        if (!this.Body.InGround && !this.gravityMove.IsRunning) {
            this.gravityMove.Enter(0, -0.035f, Vector3.down);
        }
        else if (this.Body.InGround && this.gravityMove.IsRunning) {
            this.gravityMove.Exit();
        }

        this.gravityMove.Update();
        this.controlMove.Update();
        this.groundTimer.Update(Time.deltaTime);
    }
    
    protected void LateUpdate() {
        this.Body.LateUpdate();
    }

    protected void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            return;
        }

        this.Body.DrawGizmos();
    }

    private void FlushRebornPosition() {
        this.RebornPosition = this.Body.LegalGroundPosition;
    }
}