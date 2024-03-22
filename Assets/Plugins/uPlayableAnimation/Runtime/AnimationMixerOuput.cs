using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    public class AnimationMixerOuput : BaseAnimationOutput
    {
        public AnimationClip FromClip;
        public AnimationClip ToClip;
        [Range(0, 1f)]
        public float Weight;
        private AnimationMixerPlayable m_mixerPlayable;
        private AnimationClipPlayable m_fromPlayable;
        private AnimationClipPlayable m_toPlayable;

        protected override Playable m_managerInput => m_mixerPlayable;

        protected override void ParseSettingToModel()
        {
            m_model = new AnimationOutputModel
            {
                IsAnimatorPlayable = false,
                ClipLength = ToClip.length,
                OutputTargetWeight = TransitionSetting.OutputTargetWeight,
                FadeInTime = TransitionSetting.FadeInTime,
                ExitTime = TransitionSetting.ExitTime,
                FixedTimeOffset = TransitionSetting.FixedTimeOffset,
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
            var mixLength = Mathf.Lerp(FromClip.length, ToClip.length, Weight);
            m_fromPlayable.SetSpeed(FromClip.length / mixLength);
            m_toPlayable.SetSpeed(ToClip.length / mixLength);

            m_model.ClipLength = mixLength;
            m_manager.UpdateInputModel(m_Id, m_model, LayerIndex);
        }

        protected override void CreatePlayables()
        {
            m_mixerPlayable = AnimationMixerPlayable.Create(m_manager.PlayableGraph, 2);
            m_fromPlayable = AnimationClipPlayable.Create(m_manager.PlayableGraph, FromClip);
            m_toPlayable = AnimationClipPlayable.Create(m_manager.PlayableGraph, ToClip);
            m_fromPlayable.SetTime(0f);
            m_toPlayable.SetTime(0f);

            m_manager.PlayableGraph.Connect(m_fromPlayable, 0, m_mixerPlayable, 0);
            m_manager.PlayableGraph.Connect(m_toPlayable, 0, m_mixerPlayable, 1);
            m_Id = m_mixerPlayable.GetHashCode();
            if (LayerIndex != 0 && AvatarMask != null)
            {
                m_manager.SetLayerAdditive((uint)LayerIndex, false);
                m_manager.SetLayerAvatarMask((uint)LayerIndex, AvatarMask);
            }
        }
    }
}