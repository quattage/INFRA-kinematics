
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Objects.Level.Testers.Flinger {

    public class Flinger : MonoBehaviour, IListenForCollisions {
        
        [SerializeField] private Transform _orientation;
        [SerializeField] private float _flingStrength;

        public void OnCollide(Collider coll) {
            Movable body = coll.gameObject.GetComponent<Movable>();
            Vector3 launch = (_orientation.rotation * Vector3.forward).normalized;
            launch *= _flingStrength;
            body.SetVelocity(launch);
        }
    }
}