
// SIGNATURE :)

using UnityEngine;

namespace Assets.quatworks.INFRASEC.Objects.Level.Testers.Flinger {

    public class ColliderListener : MonoBehaviour {

        [SerializeField] LayerMask targetMask;
        [SerializeField] MonoBehaviour target;

        void OnTriggerEnter(Collider other) {
            if(((1 << other.gameObject.layer) & targetMask) == 0) return;
            if(target is IListenForCollisions lfc)
                lfc.OnCollide(other);
        }
    }

    public interface IListenForCollisions {
        public abstract void OnCollide(Collider coll);
    }
}