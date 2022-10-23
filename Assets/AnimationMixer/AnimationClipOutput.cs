using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public abstract class BaseAnimationOutput : MonoBehaviour
{
    [SerializeField]
    protected int LayerIndex = 0;
    [SerializeField]
    protected AvatarMask AvatarMask;
    [SerializeField]
    protected bool IsStatic = false;
    [SerializeField]
    protected AnimationOutputModel Model;
    protected AnimationMixerManager manager;
    protected int m_Id = -1;
    public int Id => m_Id;
    protected abstract Playable ManagerInput { get; }


    private void Start()
    {
        manager = GetComponentInParent<AnimationMixerManager>();
        if (IsStatic)
        {
            CreatePlayables();
            manager.AddStaticPlayable(m_Id, ManagerInput, Model);
        }
    }

    protected abstract void CreatePlayables();
    [ContextMenu("Play")]
    public void Play(bool force = false)
    {
        if (!IsStatic)
        {
            CreatePlayables();
            manager.PlayDynamicPlayable(ManagerInput, Model, LayerIndex);
        }
        else
        {
            manager.Play(m_Id, force, LayerIndex);
        }
        if (LayerIndex != 0 && AvatarMask != null)
        {
            manager.SetLayerAdditive((uint)LayerIndex, false);
            manager.SetLayerAvatarMask((uint)LayerIndex, AvatarMask);
        }
    }
}

public class AnimationClipOutput : BaseAnimationOutput
{
    public AnimationClip ToClip;
    private AnimationClipPlayable toPlayable;
    protected override Playable ManagerInput => toPlayable;

    public bool IsUpdatable()
    {
        return m_Id != -1;
    }

    private void OnValidate()
    {
        if (IsUpdatable())
        {
            manager.UpdateInputModel(m_Id, Model);
        }
    }

    protected override void CreatePlayables()
    {
        toPlayable = AnimationClipPlayable.Create(manager.playableGraph, ToClip);
        toPlayable.SetTime(0f);
        m_Id = Animator.StringToHash(ToClip.name + Time.time.ToString());
    }
}
