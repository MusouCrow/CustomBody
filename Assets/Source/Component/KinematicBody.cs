using UnityEngine;

public class KinematicBody : IMover, IBody {
    public struct CastHit {
        public bool collided;
        public Vector3 position;
        public Vector3 normal;
        public Vector3 direction;
        public float distance;
        public GameObject gameObject;
    }

    private Collider[] Overlaps = new Collider[2];
    private float MinMoveDistance = 0.01f;
    private float Interval = 0.01f;
    private float GroundDistance = 100;
    private LayerMask LayerMask = LayerMask.GetMask("Default");

    public float slopeLimit = 30;
    public float stepOffset = 0.2f;

    private float radius;
    private float height;
    private Transform transform;
    private CapsuleCollider collider;
    private Vector3 position;
    private CastHit groundHit;
    private bool stepTick;
    private bool overlapTick;
    private bool setPositionTick;

    public float Radius {
        get {
            return this.radius;
        }
        set {
            this.radius = value;
            this.FlushCollider();
        }
    }

    public float Height {
        get {
            return this.height;
        }
        set {
            this.height = value;
            this.FlushCollider();
        }
    }

    public bool InGround {
        get;
        private set;
    }

    public bool InLegalGround {
        get;
        private set;
    }

    public Vector3 Velocity {
        get;
        private set;
    }

    public Vector3 GroundPosition {
        get {
            return new Vector3(this.position.x, this.groundHit.position.y, this.position.z);
        }
    }

    public Vector3 LegalGroundPosition {
        get;
        private set;
    }

    public KinematicBody(Transform transform, CapsuleCollider collider) {
        this.transform = transform;
        this.collider = collider;
        this.radius = collider.radius;
        this.height = collider.height - this.radius * 2;
        this.position = this.transform.position;

        this.FlushCollider();
    }

    public void LateUpdate() {
        bool tick = false;

        if (this.overlapTick) {
            tick = true;
            this.overlapTick = false;
        }

        if (this.setPositionTick) {
            this.Velocity = Vector3.zero;
            this.setPositionTick = false;
            tick = true;
        }

        if (!this.Velocity.Equal(Vector3.zero)) {
            var normal = Vector3.zero;
            var velocity = this.Velocity;

            int count = 0;
            var direction = velocity.normalized;
            var distance = velocity.magnitude;
            var hit = this.Simulate(this.position, direction, distance, ref count);

            this.Velocity = Vector3.zero;
            this.position = hit.position;
            tick = true;
        }
        
        if (tick) {
            this.FixOverlap();
            this.CheckGround();
            this.transform.position = this.position;
        }
    }

    public void DrawGizmos() {
        var radius = this.radius * Vector3.up;
        var height = this.height * Vector3.up;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.position, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.position + radius, this.radius);
        Gizmos.DrawWireSphere(this.position + height + radius, this.radius);
    }

    public void FlushCollider() {
        this.collider.radius = this.radius;
        this.collider.height = this.height + this.radius * 2;
        this.collider.center = new Vector3(0, this.collider.height * 0.5f, 0);
        this.overlapTick = true;
    }

    public void Move(Vector3 velocity) {
        this.Velocity += velocity;
    }

    public void SetPosition(Vector3 position) {
        this.position = position;
        this.setPositionTick = true;
        this.overlapTick = true;
    }

    public void OnControllerColliderHit(ControllerColliderHit hit) {}
    
    private CastHit Simulate(Vector3 position, Vector3 direction, float distance, ref int count) {
        var hit = this.CollideCast(position, direction, distance);
        var restDistance = distance - hit.distance;
        count++;
        /*
        if (hit.collided && count == 1 && this.stepOffset > 0 && restDistance > MinMoveDistance && !this.IsLegalSlope(hit.normal)) {
            var hit2 = this.CollideCast(hit.position + Vector3.up * this.stepOffset, direction, restDistance);
            
            if (!hit2.collided && hit2.distance > MinMoveDistance) {
                hit = hit2;
                restDistance = distance - hit.distance;
                this.stepTick = true;
            }
        }
        */
        if (hit.collided && restDistance > MinMoveDistance && count < 3) {
            var shift = Vector3.ProjectOnPlane(direction, hit.normal);
            // Debug.Log(count + ", " + direction + ", " + hit.normal + ", " + shift);

            if (!shift.Equal(Vector3.zero)) {
                return this.Simulate(hit.position, shift, restDistance, ref count);
            }
        }

        return hit;
    }

    private void CheckGround() {
        this.groundHit = this.CollideCast(this.position, Vector3.down, GroundDistance);
        
        if (this.stepTick) {
            if (this.groundHit.distance <= this.stepOffset) {
                this.position.y = this.groundHit.position.y;
            }
            else {
                this.position.y -= this.stepOffset;
            }

            this.stepTick = false;
        }
        
        this.InGround = this.groundHit.distance <= MinMoveDistance;
        this.InLegalGround = this.InGround && this.IsLegalSlope(this.groundHit.normal);
        
        if (this.InLegalGround) {
            this.LegalGroundPosition = this.groundHit.position;
        }
    }

    private bool FixOverlap() {
        var position = this.position;
        var height = Vector3.up * this.height;
        var radius = Vector3.up * this.radius;

        var p1 = position + radius;
        var p2 = position + radius + height;
        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, this.radius - Interval, Overlaps, LayerMask);

        if (count <= 1) {
            return false;
        }

        float distance = 0;
        Vector3 direction = Vector3.zero;
        var collider = Overlaps[0] == this.collider ? Overlaps[1] : Overlaps[0];

        bool ok = Physics.ComputePenetration(
            this.collider, position, this.transform.rotation,
            collider, collider.transform.position, collider.transform.rotation, 
            out direction, out distance
        );

        this.position = position + direction * distance;

        return true;
    }

    private CastHit CollideCast(Vector3 position, Vector3 direction, float distance) {
        var height = Vector3.up * this.height;
        var radius = Vector3.up * this.radius;

        var p1 = position + radius;
        var p2 = position + radius + height;
        RaycastHit hit;
        bool collided = Physics.CapsuleCast(p1, p2, this.radius, direction, out hit, distance, LayerMask);
        
        Vector3 normal = Vector3.zero;
        GameObject gameObject = null;

        if (collided) {
            distance = hit.distance - Interval;
            normal = hit.normal;
            gameObject = hit.collider.gameObject;
        }

        return new CastHit() {
            collided = collided,
            position = position + direction * distance,
            normal = normal,
            direction = direction,
            distance = distance,
            gameObject = gameObject
        };
    }

    private bool IsLegalSlope(Vector3 normal) {
        float angle = Vector3.Angle(Vector3.up, normal);

        return angle <= this.slopeLimit;
    }
}