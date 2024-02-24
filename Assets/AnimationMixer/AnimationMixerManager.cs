using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UPlayable.AnimationMixer
{
    [System.Serializable]
    public struct AnimationOutputModel
    {
        public bool IsAnimatorPlayable;
        public float ClipLength;
        public float OutputTargetWeight;
        public float FadeInTime;
        public float FixedTimeOffset;
        public float ExitTime;
        public float Speed;
        [Header("(non-static clip will always restart)")]
        public bool RestartWhenPlay;
    }

    public enum PlayableInputType
    {
        Static,
        Dynamic,
    }

    public struct RuntimeInputData
    {
        public int Id;
        public bool RestartWhenPlay;
        public Playable Playable;
        public PlayableInputType Type;
        public float Weight;
        public float SmoothWeight;
        public float TargetWeight;
        public float FadeDuration;
        public float FixedTimeOffset;
        public float ExitTime;
        public int OccupiedInputIndex;
        public bool IsAnimatorPlayable;
        public float ClipLength;
        public float BaseSpeed;
    }

    public class LayeredPlayablesController
    {
        public int CurrentPlayableIdInLayer;
        private int LastPlayableInPlayer = -1;
        private AnimationMixerPlayable m_rootPlayable;
        private Dictionary<int, RuntimeInputData> m_layeredPlayablesMap = new Dictionary<int, RuntimeInputData>();
        private List<RuntimeInputData> m_layeredPlayables = new List<RuntimeInputData>();
        private float m_remainExitTime;
        private float m_weightDiffThisFrame;
        private Queue<int> m_recycledIndexes = new Queue<int>();
        private bool m_hasStatic;

        public void SetRootPlayable(AnimationMixerPlayable playable)
        {
            m_rootPlayable = playable;
        }

        public void AddStaticPlayable(int id, Playable playable, AnimationOutputModel model, ref PlayableGraph graph)
        {
            m_hasStatic = true;
            var runtimeData = new RuntimeInputData
            {
                Id = id,
                Playable = playable,
                Weight = 0,
                SmoothWeight = 0,
                RestartWhenPlay = model.RestartWhenPlay,
                TargetWeight = model.OutputTargetWeight,
                FadeDuration = model.FadeInTime,
                FixedTimeOffset = model.FixedTimeOffset,
                ExitTime = model.ExitTime,
                Type = PlayableInputType.Static,
                OccupiedInputIndex = m_layeredPlayables.Count,
                ClipLength = model.ClipLength,
                IsAnimatorPlayable = model.IsAnimatorPlayable,
                BaseSpeed = model.Speed,
            };
            m_layeredPlayables.Add(runtimeData);
            m_layeredPlayablesMap.Add(id, runtimeData);
            graph.Connect(playable, 0, m_rootPlayable, m_layeredPlayables.Count - 1);
            Play(id, true);
        }

        public void PlayDynamicPlayable(Playable playable, AnimationOutputModel model, ref PlayableGraph graph)
        {
            if (m_remainExitTime <= 0)
            {
                var id = playable.GetHashCode();
                var portIndex = m_recycledIndexes.Count > 0 ? m_recycledIndexes.Dequeue() : m_layeredPlayables.Count;
                if (portIndex > m_rootPlayable.GetInputCount() - 1)
                {
                    return;
                }
                playable.SetTime(model.FixedTimeOffset);
                var runtimeData = new RuntimeInputData
                {
                    Id = id,
                    Playable = playable,
                    Weight = 0,
                    SmoothWeight = 0,
                    RestartWhenPlay = model.RestartWhenPlay,
                    TargetWeight = model.OutputTargetWeight,
                    FadeDuration = model.FadeInTime,
                    FixedTimeOffset = model.FixedTimeOffset,
                    ClipLength = model.ClipLength,
                    IsAnimatorPlayable = model.IsAnimatorPlayable,
                    ExitTime = model.ExitTime,
                    Type = PlayableInputType.Dynamic,
                    OccupiedInputIndex = portIndex,
                    BaseSpeed = model.Speed,
                };
                m_layeredPlayables.Add(runtimeData);
                m_layeredPlayablesMap.Add(id, runtimeData);
                graph.Connect(playable, 0, m_rootPlayable, portIndex);
                m_rootPlayable.SetInputWeight(portIndex, 0);
                Play(id, true);
            }
        }

        public void Play(int id, bool force = false)
        {
            if (!force && m_remainExitTime > 0)
                return;
            if (m_layeredPlayablesMap.Keys.Count > 1)
            {
                LastPlayableInPlayer = CurrentPlayableIdInLayer;
            }
            else
            {
                LastPlayableInPlayer = m_layeredPlayablesMap[id].Id;
            }
            CurrentPlayableIdInLayer = m_layeredPlayablesMap[id].Id;

            if (m_layeredPlayablesMap[id].RestartWhenPlay)
            {
                m_layeredPlayablesMap[id].Playable.SetTime(m_layeredPlayablesMap[id].FixedTimeOffset);
            }

            m_layeredPlayablesMap[id].Playable.SetSpeed(m_layeredPlayablesMap[id].BaseSpeed);
            m_layeredPlayablesMap[id].Playable.SetDuration(m_layeredPlayablesMap[id].ClipLength);
            m_remainExitTime = m_layeredPlayablesMap[id].ExitTime;
        }

        public bool IsCurrentPlayableCompleted()
        {
            if (m_layeredPlayables.Count == 0)
                return true;
            if (m_hasStatic)
                return false;
            return m_layeredPlayablesMap[CurrentPlayableIdInLayer].Playable.IsDone();
        }

        public bool IsCurrentPlaying(int id)
        {
            return CurrentPlayableIdInLayer == id;
        }

        public void UpdateInputModel(int id, AnimationOutputModel model)
        {
            var p = m_layeredPlayablesMap[id];
            p.TargetWeight = model.OutputTargetWeight;
            p.FadeDuration = model.FadeInTime;
            p.ExitTime = model.ExitTime;
            p.RestartWhenPlay = model.RestartWhenPlay;
            p.ClipLength = model.ClipLength;
            p.FixedTimeOffset = model.FixedTimeOffset;
            p.IsAnimatorPlayable = model.IsAnimatorPlayable;
            p.BaseSpeed = model.Speed;

            m_layeredPlayablesMap[id].Playable.SetSpeed(p.BaseSpeed);
            m_layeredPlayablesMap[id] = p;
            for (int i = 0; i < m_layeredPlayables.Count; i++)
            {
                if (m_layeredPlayables[i].Id == p.Id)
                {
                    m_layeredPlayables[i] = p;
                }
            }
        }

        public void OnEvaluate(float dt)
        {
            if (m_layeredPlayables.Count == 0)
                return;
            m_remainExitTime -= dt;
            var runtimePlayable = m_layeredPlayablesMap[CurrentPlayableIdInLayer];
            var lastRuntimePlayable = LastPlayableInPlayer == -1 ? runtimePlayable : m_layeredPlayablesMap[LastPlayableInPlayer];
            m_remainExitTime = Mathf.Clamp(m_remainExitTime, 0, runtimePlayable.ExitTime);
            m_weightDiffThisFrame = runtimePlayable.FadeDuration == 0 ? 1 : dt / runtimePlayable.FadeDuration;

            var currentWeight = runtimePlayable.Weight;
            var nextWeight = Mathf.Clamp(currentWeight + m_weightDiffThisFrame, 0, runtimePlayable.TargetWeight);
            var requireAdjustSpeed = lastRuntimePlayable.Id != runtimePlayable.Id && !lastRuntimePlayable.IsAnimatorPlayable && !runtimePlayable.IsAnimatorPlayable;
            var mixClipLengthForTransition = Mathf.Lerp(lastRuntimePlayable.ClipLength, runtimePlayable.ClipLength, nextWeight);

            for (int i = 0; i < m_layeredPlayables.Count; i++)
            {
                var p = m_layeredPlayables[i];
                //CurrentPlaying Playable, fade in
                if (m_layeredPlayables[i].Id == CurrentPlayableIdInLayer)
                {
                    p.Weight += m_weightDiffThisFrame;
                    p.Weight = Mathf.Clamp(p.Weight, 0, p.TargetWeight);
                    p.SmoothWeight = Mathf.Lerp(p.SmoothWeight, p.Weight, 1f - Mathf.Exp(-25f * dt));
                }
                //Other Playables, fade out
                else
                {
                    p.Weight -= m_weightDiffThisFrame;
                    p.Weight = Mathf.Clamp(p.Weight, 0, p.TargetWeight);
                    p.SmoothWeight = Mathf.Lerp(p.SmoothWeight, p.Weight, 1f - Mathf.Exp(-25f * dt));
                }

                if ((m_layeredPlayables[i].Id == LastPlayableInPlayer || m_layeredPlayables[i].Id == CurrentPlayableIdInLayer) && requireAdjustSpeed)
                {
                    p.Playable.SetSpeed(p.ClipLength / mixClipLengthForTransition * p.BaseSpeed);
                }
                m_layeredPlayables[i] = p;
                m_layeredPlayablesMap[m_layeredPlayables[i].Id] = p;
                m_rootPlayable.SetInputWeight(p.OccupiedInputIndex, p.SmoothWeight);
            }
        }

        public void OnPostUpdate(ref PlayableGraph graph)
        {
            for (int i = m_layeredPlayables.Count - 1; i >= 0; i--)
            {
                var p = m_layeredPlayables[i];
                if (m_layeredPlayables[i].Id != CurrentPlayableIdInLayer && p.Type == PlayableInputType.Dynamic && p.Weight < 0.001f && m_layeredPlayablesMap[CurrentPlayableIdInLayer].SmoothWeight > 0.99f)
                {
                    if (m_layeredPlayables[i].Id == LastPlayableInPlayer)
                    {
                        LastPlayableInPlayer = -1;
                    }

                    m_recycledIndexes.Enqueue(p.OccupiedInputIndex);
                    graph.Disconnect(m_rootPlayable, p.OccupiedInputIndex);
                    m_layeredPlayablesMap.Remove(m_layeredPlayables[i].Id);
                    graph.DestroyPlayable(m_layeredPlayables[i].Playable);
                    m_layeredPlayables.RemoveAt(i);
                }
            }
        }
    }

    public class AnimationMixerManager : MonoBehaviour
    {
        public PlayableGraph PlayableGraph;
        private AnimationPlayableOutput m_output;
        private AnimationLayerMixerPlayable m_rootLayerMixerPlayable;
        private List<LayeredPlayablesController> m_layerControllers;
        private List<float> m_targetLayerWeight;
        private List<float> m_smoothedLayerWeight;
        private int m_targetFrameRate;
        [SerializeField]
        private bool m_useCustomFrameRate;
        [SerializeField]
        private int m_customFrameRate = 60;
        private float m_lastEvaluteTime;

        public int TargetFramerate => m_customFrameRate;
        public bool EnabledCustomFPS
        {
            get
            {
                return m_useCustomFrameRate;
            }
            set
            {
                m_useCustomFrameRate = true;
                SetFPS(m_customFrameRate);
            }
        }

        public void SetFPS(float fps)
        {
            m_customFrameRate = (int)fps;
            m_targetFrameRate = m_customFrameRate;
        }

        private void OnValidate()
        {
            if (m_customFrameRate <= 1)
            {
                m_customFrameRate = 1;
            }

            if (m_useCustomFrameRate)
            {
                m_targetFrameRate = m_customFrameRate;
            }

        }
        public void Awake()
        {
            if (m_useCustomFrameRate)
            {
                m_targetFrameRate = m_customFrameRate;
            }
            m_lastEvaluteTime = Time.time;

            m_layerControllers = new List<LayeredPlayablesController>();
            m_smoothedLayerWeight = new List<float>();
            m_targetLayerWeight = new List<float>();
            PlayableGraph = PlayableGraph.Create(gameObject.name + " AnimationMixerManager");
            m_output = AnimationPlayableOutput.Create(PlayableGraph, "Animation", GetComponentInChildren<Animator>());
            m_rootLayerMixerPlayable = AnimationLayerMixerPlayable.Create(PlayableGraph, 0);
            m_output.SetSourcePlayable(m_rootLayerMixerPlayable, 0);
            PlayableGraph.Stop();

            AddLayerControllerToGraph();
        }

        public void AddLayerControllerToGraph()
        {
            var controller = new LayeredPlayablesController();
            var controllerRootPlayable = AnimationMixerPlayable.Create(PlayableGraph, 10);
            controller.SetRootPlayable(controllerRootPlayable);
            m_layerControllers.Add(controller);
            m_targetLayerWeight.Add(1f);
            m_smoothedLayerWeight.Add(1f);

            m_rootLayerMixerPlayable.SetInputCount(m_layerControllers.Count);
            PlayableGraph.Connect(controllerRootPlayable, 0, m_rootLayerMixerPlayable, m_layerControllers.Count - 1);
            m_rootLayerMixerPlayable.SetInputWeight(m_layerControllers.Count - 1, 1);
        }

        public void SetLayerAvatarMask(uint layer, AvatarMask mask)
        {
            m_rootLayerMixerPlayable.SetLayerMaskFromAvatarMask(layer, mask);
        }

        public void SetLayerAdditive(uint layer, bool additive)
        {
            m_rootLayerMixerPlayable.SetLayerAdditive(layer, additive);
        }

        public void UpdateInputModel(int id, AnimationOutputModel model, int layerIndex = 0)
        {
            m_layerControllers[layerIndex].UpdateInputModel(id, model);
        }

        public void AddStaticPlayable(int Id, Playable playable, AnimationOutputModel model, int layerIndex = 0)
        {
            if (layerIndex > m_layerControllers.Count - 1)
                AddLayerControllerToGraph();

            m_layerControllers[layerIndex].AddStaticPlayable(Id, playable, model, ref PlayableGraph);
        }

        public void PlayDynamicPlayable(Playable playable, AnimationOutputModel model, int layerIndex = 0)
        {
            if (layerIndex > m_layerControllers.Count - 1)
                AddLayerControllerToGraph();

            m_layerControllers[layerIndex].PlayDynamicPlayable(playable, model, ref PlayableGraph);
        }

        public void Play(int id, bool force = false, int layerIndex = 0)
        {
            m_layerControllers[layerIndex].Play(id, force);
        }
        public bool IsCurrentPlayable(int id, int layerIndex = 0)
        {
            return m_layerControllers[layerIndex].CurrentPlayableIdInLayer == id;
        }
        
        public int CurrentPlayableId(int layerIndex = 0)
        {
            return m_layerControllers[layerIndex].CurrentPlayableIdInLayer;
        }

        private void Update()
        {
            var time = Time.time;
            var deltaTime = Time.deltaTime;
            for (int i = 0; i < m_layerControllers.Count; i++)
            {
                m_layerControllers[i].OnEvaluate(deltaTime);
                m_smoothedLayerWeight[i] = Mathf.Lerp(m_smoothedLayerWeight[i], m_targetLayerWeight[i], 1f - Mathf.Exp(-12f * deltaTime));
                m_rootLayerMixerPlayable.SetInputWeight(i, m_smoothedLayerWeight[i]);
            }

            if (m_useCustomFrameRate && (time - m_lastEvaluteTime > 1f / m_targetFrameRate))
            {
                var diff = time - m_lastEvaluteTime;
                PlayableGraph.Evaluate(diff);
                m_lastEvaluteTime = time;
            }
            else if (!m_useCustomFrameRate)
            {
                m_lastEvaluteTime = Time.time;
                PlayableGraph.Evaluate(deltaTime);
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < m_layerControllers.Count; i++)
            {
                m_layerControllers[i].OnPostUpdate(ref PlayableGraph);
                if (m_layerControllers[i].IsCurrentPlayableCompleted())
                {
                    m_targetLayerWeight[i] = 0;
                }
                else
                {
                    m_targetLayerWeight[i] = 1;
                }
            }
        }

        public void RemoveMixerFromGraph(Playable playable)
        {
            PlayableGraph.DestroyPlayable(playable);
            m_output.SetSourcePlayable(PlayableGraph.GetRootPlayable(0), 0);
        }

        private void OnDestroy()
        {
            if (PlayableGraph.IsValid())
                PlayableGraph.Destroy();
        }

    }
}