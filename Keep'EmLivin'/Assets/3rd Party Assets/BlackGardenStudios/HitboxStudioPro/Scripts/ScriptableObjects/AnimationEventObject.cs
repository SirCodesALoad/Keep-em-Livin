using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlackGardenStudios.HitboxStudioPro
{
    public class AnimationEventObject : ScriptableObject
    {
        public AnimationClip clip;
        public int eventIndex;

        static public AnimationEventObject Edit(AnimationClip clip, int eventIndex)
        {
            var animationEventObject = CreateInstance<AnimationEventObject>();

            animationEventObject.hideFlags = HideFlags.HideInHierarchy;
            animationEventObject.clip = clip;
            animationEventObject.eventIndex = eventIndex;
            animationEventObject.name = "Animation Event";

            return animationEventObject;
        }
        
    }
}
