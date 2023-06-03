using UnityEngine;

namespace Player.Interaction
{
    public interface IInteractable
    {
        public abstract void Interact();
        public virtual void Interact(GameObject other = default) { Interact(); }
        public virtual void ExitInteract() { }
    }
}