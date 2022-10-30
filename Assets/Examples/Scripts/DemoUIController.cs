using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UPlayable.AnimationMixer;
using System;

namespace UPlayable.AnimationMixer.Example
{
    public class DemoUIController : MonoBehaviour
    {
        public TextMeshProUGUI SpeedValueText;
        public TextMeshProUGUI FpsValueText;
        public AnimationMixerManager Manager;
        public BaseAnimationOutput Output;
        // Start is called before the first frame update
        void Start()
        {
            Manager.EnabledCustomFPS = true;
        }

        // Update is called once per frame
        void Update()
        {
            FpsValueText.text = "" + Manager.TargetFramerate;
            SpeedValueText.text = String.Format("{0:0.0}", Output.BaseSpeed);
        }
    }
}