using UnityEngine;
using System;

namespace ProceduralWorlds
{
    [System.Serializable]
    public struct SerializableKeyframe : IEquatable< SerializableKeyframe >
    {
        public float inTangent;
        public float outTangent;
        public float value;
        public float time;
    
        public SerializableKeyframe(float time, float value, float inTangent, float outTangent)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }

		public bool Equals(SerializableKeyframe other)
		{
            return other.inTangent == inTangent
                && other.outTangent == outTangent
                && other.value == value
                && other.time == time;
		}

		public static implicit operator Keyframe(SerializableKeyframe sk)
        {
            return new Keyframe(sk.time, sk.value, sk.inTangent, sk.outTangent);
        }
    
        public static implicit operator SerializableKeyframe(Keyframe key)
        {
            return new SerializableKeyframe(key.time, key.value, key.inTangent, key.outTangent);
        }
    }
}