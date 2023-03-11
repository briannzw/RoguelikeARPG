using UnityEngine;

namespace Player.Interaction
{
    public interface IInteractable
    {
        void Interact() { }
        void Interact(GameObject other = default) { Interact(); }
        public void ExitInteract() { }
    }
}