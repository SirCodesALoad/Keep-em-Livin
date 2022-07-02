using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlackGardenStudios.HitboxStudioPro
{
    [CustomEditor(typeof(AnimationEventObject))]
    public class AnimationEventObjectInspector : Editor
    {
        AnimationEventObject m_Target;

        private void OnEnable()
        {
            m_Target = target as AnimationEventObject;
        }

        public override void OnInspectorGUI()
        {
            var events = AnimationUtility.GetAnimationEvents(m_Target.clip);
            var targetEvent = events[m_Target.eventIndex];

            GUI.changed = false;

            targetEvent.functionName = EditorGUILayout.TextField("Function", targetEvent.functionName);
            targetEvent.floatParameter = EditorGUILayout.FloatField("Float", targetEvent.floatParameter);
            targetEvent.intParameter = EditorGUILayout.IntField("Int", targetEvent.intParameter);
            targetEvent.stringParameter = EditorGUILayout.TextField("String", targetEvent.stringParameter);
            targetEvent.objectReferenceParameter = EditorGUILayout.ObjectField(new GUIContent("Object", "Cannot be a scene object. Must be an object/asset in your project."), targetEvent.objectReferenceParameter, typeof(Object), false);
            targetEvent.messageOptions = (SendMessageOptions)EditorGUILayout.EnumPopup(new GUIContent("SendMessage", "Choose whether or not an error should be logged when there is no receiver for this event"), targetEvent.messageOptions);

            if(GUI.changed == true)
                AnimationUtility.SetAnimationEvents(m_Target.clip, events);
        }
    }
}
