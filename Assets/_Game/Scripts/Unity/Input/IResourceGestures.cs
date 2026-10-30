using UnityEngine;
namespace TheTrailWalker.Input
{
    public interface IResourceGestures
    {
        bool Begin(int pointer, Vector2 position);
        void Move(int pointer, Vector2 position);
        void End(int pointer, Vector2 position, bool canceled);
        void CancelAll();
    }
}
