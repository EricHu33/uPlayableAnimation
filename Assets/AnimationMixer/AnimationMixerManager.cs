using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct AnimationOutputModel
{
    public float OutputTargetWeight;
    public float FadeInTime;
    public float ExitTime;
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
    public float ExitTime;
    public int OccupiedInputIndex;
}


public interface IAnimationManagerMixerInput
{
    Playable Input { get; }
}


public class LayeredPlayablesController
{
    public int CurrentPlayableIdInLayer;
    private AnimationMixerPlayable rootPlayable;
    private Dictionary<int, RuntimeInputData> layeredPlayablesMap = new Dictionary<int, RuntimeInputData>();
    private List<RuntimeInputData> layeredPlayables = new List<RuntimeInputData>();
    public float remainExitTime;
    private float timeSincePlay;
    private float weightDiffThisFrame;
    private Queue<int> recycledIndexes = new Queue<int>();
    private bool hasStatic;

    public void SetRootPlayable(AnimationMixerPlayable playable)
    {
        rootPlayable = playable;
    }

    public void AddStaticPlayable(int id, Playable playable, AnimationOutputModel model, ref PlayableGraph graph)
    {
        hasStatic = true;
        var runtimeData = new RuntimeInputData
        {
            Id = id,
            Playable = playable,
            Weight = 0,
            SmoothWeight = 0,
            RestartWhenPlay = model.RestartWhenPlay,
            TargetWeight = model.OutputTargetWeight,
            FadeDuration = model.FadeInTime,
            ExitTime = model.ExitTime,
            Type = PlayableInputType.Static,
            OccupiedInputIndex = layeredPlayables.Count,
        };
        layeredPlayables.Add(runtimeData);
        layeredPlayablesMap.Add(id, runtimeData);
        graph.Connect(playable, 0, rootPlayable, layeredPlayables.Count - 1);
        Play(id, true);
    }

    public void PlayDynamicPlayable(Playable playable, AnimationOutputModel model, ref PlayableGraph graph)
    {
        if (remainExitTime <= 0)
        {
            var id = playable.GetHashCode();
            var portIndex = recycledIndexes.Count > 0 ? recycledIndexes.Dequeue() : layeredPlayables.Count;
            if (portIndex > rootPlayable.GetInputCount() - 1)
            {
                return;
            }
            timeSincePlay = 0;
            playable.SetTime(0);
            var runtimeData = new RuntimeInputData
            {
                Id = id,
                Playable = playable,
                Weight = 0,
                SmoothWeight = 0,
                RestartWhenPlay = model.RestartWhenPlay,
                TargetWeight = model.OutputTargetWeight,
                FadeDuration = model.FadeInTime,
                ExitTime = model.ExitTime,
                Type = PlayableInputType.Dynamic,
                OccupiedInputIndex = portIndex,
            };
            layeredPlayables.Add(runtimeData);
            layeredPlayablesMap.Add(id, runtimeData);
            graph.Connect(playable, 0, rootPlayable, portIndex);
            rootPlayable.SetInputWeight(portIndex, 0);
            Play(id, true);
        }
    }

    public void Play(int id, bool force = false)
    {
        if (!force && remainExitTime > 0)
            return;
        CurrentPlayableIdInLayer = layeredPlayablesMap[id].Id;
        if (layeredPlayablesMap[id].RestartWhenPlay)
        {
            layeredPlayablesMap[id].Playable.SetTime(0);
        }
        remainExitTime = layeredPlayablesMap[id].ExitTime;
        timeSincePlay = 0;
    }

    public bool IsCurrentPlayableCompleted()
    {
        if (layeredPlayables.Count == 0)
            return true;
        if (hasStatic)
            return false;
        return timeSincePlay >= layeredPlayablesMap[CurrentPlayableIdInLayer].Playable.GetDuration();
    }

    public bool IsCurrentPlaying(int id)
    {
        return CurrentPlayableIdInLayer == id;
    }

    public void UpdateInputModel(int id, AnimationOutputModel model)
    {
        var p = layeredPlayablesMap[id];
        p.TargetWeight = model.OutputTargetWeight;
        p.FadeDuration = model.FadeInTime;
        p.ExitTime = model.ExitTime;
        p.RestartWhenPlay = model.RestartWhenPlay;
        layeredPlayablesMap[id] = p;
        for (int i = 0; i < layeredPlayables.Count; i++)
        {
            if (layeredPlayables[i].Id == p.Id)
            {
                layeredPlayables[i] = p;
            }
        }
    }

    public void OnEvaluate(float dt)
    {
        if (layeredPlayables.Count == 0)
            return;
        timeSincePlay += dt;
        remainExitTime -= dt;
        var runtimePlayable = layeredPlayablesMap[CurrentPlayableIdInLayer];
        remainExitTime = Mathf.Clamp(remainExitTime, 0, runtimePlayable.ExitTime);
        weightDiffThisFrame = runtimePlayable.FadeDuration == 0 ? 1 : dt / runtimePlayable.FadeDuration;

        for (int i = 0; i < layeredPlayables.Count; i++)
        {
            var p = layeredPlayables[i];
            if (layeredPlayables[i].Id == CurrentPlayableIdInLayer)
            {
                p.Weight += weightDiffThisFrame;
                p.Weight = Mathf.Clamp(p.Weight, 0, p.TargetWeight);
                p.SmoothWeight = Mathf.Lerp(p.SmoothWeight, p.Weight, 1f - Mathf.Exp(-25f * dt));
            }
            else
            {
                p.Weight -= weightDiffThisFrame;
                p.Weight = Mathf.Clamp(p.Weight, 0, p.TargetWeight);
                p.SmoothWeight = Mathf.Lerp(p.SmoothWeight, p.Weight, 1f - Mathf.Exp(-25f * dt));
            }
            layeredPlayables[i] = p;
            layeredPlayablesMap[layeredPlayables[i].Id] = p;
            rootPlayable.SetInputWeight(p.OccupiedInputIndex, p.SmoothWeight);
        }
    }

    public void OnPostUpdate(ref PlayableGraph graph)
    {
        for (int i = layeredPlayables.Count - 1; i >= 0; i--)
        {
            var p = layeredPlayables[i];
            if (layeredPlayables[i].Id != CurrentPlayableIdInLayer && p.Type == PlayableInputType.Dynamic && p.Weight < 0.001f && layeredPlayablesMap[CurrentPlayableIdInLayer].SmoothWeight > 0.99f)
            {
                recycledIndexes.Enqueue(p.OccupiedInputIndex);
                graph.Disconnect(rootPlayable, p.OccupiedInputIndex);
                layeredPlayablesMap.Remove(layeredPlayables[i].Id);
                graph.DestroyPlayable(layeredPlayables[i].Playable);
                layeredPlayables.RemoveAt(i);
            }
        }
    }
}

public class AnimationMixerManager : MonoBehaviour
{
    public PlayableGraph playableGraph;
    public AnimationPlayableOutput output;
    public AnimationLayerMixerPlayable rootLayerMixerPlayable;
    private List<LayeredPlayablesController> layerControllers;
    private List<float> targetLayerWeight;
    private List<float> smoothedLayerWeight;

    public void Awake()
    {
        layerControllers = new List<LayeredPlayablesController>();
        smoothedLayerWeight = new List<float>();
        targetLayerWeight = new List<float>();
        playableGraph = PlayableGraph.Create(gameObject.name + " AnimationMixerManager");
        output = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponentInChildren<Animator>());
        rootLayerMixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 0);
        output.SetSourcePlayable(rootLayerMixerPlayable, 0);
        playableGraph.Stop();

        AddLayerControllerToGraph();
    }

    public void AddLayerControllerToGraph()
    {
        var controller = new LayeredPlayablesController();
        var controllerRootPlayable = AnimationMixerPlayable.Create(playableGraph, 10);
        controller.SetRootPlayable(controllerRootPlayable);
        layerControllers.Add(controller);
        targetLayerWeight.Add(1f);
        smoothedLayerWeight.Add(1f);

        rootLayerMixerPlayable.SetInputCount(layerControllers.Count);
        playableGraph.Connect(controllerRootPlayable, 0, rootLayerMixerPlayable, layerControllers.Count - 1);
        rootLayerMixerPlayable.SetInputWeight(layerControllers.Count - 1, 1);
    }

    public void SetLayerAvatarMask(uint layer, AvatarMask mask)
    {
        rootLayerMixerPlayable.SetLayerMaskFromAvatarMask(layer, mask);
    }

    public void SetLayerAdditive(uint layer, bool additive)
    {
        rootLayerMixerPlayable.SetLayerAdditive(layer, additive);
    }

    public void UpdateInputModel(int id, AnimationOutputModel model, int layerIndex = 0)
    {
        layerControllers[layerIndex].UpdateInputModel(id, model);
    }

    public void AddStaticPlayable(int Id, Playable playable, AnimationOutputModel model, int layerIndex = 0)
    {
        if (layerIndex > layerControllers.Count - 1)
            AddLayerControllerToGraph();

        layerControllers[layerIndex].AddStaticPlayable(Id, playable, model, ref playableGraph);
    }

    public void PlayDynamicPlayable(Playable playable, AnimationOutputModel model, int layerIndex = 0)
    {
        if (layerIndex > layerControllers.Count - 1)
            AddLayerControllerToGraph();

        layerControllers[layerIndex].PlayDynamicPlayable(playable, model, ref playableGraph);
    }

    public void Play(int id, bool force = false, int layerIndex = 0)
    {
        layerControllers[layerIndex].Play(id, force);
    }
    public bool IsCurrentPlayable(int id, int layerIndex = 0)
    {
        return layerControllers[layerIndex].CurrentPlayableIdInLayer == id;
    }

    private void Update()
    {
        for (int i = 0; i < layerControllers.Count; i++)
        {
            layerControllers[i].OnEvaluate(Time.deltaTime);
            smoothedLayerWeight[i] = Mathf.Lerp(smoothedLayerWeight[i], targetLayerWeight[i], 1f - Mathf.Exp(-12f * Time.deltaTime));
            rootLayerMixerPlayable.SetInputWeight(i, smoothedLayerWeight[i]);
        }
        playableGraph.Evaluate(Time.deltaTime);
    }

    private void LateUpdate()
    {
        for (int i = 0; i < layerControllers.Count; i++)
        {
            layerControllers[i].OnPostUpdate(ref playableGraph);
            if (layerControllers[i].IsCurrentPlayableCompleted())
            {
                targetLayerWeight[i] = 0;
            }
            else
            {
                targetLayerWeight[i] = 1;
            }
        }
    }

    public void RemoveMixerFromGraph(Playable playable)
    {
        playableGraph.DestroyPlayable(playable);
        output.SetSourcePlayable(playableGraph.GetRootPlayable(0), 0);
    }

    private void OnDestroy()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
    }

}
