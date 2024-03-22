using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UPlayable.AnimationMixer.Example
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnAnimationEventRequest(string s)
        {
            Debug.Log("AnimationEvent : " + s);
        }
    }
}