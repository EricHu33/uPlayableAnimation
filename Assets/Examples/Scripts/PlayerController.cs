using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UPlayable.AnimationMixer;

namespace UPlayable.AnimationMixer.Example
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float Speed;
        public float Gravity = -10f;
        public AnimatorMixerOutput directionMoveMixer;
        private CharacterController characterController;
        private Vector3 velocity;
        private Transform cam;


        public void Start()
        {
            characterController = GetComponent<CharacterController>();
            cam = Camera.main.transform;
        }

        private float smoothWeight;
        private Vector2 smoothInputDir;
        public void Update()
        {
            if (directionMoveMixer.Id != -1 && !GetComponentInChildren<AnimationMixerManager>().IsCurrentPlayable(directionMoveMixer.Id))
            {
                GetComponentInChildren<AnimationMixerManager>().Play(directionMoveMixer.Id);
            }
            var inputMovement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            inputMovement = Vector3.ClampMagnitude(inputMovement, 1f);

            if (characterController.isGrounded)
            {
                var camRelativeMovement = Vector3.ProjectOnPlane(cam.rotation * inputMovement, Vector3.up);
                var targetVelocity = camRelativeMovement * Speed;
                velocity.y = 0;
                velocity = Vector3.Lerp(velocity, targetVelocity, 1f - Mathf.Exp(-25f * Time.deltaTime));
            }
            else
            {
                velocity.y += Gravity * Time.deltaTime;
            }

            characterController.Move(velocity * Time.deltaTime);
            smoothInputDir = Vector2.Lerp(smoothInputDir, new Vector2(inputMovement.x, inputMovement.z), 1f - Mathf.Exp(-8f * Time.deltaTime));
            smoothWeight = Mathf.Lerp(smoothWeight, new Vector2(inputMovement.x, inputMovement.z).magnitude != 0 ? 1 : 0, 1f - Mathf.Exp(-8f * Time.deltaTime));
            directionMoveMixer.Direction = smoothInputDir;
            directionMoveMixer.Weight = smoothWeight;
        }
    }
}