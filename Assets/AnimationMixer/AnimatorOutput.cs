using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    public class AnimatorOutput : BaseAnimationOutput
    {
        public RuntimeAnimatorController AnimationControll;
        protected override Playable m_managerInput => m_toPlayable;
        private AnimatorControllerPlayable m_toPlayable;

        protected override void CreatePlayables()
        {
            m_toPlayable = AnimatorControllerPlayable.Create(m_manager.PlayableGraph, AnimationControll);
            m_toPlayable.SetTime(0f);
            m_Id = Animator.StringToHash(AnimationControll.name + Time.time.ToString());
        }
    }
}