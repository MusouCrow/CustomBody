using UnityEngine;

public class Body : MonoBehaviour {
    private new CapsuleCollider collider;
    private new Transform transform;

    private Vector3 velocity;
    private Vector3 collided;

    protected void Awake() {
        this.collider = this.GetComponent<CapsuleCollider>();
        this.transform = this.gameObject.transform;
    }

    protected void LateUpdate() {
        if (this.velocity == Vector3.zero) {
            return;
        }
        
        var direction = this.velocity.normalized;
        var distance = this.velocity.magnitude;
        var radius = this.collider.radius;
        var center = this.transform.position + this.collider.center;
        var height = this.collider.height * Vector3.up * 0.5f;
        var point1 = center + height;
        var point2 = center - height;
        RaycastHit hit;
        
        bool ok = Physics.CapsuleCast(point1, point2, radius, direction, out hit, distance);
        
        if (ok) {
            distance = hit.distance - 0.0001f;
            this.collided = this.transform.position + distance * direction;
        }
        
        this.transform.position += distance * direction;
        this.velocity = Vector3.zero;
    }

    protected void OnDrawGizmos() {
        if (!this.collider) {
            return;
        }

        Gizmos.DrawSphere(this.collided, 0.1f);
        Gizmos.DrawWireSphere(this.collided, this.collider.radius);
    }
    
    public void Move(Vector3 velocity) {
        this.velocity += velocity;
    }
}