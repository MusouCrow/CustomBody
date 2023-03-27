using UnityEngine;

public class Tester : MonoBehaviour {
    public enum CastType {Box, Capsule};

    public struct CastHit {
        public bool collided;
        public Vector3 position;
        public Vector3 normal;
        public Vector3 direction;
        public float distance;
        public GameObject gameObject;
    }

    public CastType castType;
    public float radius = 1;
    public float height = 1.5f;
    public float slopeLimit = 45;
    public float stepOffset = 0.3f;
    public Vector3 velocity;

    private CastHit groundHit;
    private CastHit moveHit;
    private bool stepTick;
    private bool moveTick;

    public bool InGround {
        get;
        private set;
    }

    public Vector3 Position {
        get {
            return this.transform.position;
        }
    }

    protected void Update() {        
        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;
        var normal = Vector3.zero;
        int count = 0;
        
        if (this.velocity.y.Equal(0) && this.InGround) {
            normal = this.groundHit.normal;
        }

        this.moveHit = this.Simulate(this.Position, direction, distance, normal, ref count);
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            this.transform.position = this.moveHit.position;
            this.moveTick = true;
        }
        else if (Input.GetKeyDown(KeyCode.Return)) {
            this.transform.position = this.groundHit.position;
            this.moveTick = true;
        }
        
        this.CheckGround();

        this.stepTick = false;
        this.moveTick = false;
    }

    protected void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }

        this.DrawUnitGizmos(this.Position, Color.yellow);
        this.DrawUnitGizmos(this.moveHit.position, Color.green);

        if (!this.InGround) {
            this.DrawUnitGizmos(this.groundHit.position, Color.black);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(this.Position, this.Position + this.velocity);

        if (this.moveHit.collided) {
            var direction = this.ToShiftDirection(this.moveHit.direction, this.moveHit.normal);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(this.moveHit.position, direction);
        }

        if (this.groundHit.collided) {
            var normal = Vector3.ProjectOnPlane(this.transform.forward, this.groundHit.normal);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(this.groundHit.position, normal);

            var go = this.groundHit.gameObject;
            var t = go.transform;
            var filter = go.GetComponent<MeshFilter>();

            if (filter) {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawMesh(filter.sharedMesh, 0, t.position, t.rotation, t.lossyScale);
            }
        }
    }

    private void DrawUnitGizmos(Vector3 position, Color color) {
        if (this.castType == CastType.Box) {
            this.DrawBoxGizmos(position, color);
        }
        else {
            this.DrawCapsuleGizmos(position, color);
        }
    }

    private void DrawBoxGizmos(Vector3 position, Color color) {
        var size = new Vector3(this.radius, this.height, this.radius) * 2;
        var height = this.height * Vector3.up;

        Gizmos.color = color;
        Gizmos.DrawWireCube(position + height, size);
    }

    private void DrawCapsuleGizmos(Vector3 position, Color color) {
        var radius = this.radius * Vector3.up;
        var height = this.height * Vector3.up;

        Gizmos.color = color;
        Gizmos.DrawSphere(position, 0.1f);

        Gizmos.DrawWireSphere(position + radius, this.radius);
        Gizmos.DrawWireSphere(position + height + radius, this.radius);
    }

    private void CheckGround() {
        this.groundHit = this.CollideCast(this.Position, Vector3.down, 100);
        this.InGround = this.groundHit.distance <= 0.1f && this.IsLegalSlope(this.groundHit.normal);

        if (this.moveTick && this.stepTick) {
            if (this.groundHit.distance <= this.stepOffset) {
                var pos = this.transform.position;
                pos.y = this.groundHit.position.y;
                this.transform.position = pos;
            }
        }

        RaycastHit hit;
        bool ok = Physics.Raycast(this.Position, Vector3.down, out hit, 100);
        this.groundHit.normal = ok ? hit.normal : Vector3.up;

        if (this.InGround && this.velocity.y < 0) {
            this.velocity.y = 0;
        }
    }

    private CastHit Simulate(Vector3 position, Vector3 direction, float distance, Vector3 normal, ref int count) {
        var planeDir = normal.Equal(Vector3.zero) ? direction : Vector3.ProjectOnPlane(direction, normal);
        var hit = this.CollideCast(position, planeDir, distance);
        count++;

        var restDistance = distance - hit.distance;

        // Debug.Log(position + ", " + direction + ", " + distance + ", " + normal + ", " + count);
        
        if (count == 1 && this.stepOffset > 0 && hit.collided && restDistance > 0.1f && direction.y.Equal(0)) {
            var hit2 = this.CollideCast(hit.position + Vector3.up * this.stepOffset, planeDir, restDistance);

            if (hit2.distance > 0.1f) {
                hit = hit2;
                restDistance = distance - hit.distance;
                this.stepTick = true;
            }
        }
        
        if (hit.collided && restDistance > 0.1f) {
            bool pass = false;
            var shift = this.ToShiftDirection(direction, hit.normal);

            if (this.IsLegalSlope(hit.normal)) {
                normal = hit.normal;
                pass = true;
            }
            else {
                normal = Vector3.zero;
            }
            
            if (hit.distance <= 0.1f && !shift.Equal(Vector3.zero) && count == 1) {
                direction = shift;
                pass = true;
            }
            
            if (pass) {
                return this.Simulate(hit.position, direction, restDistance, normal, ref count);
            }
        }
        
        return hit;
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

    private CastHit CollideCast(Vector3 position, Vector3 direction, float distance) {
        if (this.castType == CastType.Box) {
            return this.BoxCast(position, direction, distance);
        }

        return this.CapsuleCast(position, direction, distance);
    }

    private CastHit CapsuleCast(Vector3 position, Vector3 direction, float distance) {
        var height = Vector3.up * this.height;
        var radius = Vector3.up * this.radius;

        var p1 = position + radius;
        var p2 = position + radius + height;
        RaycastHit hit;
        bool collided = Physics.CapsuleCast(p1, p2, this.radius, direction, out hit, distance);
        
        Vector3 normal = Vector3.zero;
        GameObject gameObject = null;

        if (collided) {
            distance = hit.distance - 0.001f;
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

    private CastHit BoxCast(Vector3 position, Vector3 direction, float distance) {
        RaycastHit hit;
        var height = Vector3.up * this.height;
        var extents = new Vector3(this.radius, this.height, this.radius);
        bool collided = Physics.BoxCast(position + height, extents, direction, out hit, Quaternion.identity, distance);

        Vector3 normal = Vector3.zero;
        GameObject gameObject = null;

        if (collided) {
            distance = hit.distance - 0.001f;
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
}