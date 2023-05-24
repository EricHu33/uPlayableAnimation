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

        protected override void CreatePlayables()
        {
            m_toPlayable = AnimatorControllerPlayable.Create(m_manager.PlayableGraph, AnimationControll);
            m_toPlayable.SetTime(0f);
            m_Id = m_toPlayable.GetHashCode();
        }
    }
}