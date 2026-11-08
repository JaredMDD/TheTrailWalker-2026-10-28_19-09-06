using TheTrailWalker.Domain.Travel;
using UnityEngine;
namespace TheTrailWalker.Harvest
{
    // Stable scene identity and hint sockets, without input, movement or reward logic.
    public sealed class Tier1HarvestVisual : MonoBehaviour
    {
        public string AnchorId;
        public InteractableKind Kind;
        public Transform FocusAnchor, HintAnchor, RippleRoot;
        public Collider SelectionBounds;
    }
}
