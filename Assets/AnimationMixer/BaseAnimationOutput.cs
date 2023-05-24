using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    [RequireComponent(typeof(AnimationMixerManager))]
    public abstract class BaseAnimationOutput : MonoBehaviour
    {
        [System.Serializable]
        public class ClipTransitionSetting
        {
            public float OutputTargetWeight = 1;
            [Tooltip("Blending time to other animation")]
            public float FadeInTime;
            [Tooltip("Starting time offset in seconds")]
            public float FixedTimeOffset;
            [Tooltip("Transition to other animation will be banned for ExitTime seconds")]
            public float ExitTime;
            public float ClipSpeed = 1;
            [Header("(non-static clip will always restart, which will ignore this flag)")]
            [Tooltip("If the animation needs to restart when Play execute")]
            public bool RestartWhenPlay;

        }

        [SerializeField]
        protected int LayerIndex = 0;
        [SerializeField]
        protected AvatarMask AvatarMask;
        [SerializeField]
        protected bool IsStatic = false;
        [SerializeField]
        protected ClipTransitionSetting TransitionSetting = new ClipTransitionSetting();
        protected AnimationOutputModel m_model;
        protected AnimationMixerManager m_manager;
        protected int m_Id = -1;
        protected abstract Playable m_managerInput { get; }
        protected Animator m_animator;
        public int Id => m_Id;
        public float BaseSpeed
        {
            get { return TransitionSetting.ClipSpeed; }
        }

        private void Start()
        {
            m_manager = GetComponentInParent<AnimationMixerManager>();
            m_animator = GetComponentInParent<Animator>();
            ParseSettingToModel();
            if (IsStatic)
            {
                CreatePlayables();
                m_manager.AddStaticPlayable(m_Id, m_managerInput, m_model);
            }
        }

        public void SetSpeed(float speed)
        {
            TransitionSetting.ClipSpeed = speed;
            ParseSettingToModel();
            if (m_Id != -1 && IsStatic)
            {
                m_manager.UpdateInputModel(m_Id, m_model, LayerIndex);
            }
        }

        public void SetFixedTimeOffset(float fixedTimeOffset)
        {
            TransitionSetting.FixedTimeOffset = fixedTimeOffset;
            ParseSettingToModel();
            if (m_Id != -1 && IsStatic)
            {
                m_manager.UpdateInputModel(m_Id, m_model, LayerIndex);
            }
        }

        protected virtual void ParseSettingToModel()
        {
            m_model = new AnimationOutputModel
            {
                OutputTargetWeight = TransitionSetting.OutputTargetWeight,
                FadeInTime = TransitionSetting.FadeInTime,
                ExitTime = TransitionSetting.ExitTime,
                RestartWhenPlay = TransitionSetting.RestartWhenPlay,
                FixedTimeOffset = TransitionSetting.FixedTimeOffset,
                Speed = TransitionSetting.ClipSpeed,
            };
        }

        private void OnValidate()
        {
            ParseSettingToModel();
            if (m_Id != -1 && IsStatic)
            {
                m_manager.UpdateInputModel(m_Id, m_model, LayerIndex);
            }
        }

        protected abstract void CreatePlayables();
        [ContextMenu("Play")]
        public void Play(bool force = false)
        {
            if (!IsStatic)
            {
                ParseSettingToModel();
                CreatePlayables();
                m_manager.PlayDynamicPlayable(m_managerInput, m_model, LayerIndex);
            }
            else
            {
                m_manager.Play(m_Id, force, LayerIndex);
            }
            if (LayerIndex != 0 && AvatarMask != null)
            {
                m_manager.SetLayerAdditive((uint)LayerIndex, false);
                m_manager.SetLayerAvatarMask((uint)LayerIndex, AvatarMask);
            }
        }
    }
}