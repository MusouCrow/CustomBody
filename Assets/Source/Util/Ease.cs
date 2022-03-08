public class Ease : Gear {
    public float from;
    public float to;
    public float current;
    public float speed;
    public int direction;
    public bool isRound;

    public float Progress {
        get {
            float v = this.from < this.to ? this.to : this.from;

            return this.current / v;
        }
    }

    public void Update(float rate=1) {
        if (!this.IsRunning) {
            return;
        }

        this.current += this.speed * this.direction * rate;

        if (this.speed > 0 && ((this.direction == 1 && this.current >= this.to) || (this.direction == -1 && this.current <= this.to))) {
            this.current = this.to;

            if (this.isRound) {
                this.Enter(this.to, this.from, this.speed, this.isRound);
            }
            else {
                this.Exit();
            }
        }
    }

    public void Enter(float from, float to, float speed, bool isRound=false) {
        base.Enter();
        
        this.from = from;
        this.to = to;
        this.current = from;
        this.speed = speed;
        this.isRound = isRound;
        this.direction = this.from < this.to ? 1 : -1;
    }
}