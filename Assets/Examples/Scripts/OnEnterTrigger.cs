using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UPlayable.AnimationMixer.Example
{
    public class OnEnterTrigger : MonoBehaviour
    {
        public UnityEvent OnEnter;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private IEnumerator FlashColor()
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
            yield return new WaitForSeconds(0.35f);
            GetComponent<MeshRenderer>().material.color = Color.white;
        }

        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(FlashColor());
            OnEnter?.Invoke();
        }
    }
}