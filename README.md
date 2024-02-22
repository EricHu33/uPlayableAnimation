# uPlayableAnimation (Unity Playable Animation)
<img src="https://user-images.githubusercontent.com/13420668/200156802-9478f7e0-3722-4764-894e-5117135d2360.png" width="600"> <img src="https://user-images.githubusercontent.com/13420668/197787147-9e4aec68-0728-4ff9-b5cb-0c1e6109ca1c.gif" width="400">

While Unity's node based animator is visually straightforward, it does not scale well. The aim of this project is to create a library of scripts that replaces Unity’s animator controller. 

uPlayableAnimation uses  [Playables API](https://docs.unity3d.com/Manual/Playables.html), to minimize the use of the animator controller. Animation logic is added to a MonoBehaviour script. Making animation handling logic more easily reusable and scalable.

Simply drag and drop the provided ***AnimationClipOutput*** script onto a game object that has an animator component (the animator component does not require a controller). At runtime animations can easily be blended into any state.

**This project is still work-in-progress.**

### You can provide your feedback at the [Unity forum post](https://forum.unity.com/threads/uplayableanimation-playableapi-animation-system.1353557/) ###

-----
## How to install

### From source

1. Download ZIP archieve using “Code” green button on the main github page.
2. Copy past “Assets/Plugins/uPlayableAnimation” folder to your project.

### Package Manager

1. Open Package Manager at Unity project.
2. Click on “plus” at left up corner.
3. Choose “Add package from git URL…”, past link https://github.com/{Company}/uPlayableAnimation.git?path=/Assets/Plugins/uPlayableAnimation ****and click “add” button.
4. For more info you could refer to: “[**https://docs.unity3d.com/Manual/upm-git.html](https://docs.unity3d.com/Manual/upm-git.html)”.**

## This project provides

 - Transition into different animation clips without wiring the animator controller over and over again.
 - Assign/Transition animation at runtime.
 - Avatar mask (layer-animation) support.
 - Custom animation frame rate at runtime.
 - Custom animation speed at runtime.
 - Supports crossfade transitions between existing animator controllers at runtime.
 - Custom crossfade time for each animation clip at runtime.
 - FixedTimeOffSet option for offsetting target animation's start frametime when performing a crossfade.
-----
# Showcase

### Play multiple animation clips and on multiple animator controllers at runtime
![ezgif-5-ab446a8f1d](https://user-images.githubusercontent.com/13420668/200158277-8ec630d2-d0fb-489c-b15d-8e8d27adc033.gif)

> The ***AnimationMixerManager*** creates a [PlayableGraph](https://docs.unity3d.com/Manual/Playables-Graph.html) and manages all the playable input and output within the graph. The role of AnimationOutput is to prepare the animation settings and clips and request the manager to play them. The manager will handle the life cycle of all the playables in the graph.
 
### Customize animation frame rate and animation clip speed at runtime
 
![ezgif-5-3e6e88577d](https://user-images.githubusercontent.com/13420668/200158842-e75f9c94-01a5-4411-8c23-a84096fe4564.gif)

> For performance and other reasons, you can adjust the frame rate of each animator at runtime as well as each clip's speed.
> Setting speed to negative will play the clip backwards.

### Also works for non-character animators
![ezgif-5-da099ab765](https://user-images.githubusercontent.com/13420668/200158936-22976b95-8af6-4801-981b-b6823d032b3d.gif)


### Supports Avatar Mask (layered animation)
![ezgif-5-d316fb8c4c](https://user-images.githubusercontent.com/13420668/200158862-6f5584de-eca0-448b-8bf1-9581e982eba5.gif)
> The script handles layer blending through the Playables API.

-----

# Setup/Basic usage - Animation Clip Output

If you’re unfamiliar with [Unity’s Playables API](https://docs.unity3d.com/Manual/Playables-Graph.html), read the documentation. Then install the [Playables Graph Visualizer](https://github.com/Unity-Technologies/graph-visualizer).

Create a game object that be can be a character or any object, then add an Animator component (leave the Animator component empty). 

 - Drag the ***AnimationClipOutput*** component on the game object.
 - Add an animation clip to ***AnimationClipOutput*** and create a script that calls ***Play()*** or for testing simply use the ‘Play’ button on the component.

<img src="https://user-images.githubusercontent.com/13420668/197769571-a9d87ad7-1412-45d9-89e3-c983ef7c2f45.png" width="400">

There are two approaches to blend between animation clips.

 1. To play a specific animation clip at runtime, assign it to the ToClip variable of the AnimationClipOutput component and then invoke the Play method.

![ezgif-1-55c94e60bf](https://user-images.githubusercontent.com/13420668/197914750-8e2b85e5-96c1-4439-8bbb-6a22e01f0b3a.gif)

2. There’s no limit to the number of ***AnimationClipOutput*** components each game object can have. Invoke the Play method on each ***AnimationClipOutput*** and the manager script will handle the blending for you.

![ezgif-1-14adca54ea](https://user-images.githubusercontent.com/13420668/197914729-0dbf31a3-20d4-414c-a649-b7df52ddb89f.gif)

-----

## IsStatic flag

![image](https://user-images.githubusercontent.com/13420668/197807707-9eb82316-9707-4cb3-b1dc-052f8ee6cdbc.png)

For dynamic animations that are played once and then discarded, the ***IsStatic*** field should be set to false. This will cause the manager to destroy the playable animation on the graph once it is finished. In other words, each time the Play() method is invoked on a non-static AnimationClipOutput component, the manager will create a new playable and blend into it, then destroy the old playable.

Animation clips can be swapped at runtime. The new animation will take effect once Play() has been invoked.

Some animation states are meant to be permanently on the animator, such as idle animations. If you set the ***IsStatic*** toggle to true on the ***AnimationClipOutput*** component, the playable animation will persist on the playable graph. The downside is that you can't change the clip on the component at runtime. However, replaying a static animation has less overhead than a dynamic one.

-----

## Avatar mask setup
<img src="https://user-images.githubusercontent.com/13420668/197806169-3f9cd5e0-e99a-472e-909a-685d578e122b.png" height="300"> <img src="https://user-images.githubusercontent.com/13420668/197805717-72db725e-dfc1-4819-a49c-e2b0e2f4d965.png" height="300">

By assigning a layer index inside the ***AnimationClipOutput*** component. The manager will create a [AnimationLayerMixerPlayable](https://docs.unity3d.com/ScriptReference/Animations.AnimationLayerMixerPlayable.html) on its own playable graph to handle the blending between different layers.

![image](https://user-images.githubusercontent.com/13420668/197807237-8d2d0f29-58ac-44f2-a36b-ab8f6008932b.png)
![image](https://user-images.githubusercontent.com/13420668/197807343-1767938b-cd9a-4289-a5f0-fa6d7f9b4486.png)

**Be careful not to assign multiple avatar masks to the same layer, as this will break the system.***

-----

## Animator Output

![image](https://user-images.githubusercontent.com/13420668/198029098-0c5113de-1a0a-4f2a-a9a1-02252e25360a.png)

In some cases, you may need to blend your animator with an existing animator controller. The AnimatorOutput component allows the animator to transition to a specific animation controller, even if the transition occurs at runtime between different animation clips and animator controllers.

![ezgif-3-80e3c11651](https://user-images.githubusercontent.com/13420668/198029824-1959b864-2290-4b67-b1f0-1bc6bcf062a9.gif)

***The above gif demonstrates the use of the AnimationClipOutput and AnimatorOutput components.*** Notice how the AnimationMixerManager automatically manages the playables in the playable graph. This allows for seamless mixing of animations at runtime.


-----

## Animator Mixer Output
<img src="https://user-images.githubusercontent.com/13420668/198034568-c7c500eb-b632-4e33-8b25-43e9d60b47b3.png" height="300">

The AnimatorOutput component allows you to transition to different animator controllers. However, sometimes you may need more precise control over the transition weight between an animation clip and an animator controller. For example, you may have an idle animation and an animator controller that uses a 2D blend tree for strafe locomotion. In this case, the AnimatorMixerOutput component allows you to set up an animation clip and an animator controller, and then control the transition weight between them using the exposed Weight variable.

![ezgif-3-10d7762ef7](https://user-images.githubusercontent.com/13420668/198037043-892ac160-1441-4564-84d6-aa4917b7b71f.gif)

Check the 6th example scene's player control unity-chan and see how it setup.

 
-----

## TODO

 - Improve the component's custom editor.
 - Support unity animation rigging package.
 - More/better examples (WIP).
 - Clearer documentation.
 - ~~Expose playable speed for easier customization ~~ (Done)
 - ~~Custom frame rate for each animator~~ (Done)
 
-----

## License

Code under UPlayableAnimation is MIT.

All the unity-chan! related assets inside the Example folder are under [UCL-2.0](https://unity-chan.com/contents/license_en/)



