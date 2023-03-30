using UnityEngine;

public class Entity : MonoBehaviour {    
    private EaseMove gravityMove;
    private EaseMove flightMove;
    private ControlMove controlMove;
    private Timer groundTimer;

    public float gravityRate = 1;

    public IBody Body {
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

        var mover = this.Body as IMover;
        this.gravityMove = new EaseMove(mover);
        this.flightMove = new EaseMove(mover);
        this.controlMove = new ControlMove(mover, 0.1f);
        this.groundTimer = new Timer();
    }

    protected void Start() {
        this.groundTimer.Enter(1, this.FlushRebornPosition, true);
        this.FlushRebornPosition();
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && this.Body.InGround) {
            this.Flight(0.45f, 0.035f);
        }
    }

    protected void FixedUpdate() {
        if (!this.Body.InGround && !this.gravityMove.IsRunning) {
            this.gravityMove.Enter(0, -0.035f, Vector3.down);
        }
        else if (this.Body.InGround && this.gravityMove.IsRunning) {
            this.gravityMove.Exit();
        }

        this.gravityMove.Update(this.gravityRate);
        this.flightMove.Update();
        this.groundTimer.Update(Time.fixedDeltaTime);
        this.controlMove.Update();

        if (this.Body.InGround && !this.Body.InLegalGround && this.Body.Velocity.y <= 0) {
            this.Body.Move(Vector3.down * 0.3f);
        }

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

    private void OnFlightEnd() {
        this.gravityRate = 1;
    }

    public void Flight(float power, float speed) {
        this.flightMove.Enter(power, speed, Vector3.up, false, this.OnFlightEnd);
        this.gravityRate = 0;
    }
}