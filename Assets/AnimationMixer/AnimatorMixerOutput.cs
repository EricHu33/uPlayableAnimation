using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    public class AnimatorMixerOutput : BaseAnimationOutput
    {
        public AnimationClip FromClip;
        public RuntimeAnimatorController animCtrl;
        [Range(0, 1f)]
        public float Weight;
        public Vector2 Direction;
        protected override Playable m_managerInput => m_mixerPlayable;
        private AnimationMixerPlayable m_mixerPlayable;
        private AnimationClipPlayable m_fromPlayable;
        private AnimatorControllerPlayable m_toPlayable;

        protected override void ParseSettingToModel()
        {
            m_model = new AnimationOutputModel
            {
                IsAnimatorPlayable = true,
                ClipLength = 0,
                OutputTargetWeight = TransitionSetting.OutputTargetWeight,
                FadeInTime = TransitionSetting.FadeInTime,
                ExitTime = TransitionSetting.ExitTime,
                RestartWhenPlay = TransitionSetting.RestartWhenPlay,
                Speed = TransitionSetting.ClipSpeed,
            };
        }

        private void Update()
        {
            if (!m_mixerPlayable.IsValid())
                return;
            Weight = Mathf.Clamp01(Weight);
            m_mixerPlayable.SetInputWeight(0, 1.0f - Weight);
            m_mixerPlayable.SetInputWeight(1, Weight);
            m_animator.SetFloat("Horizontal", Direction.x);
            m_animator.SetFloat("Vertical", Direction.y);
        }

        protected override void CreatePlayables()
        {
            m_mixerPlayable = AnimationMixerPlayable.Create(m_manager.PlayableGraph, 2);
            m_fromPlayable = AnimationClipPlayable.Create(m_manager.PlayableGraph, FromClip);
            m_toPlayable = AnimatorControllerPlayable.Create(m_manager.PlayableGraph, animCtrl);
            m_fromPlayable.SetTime(0f);

            m_manager.PlayableGraph.Connect(m_fromPlayable, 0, m_mixerPlayable, 0);
            m_manager.PlayableGraph.Connect(m_toPlayable, 0, m_mixerPlayable, 1);
            m_Id = m_mixerPlayable.GetHashCode();
        }
    }
}