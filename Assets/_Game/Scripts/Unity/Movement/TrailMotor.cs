using TheTrailWalker.Domain;
using UnityEngine;

namespace TheTrailWalker.Movement
{
    public sealed class TrailMotor : MonoBehaviour
    {
        public MovementState State { get; private set; }
        public ITrailPath Path { get; private set; }
        public System.Func<float, float> LimitMovementSeconds { get; set; }
        // While detached (seated at a camp or walking to/from a seat) the player is posed by the camp flow, not the path.
        public bool Detached { get; set; }
        public void Bind(MovementState state, ITrailPath path) { State = state; Path = path; Present(); }
        private void Update()
        {
            if (State == null || Detached) return;
            float seconds = LimitMovementSeconds == null ? Time.deltaTime : LimitMovementSeconds(Time.deltaTime);
            State.Advance(seconds, Path.Length);
            Present();
        }
        public void Present()
        {
            Path.Evaluate(State.Distance, out Vector3 position, out Vector3 tangent);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(tangent, Vector3.up));
        }
    }
}
