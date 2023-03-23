using UnityEngine;

public class Tester : MonoBehaviour {
    public float radius = 1;
    public float height = 1.5f;
    public float distance = 10;
    public Vector3 direction = new Vector3(0, 0, 1);

    private Vector3 target;
    private GameObject aim;

    protected void Awake() {
        this.aim = new GameObject("Aim");
        var cc = this.aim.AddComponent<CharacterController>();
        cc.radius = radius;
        cc.height = height;
        cc.enabled = false;

        this.direction = this.direction.normalized;
    }

    protected void Update() {
        RaycastHit hit;
        
        var row = new Vector3(this.radius, this.height * 0.5f, this.radius);
        row = row.Mul(this.direction);

        var position = this.transform.position - row;
        var height = Vector3.up * this.height;
        var p1 = position + height * 0.25f;
        var p2 = position - height * 0.25f;

        bool ok = Physics.CapsuleCast(p1, p2, this.radius, this.direction, out hit, this.distance);

        if (ok) {
            this.target = hit.point - row;
        }
        else {
            this.target = position + this.direction * this.distance;
        }

        this.aim.transform.position = this.target;
    }

    protected void OnDrawGizmos() {
        /*
        var matrix = Matrix4x4.identity;
        matrix.SetTRS(Vector3.zero, Quaternion.identity, new Vector3(this.radius, this.height, this.radius));
        Gizmos.matrix = matrix;
        */
        // Gizmos.color = Color.green;
        // Gizmos.DrawWireSphere(this.transform.position, 1);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.target, 0.1f);
        // Gizmos.DrawWireSphere(this.target, 1);

        // Gizmos.matrix = Matrix4x4.identity;
    }
}