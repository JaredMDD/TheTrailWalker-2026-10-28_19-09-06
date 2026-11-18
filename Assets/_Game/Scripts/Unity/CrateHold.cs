namespace TheTrailWalker
{
    // Milestone 12: one bag drag over the warehouse crates. The view ticks it every frame with the crate under the finger
    // (-1, 0 or 1), whether each crate is animating, the frame time and whether the held stack is still live; it answers the
    // crate to deposit into this frame, or -1. The view sets Deposited after a deposit succeeds.
    //  - A stack rests 0.3 s over a crate before the drag's first deposit (a pass over a crate on the way elsewhere does nothing).
    //  - After that, entering a crate that is not animating deposits at once, and so does staying over a crate whose animation ends.
    //  - It fires at most once per crate entry or landing, so a refused deposit is tried (and its reason shown) once per entry.
    //  - A non-stackable item deposits only on release, and a release deposits only if the drag deposited nothing yet.
    public sealed class CrateHold
    {
        public const float Rest = 0.3f, AnimationSeconds = 0.65f;
        private readonly bool stackable;
        private readonly bool[] wasAnimating = new bool[2];
        private int over = -1;
        private float overTime;
        private bool armed;

        public bool Deposited { get; set; }

        public CrateHold(bool stackable) { this.stackable = stackable; }

        public int Tick(int crate, bool animating0, bool animating1, float deltaTime, bool live)
        {
            bool animating = crate == 0 ? animating0 : crate == 1 && animating1;
            bool landed = crate >= 0 && wasAnimating[crate] && !animating;
            wasAnimating[0] = animating0; wasAnimating[1] = animating1;
            if (crate != over) { over = crate; overTime = 0f; armed = true; }
            overTime += deltaTime;
            if (landed) armed = true;
            if (!live || !stackable || crate < 0 || animating || !armed || (!Deposited && overTime < Rest)) return -1;
            armed = false;
            return crate;
        }

        public int Release(int crate, bool animating0, bool animating1, bool live)
        {
            bool animating = crate == 0 ? animating0 : crate == 1 && animating1;
            return live && !Deposited && crate >= 0 && !animating ? crate : -1;
        }
    }
}
