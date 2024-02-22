using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UPlayable.AnimationMixer.Example
{
    public class FaceUpdateExample_Animator : MonoBehaviour
    {
        public AnimationClip[] FacialClips;
        public float Duration;
        private int index;
        private float elapsedTime;
        // Start is called before the first frame update
        void Start()
        {
            index = FacialClips.Length - 1;
            GetComponent<Animator>().SetFloat("Speed", 0.8f);
        }

        // Update is called once per frame
        void Update()
        {
            if (index < 0)
                return;
            elapsedTime += Time.deltaTime;
            if (elapsedTime > Duration)
            {
                elapsedTime = 0;
                GetComponent<Animator>().SetLayerWeight(1, 1);
                GetComponent<Animator>().CrossFade(FacialClips[index].name, 0.25f);
            }
            index++;
            if (index > FacialClips.Length - 1)
            {
                index = 0;
            }
        }
    }
}