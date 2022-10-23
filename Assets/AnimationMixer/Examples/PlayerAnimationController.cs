using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimationController : MonoBehaviour
{
    public AnimatorMixerOutput m_directionMoveMixer;


    public void Start()
    {
    }

    private float smoothWeight;
    private Vector2 smoothInputDir;
    public void Update()
    {

    }
}
