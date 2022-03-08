using UnityEngine;

public class Body : MonoBehaviour {
    public const float MIN_RANGE = 0.01f;

    public float stepOffset;

    private new BoxCollider collider;
    private new Transform transform;

    private Vector3 velocity;
    private Vector3 collided;
    private Vector3 normal;
    private EaseMove gravityMove;

    public bool IsGrounded {
        get;
        private set;
    }

    protected void Awake() {
        this.collider = this.GetComponent<BoxCollider>();
        this.transform = this.gameObject.transform;
        this.gravityMove = new EaseMove(this);
        this.normal = Vector3.up;
    }

    protected void Start() {
        var pos = this.transform.position;
        var size = this.collider.size;
        var center = pos + this.collider.center + Vector3.up * size.y;
        RaycastHit hit;
        bool ok = Physics.BoxCast(center, size, Vector3.down, out hit, Quaternion.identity, size.y);
        
        if (ok) {
            this.normal = hit.normal;
            this.IsGrounded = true;
            pos.y = hit.point.y + MIN_RANGE;
            this.transform.position = pos;
        }
    }

    protected void LateUpdate() {
        if (!this.IsGrounded && !this.gravityMove.IsRunning) {
            this.gravityMove.Enter(0, -0.035f, Vector3.down);
        }
        else if (this.IsGrounded && this.gravityMove.IsRunning) {
            this.gravityMove.Exit();
            this.gravityMove.Power = 0;
        }

        this.gravityMove.Update();

        if (this.velocity == Vector3.zero) {
            return;
        }

        var velocity = Mathf.Approximately(this.velocity.y, 0) ? Vector3.ProjectOnPlane(this.velocity, this.normal) : this.velocity;
        var direction = velocity.normalized;
        var distance = velocity.magnitude;
        var position = this.transform.position;
        var offset = this.collider.center + Vector3.up * this.stepOffset;
        var center = position + offset;
        var size = this.collider.size * 0.5f;
        RaycastHit hit;
        
        bool ok = Physics.BoxCast(center, size, direction, out hit, Quaternion.identity, distance);
        
        if (ok) {
            distance = hit.distance - MIN_RANGE;
            this.collided = position + distance * direction;
        }
        
        position += distance * direction;
        center = position + offset;
        ok = Physics.BoxCast(center, size, Vector3.down, out hit, Quaternion.identity, 100);

        if (ok) {
            this.IsGrounded = hit.distance <= this.stepOffset + MIN_RANGE;
            this.normal = hit.normal;

            if (this.IsGrounded) {
                position.y = hit.point.y + MIN_RANGE;
            }
        }
        else {
            this.IsGrounded = false;
            this.normal = Vector3.up;
        }

        this.transform.position = position;
        this.velocity = Vector3.zero;
    }

    protected void OnDrawGizmos() {
        if (!this.collider) {
            return;
        }

        var center = this.transform.position + this.collider.center;
        var size = this.collider.size;

        Gizmos.DrawSphere(this.collided, 0.1f);
    }
    
    public void Move(Vector3 velocity) {
        this.velocity += velocity;
    }
}