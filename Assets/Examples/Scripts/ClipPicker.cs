using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPlayable.AnimationMixer;

namespace UPlayable.AnimationMixer.Example
{
    public class ClipPicker : MonoBehaviour
    {
        private int index;
        public AnimationClipOutput Target;
        public AnimationClip[] Clips;
        public bool AutoChange;
        private float elapsedTime;
        // Start is called before the first frame update
        void Start()
        {
            index = Clips.Length - 1;
        }

        private void Update()
        {
            if (!AutoChange && Input.GetKeyDown(KeyCode.Space))
            {
                NextClip();
                Target.Play();
            }
            elapsedTime += Time.deltaTime;
            if (AutoChange && elapsedTime > 1f)
            {
                elapsedTime = 0;
                NextClip();
                Target.Play();
            }
        }

        public void NextClip()
        {
            if (index < 0)
                return;
            Target.ToClip = Clips[index];
            index++;
            if (index > Clips.Length - 1)
            {
                index = 0;
            }
        }
    }
}