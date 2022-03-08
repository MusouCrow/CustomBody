public class Gear {
    public virtual bool IsRunning {
        get;
        protected set;
    }
    
    public virtual void Enter() {
        this.IsRunning = true;
    }

    public virtual void Exit() {
        this.IsRunning = false;
    }
}