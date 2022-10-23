using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomClipPicker : MonoBehaviour
{
    public AnimationClip[] Clips;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void PickRandomClip()
    {
        GetComponent<AnimationClipOutput>().ToClip = Clips[Random.Range(0, Clips.Length)];
    }
}
