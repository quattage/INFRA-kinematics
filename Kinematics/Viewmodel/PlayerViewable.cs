
// SIGNATURE :)

using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.Viewmodel {

    public interface PlayerViewable {
        public abstract Camera GetPrimaryCamera();
        public abstract Transform GetViewmodelTransform();
        public abstract void Possess();
        public abstract void UnPossess();
        public abstract bool IsPossessed();
        public abstract bool IsAvailable();
    }
}