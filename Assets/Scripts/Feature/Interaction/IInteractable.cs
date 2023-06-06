using UnityEngine;

namespace Player.Interaction
{
    public interface IInteractable
    {
        public abstract void Interact();
        public abstract void Interact(GameObject other = default);
        public virtual void ExitInteract() { }
    }
}