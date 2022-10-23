using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimatorMixerOutput : BaseAnimationOutput
{
    public AnimationClip FromClip;
    public RuntimeAnimatorController animCtrl;
    [Range(0, 1f)]
    public float Weight;
    private AnimationMixerPlayable mixerPlayable;
    private AnimationClipPlayable fromPlayable;
    private AnimatorControllerPlayable toPlayable;
    public Vector2 Direction;
    protected override Playable ManagerInput => mixerPlayable;

    public bool IsUpdatable()
    {
        return Id != -1;
    }

    private void OnValidate()
    {
        if (IsUpdatable())
        {
            manager.UpdateInputModel(Id, Model);
        }
    }

    private void Update()
    {
        if (!mixerPlayable.IsValid())
            return;
        Weight = Mathf.Clamp01(Weight);
        mixerPlayable.SetInputWeight(0, 1.0f - Weight);
        mixerPlayable.SetInputWeight(1, Weight);
        GetComponent<Animator>().SetFloat("Horizontal", Direction.x);
        GetComponent<Animator>().SetFloat("Vertical", Direction.y);
    }

    protected override void CreatePlayables()
    {
        mixerPlayable = AnimationMixerPlayable.Create(manager.playableGraph, 2);
        fromPlayable = AnimationClipPlayable.Create(manager.playableGraph, FromClip);
        toPlayable = AnimatorControllerPlayable.Create(manager.playableGraph, animCtrl);
        fromPlayable.SetTime(0f);

        manager.playableGraph.Connect(fromPlayable, 0, mixerPlayable, 0);
        manager.playableGraph.Connect(toPlayable, 0, mixerPlayable, 1);
        m_Id = Animator.StringToHash(FromClip.name + animCtrl.name + Time.time.ToString());
    }
}
