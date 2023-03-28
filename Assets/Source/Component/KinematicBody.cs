using UnityEngine;

public class KinematicBody : IMover {
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
    private float GroundDistance = 100;
    private LayerMask LayerMask = LayerMask.GetMask("Default");

    public float slopeLimit = 30;
    public float stepOffset = 0.3f;

    private float radius;
    private float height;
    private Transform transform;
    private CapsuleCollider collider;
    private Vector3 position;
    private CastHit groundHit;
    private bool stepTick;
    private bool overlapTick;
    private bool setPositionTick;
    private Vector3 settingPosition;

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

    public Vector3 GroundNormal {
        get {
            return this.groundHit.normal;
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

        this.FlushCollider();
    }

    public void LateUpdate() {
        bool tick = false;

        if (this.overlapTick) {
            if (this.setPositionTick) {
                this.transform.position = this.settingPosition;
                this.setPositionTick = false;
                this.Velocity = Vector3.zero;
            }

            this.FixOverlap();
            this.overlapTick = false;
            tick = true;
        }

        if (!this.Velocity.Equal(Vector3.zero)) {
            var normal = Vector3.zero;
            var velocity = this.Velocity;

            if (this.InGround) {
                if (velocity.y <= 0) {
                    normal = this.groundHit.normal;
                }
            }

            int count = 0;
            var direction = velocity.normalized;
            var distance = velocity.magnitude;
            var hit = this.Simulate(this.position, direction, distance, normal, ref count);

            this.Velocity = Vector3.zero;
            this.position = hit.position;
            tick = true;
        }
        
        if (tick) {
            this.CheckGround();
            this.transform.position = position;
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
        this.settingPosition = position;
        this.setPositionTick = true;
        this.overlapTick = true;
    }

    private CastHit Simulate(Vector3 position, Vector3 direction, float distance, Vector3 normal, ref int count) {
        var planeDir = normal.Equal(Vector3.zero) ? direction : Vector3.ProjectOnPlane(direction, normal);
        var hit = this.CollideCast(position, planeDir, distance);
        count++;

        var restDistance = distance - hit.distance;

        // Debug.Log(position + ", " + direction + ", " + distance + ", " + normal + ", " + planeDir + ", " + count);
        
        // 阶梯处理
        if (hit.collided && count == 1 && this.stepOffset > 0 && restDistance > MinMoveDistance && direction.y <= 0) {
            var hit2 = this.CollideCast(hit.position + Vector3.up * this.stepOffset, planeDir, restDistance);

            if (hit2.distance > MinMoveDistance) {
                hit2.normal = hit.normal;
                hit = hit2;
                restDistance = distance - hit.distance;
                this.stepTick = true;
            }
        }
        
        if (hit.collided && restDistance > MinMoveDistance) {
            bool pass = false;
            var shift = this.ToShiftDirection(direction, hit.normal);
            
            // 多段平滑
            /*
            if (this.IsLegalSlope(hit.normal)) {
                normal = hit.normal;
                pass = true;
            }
            else {
                normal = Vector3.zero;
            }
            */
            
            // 滑动
            if (hit.distance <= MinMoveDistance && !shift.Equal(Vector3.zero) && count < 3) {
                direction = shift;
                pass = true;
            }
            
            if (pass) {
                return this.Simulate(hit.position, direction, restDistance, normal, ref count);
            }
        }
        
        
        return hit;
    }

    private void CheckGround() {
        this.groundHit = this.CollideCast(this.position, Vector3.down, GroundDistance);
        this.InGround = this.groundHit.distance <= MinMoveDistance;
        this.InLegalGround = this.InGround && this.IsLegalSlope(this.groundHit.normal);
        // Debug.Log(this.groundHit.normal + ", " + this.InLegalGround);
        if (this.InLegalGround) {
            this.LegalGroundPosition = this.groundHit.position;
        }

        // RaycastHit hit;
        // bool ok = Physics.Raycast(this.position, Vector3.down, out hit, GroundDistance, LayerMask);
        // this.groundHit.normal = ok ? hit.normal : Vector3.up;
        
        if (this.stepTick) {
            if (this.groundHit.distance <= this.stepOffset) {
                this.position.y = this.groundHit.position.y;
            }

            this.stepTick = false;
        }
    }

    private bool FixOverlap() {
        var position = this.transform.position;
        var height = Vector3.up * this.height;
        var radius = Vector3.up * this.radius;

        var p1 = position + radius;
        var p2 = position + radius + height;
        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, this.radius - MinMoveDistance, Overlaps, LayerMask);

        if (count <= 1) {
            this.position = position;
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
            distance = hit.distance - MinMoveDistance;
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

    private Vector3 ToShiftDirection(Vector3 direction, Vector3 normal) {
        var dir = Vector3.ProjectOnPlane(direction, normal);
        dir.y = direction.y;

        return dir.normalized;
    }
}