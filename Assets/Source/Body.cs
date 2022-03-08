using UnityEngine;

public class Body : MonoBehaviour {
    private new BoxCollider collider;
    private new Transform transform;

    private Vector3 velocity;
    private Vector3 collided;
    private EaseMove gravityMove;

    public bool IsGrounded {
        get;
        private set;
    }

    protected void Awake() {
        this.collider = this.GetComponent<BoxCollider>();
        this.transform = this.gameObject.transform;
        this.gravityMove = new EaseMove(this);
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

        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;
        var center = this.transform.position + this.collider.center;
        var size = this.collider.size * 0.5f;
        RaycastHit hit;
        
        bool ok = Physics.BoxCast(center, size, direction, out hit, Quaternion.identity, distance);
        
        if (ok) {
            distance = hit.distance - 0.001f;
            this.collided = this.transform.position + distance * direction;
        }
        
        this.transform.position += distance * direction;
        this.velocity = Vector3.zero;

        this.IsGrounded = Physics.BoxCast(center, size, Vector3.down, out hit, Quaternion.identity, 0.01f);
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