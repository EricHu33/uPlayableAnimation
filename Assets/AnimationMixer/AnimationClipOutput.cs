using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    public class AnimationClipOutput : BaseAnimationOutput
    {
        public AnimationClip ToClip;
        private AnimationClipPlayable m_toPlayable;
        protected override Playable m_managerInput => m_toPlayable;

        protected override void CreatePlayables()
        {
            m_toPlayable = AnimationClipPlayable.Create(m_manager.PlayableGraph, ToClip);
            m_toPlayable.SetTime(0f);
            m_Id = Animator.StringToHash(ToClip.name + Time.time.ToString());
        }
    }

}
