using UnityEngine;
using System.Collections;

namespace ProceduralWorlds
{
    [System.Serializable]
    public class SerializableAnimationCurve
    {
        public SerializableKeyframe[] keys;
        public WrapMode postWrapMode;
        public WrapMode preWrapMode;

        public SerializableAnimationCurve() {
            keys = new SerializableKeyframe[0];
        }

        public SerializableAnimationCurve(AnimationCurve ac)
        {
            SetAnimationCurve(ac);
        }
    
        public static explicit operator AnimationCurve(SerializableAnimationCurve sac)
        {
            int i = 0;

            if (sac == null)
                return null;

            Keyframe[] keyframes = new Keyframe[sac.keys.Length];
            foreach (var key in sac.keys)
            {
                keyframes[i].time = key.time;
                keyframes[i].value = key.value;
                keyframes[i].inTangent = key.inTangent;
                keyframes[i].outTangent = key.outTangent;
                i++;
            }
            AnimationCurve ac = new AnimationCurve(keyframes);
            ac.postWrapMode = sac.postWrapMode;
            ac.preWrapMode = sac.preWrapMode;
            return ac;
        }

        public void SetAnimationCurve(AnimationCurve ac)
        {
            int i = 0;
            keys = new SerializableKeyframe[ac.keys.Length];
            foreach (var key in ac.keys)
                keys[i++] = key;
            preWrapMode = ac.preWrapMode;
            postWrapMode = ac.postWrapMode;
        }

        public static implicit operator SerializableAnimationCurve(AnimationCurve ac)
        {
            SerializableAnimationCurve  sac = new SerializableAnimationCurve();
            sac.SetAnimationCurve(ac);
            return sac;
        }
    }
}