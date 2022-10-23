using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationMixerOuput : BaseAnimationOutput
{
    public AnimationClip FromClip;
    public AnimationClip ToClip;
    [Range(0, 1f)]
    public float Weight;
    private AnimationMixerPlayable mixerPlayable;
    private AnimationClipPlayable fromPlayable;
    private AnimationClipPlayable toPlayable;

    protected override Playable ManagerInput => mixerPlayable;

    private void OnValidate()
    {
        if (IsUpdatable())
        {
            manager.UpdateInputModel(m_Id, Model);
        }
    }

    private void Update()
    {
        if (!mixerPlayable.IsValid())
            return;
        Weight = Mathf.Clamp01(Weight);
        mixerPlayable.SetInputWeight(0, 1.0f - Weight);
        mixerPlayable.SetInputWeight(1, Weight);
        float mixLength = Mathf.Lerp(FromClip.length, ToClip.length, Weight);
        fromPlayable.SetSpeed(FromClip.length / mixLength);
        toPlayable.SetSpeed(ToClip.length / mixLength);
    }

    public bool IsUpdatable()
    {
        return Id != -1;
    }

    protected override void CreatePlayables()
    {
        mixerPlayable = AnimationMixerPlayable.Create(manager.playableGraph, 2);
        fromPlayable = AnimationClipPlayable.Create(manager.playableGraph, FromClip);
        toPlayable = AnimationClipPlayable.Create(manager.playableGraph, ToClip);
        fromPlayable.SetTime(0f);
        toPlayable.SetTime(0f);

        manager.playableGraph.Connect(fromPlayable, 0, mixerPlayable, 0);
        manager.playableGraph.Connect(toPlayable, 0, mixerPlayable, 1);
        m_Id = Animator.StringToHash(FromClip.name + ToClip.name + Time.time.ToString());
        if (LayerIndex != 0 && AvatarMask != null)
        {
            manager.SetLayerAdditive((uint)LayerIndex, false);
            manager.SetLayerAvatarMask((uint)LayerIndex, AvatarMask);
        }
    }
}
