using UnityEngine;

public class Body : MonoBehaviour {
    public const float MIN_RANGE = 0.01f;

    public float stepOffset = 0.02f;
    public float angleLimit = 45;

    private new BoxCollider collider;
    private new Transform transform;

    private Vector3 velocity;
    private Vector3 normal;
    private EaseMove gravityMove;
    private float groundY;

    public float Gravity {
        get {
            return this.gravityMove.Power;
        }
    }
    
    public bool IsGrounded {
        get;
        private set;
    }

    public float GroundY {
        get {
            return this.IsGrounded ? this.transform.position.y : this.groundY;
        }
    }

    public float High {
        get {
            return this.transform.position.y - this.GroundY;
        }
    }

    public Vector3 GroundPosition {
        get {
            var pos = this.transform.position;
            pos.y = this.GroundY;
            
            return pos;
        }
    }

    public Vector3 LatePosition {
        get;
        protected set;
    }

    public Vector3 OriginPosition {
        get;
        protected set;
    }

    public Vector3 LegalPosition {
        get;
        protected set;
    }

    protected void Awake() {
        this.collider = this.GetComponent<BoxCollider>();
        this.transform = this.gameObject.transform;
        this.gravityMove = new EaseMove(this);
        this.normal = Vector3.up;
    }

    protected void Start() {
        this.LegalPosition = this.transform.position;
        this.AdjustPosition();
        this.OriginPosition = this.transform.position;
    }

    protected void FixedUpdate() {
        if (!this.IsGrounded && !this.gravityMove.IsRunning) {
            this.gravityMove.Enter(0, -0.035f, Vector3.down);
        }
        else if (this.IsGrounded && this.gravityMove.IsRunning) {
            this.gravityMove.Exit();
            this.gravityMove.Power = 0;
        }

        this.gravityMove.Update();

        if (this.IsGrounded) {
            float angle = Vector3.Angle(Vector3.up, this.normal);

            if (angle > this.angleLimit) {
                this.velocity -= Vector3.ProjectOnPlane(Vector3.up, -this.normal);
            }
        }

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
        }
        
        position += distance * direction;
        center = position + offset;
        ok = Physics.BoxCast(center, size, Vector3.down, out hit, Quaternion.identity, 100);
        
        if (ok) {
            this.IsGrounded = hit.distance <= this.stepOffset + MIN_RANGE;
            this.normal = hit.normal;
            this.groundY = hit.point.y + MIN_RANGE;

            if (this.IsGrounded) {
                position.y = this.groundY;
                this.CheckLegalPosition(hit, position);
            }
        }
        else {
            this.IsGrounded = false;
            this.normal = Vector3.up;
            this.groundY = position.y;
        }

        this.SetPosition(position);
    }
    
    public void Move(Vector3 velocity) {
        this.velocity += velocity;
    }

    public void SetPosition(Vector3 position, bool adjust=false) {
        this.LatePosition = this.transform.position;
        this.transform.position = position;
        this.velocity = Vector3.zero;
        
        if (adjust) {
            this.AdjustPosition();
        }
    }
    
    private void AdjustPosition() {
        var pos = this.transform.position;
        var size = this.collider.size;
        var center = pos + this.collider.center + Vector3.up * size.y;
        RaycastHit hit;
        bool ok = Physics.BoxCast(center, size, Vector3.down, out hit, Quaternion.identity, size.y);
        
        if (ok) {
            this.normal = hit.normal;
            this.groundY = pos.y;
            this.IsGrounded = true;

            pos.y = hit.point.y + MIN_RANGE;
            this.transform.position = pos;
            this.CheckLegalPosition(hit, pos);
        }
        else {
            this.normal = Vector3.up;
            this.groundY = this.transform.position.y;
            this.IsGrounded = false;
        }
    }

    private void CheckLegalPosition(RaycastHit hit, Vector3 position) {
        float angle = Vector3.Angle(Vector3.up, hit.normal);

        if (angle <= this.angleLimit) {
            this.LegalPosition = position;
        }
    }
}