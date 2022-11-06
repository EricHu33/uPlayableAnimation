# uPlayableAnimation(unity-playable-Animation) Project
<img src="https://user-images.githubusercontent.com/13420668/200156802-9478f7e0-3722-4764-894e-5117135d2360.png" width="600"> <img src="https://user-images.githubusercontent.com/13420668/197787147-9e4aec68-0728-4ff9-b5cb-0c1e6109ca1c.gif" width="400">

While Unity's animator controller's node base approach is visually straightforward.
It did not scale well when the number of animation states start to pile up.

This project aim to create scripts that can help to avoid working on ever growing, over complex animator controller in Unity.
By using Unity's [Playables API](https://docs.unity3d.com/Manual/Playables.html), we can minimize the usage of animator controller & moving all animation logic into mono behaviour. We can make the whole animation handling logic more reusable/scalable

Simply drag & drop the provided output component on animated game object. The animator can easily blend into any state at runtime. 

***This project is still work-in-progress.*** 

-----

## What this project provide

 - Transition into different animation clip without wiring the animator controller over and over again.
 - Assign/Transition animation at runtime.
 - Avatar mask (layer-animation) support.
 - Custom animation framerate at runtime
 - Custom animation speed at runtime
 - Support transition between exist animator controller at runtime.
 - Custom smooth blending time for each animation clop at runtime.

-----
# Showcase

### Play different animation clip and different animator controller at runtime
![ezgif-5-ab446a8f1d](https://user-images.githubusercontent.com/13420668/200158277-8ec630d2-d0fb-489c-b15d-8e8d27adc033.gif)

> Under the hood. The ***AnimationMixerManager*** create a PlayableGraph and will handle all the playable input/output inside the graph. 
 ***AnimationOutput***'s role is very simple - prepare animation settings/clip and ask the manager to play it. While the manager will handle all the life cycle of all playables on the graph. 
 
### Customize animation framerate & animation clip speed at runtime
 
![ezgif-5-3e6e88577d](https://user-images.githubusercontent.com/13420668/200158842-e75f9c94-01a5-4411-8c23-a84096fe4564.gif)

> For performance and other reason, you can adjust framerate of each animator at runtime. each clip's speed is customizable as well
> Set speed to nagative will play the clip in backward!

### Also works for non-character animator
![ezgif-5-da099ab765](https://user-images.githubusercontent.com/13420668/200158936-22976b95-8af6-4801-981b-b6823d032b3d.gif)


### Support Avatar Mask(Layerd animation) 
![ezgif-5-d316fb8c4c](https://user-images.githubusercontent.com/13420668/200158862-6f5584de-eca0-448b-8bf1-9581e982eba5.gif)
> The script also handle layer blending through Playables API.

-----

# Setup/Basic usage - Animation Clip Output

First of all, prepare a character/object with animator. And leave the animator controller as empty.

 - Drag the ***AnimationClipOutput*** component next to your character's animator
 - Setup the clip you want and call the ***Play()*** method (for convenience, you can also use the Play button on the component inspector).

<img src="https://user-images.githubusercontent.com/13420668/197769571-a9d87ad7-1412-45d9-89e3-c983ef7c2f45.png" width="400">

There are 2 approaches to blend into different animation clip.

 1. Assign the ToClip variable of the ***Output*** component with animation clip you want at runtime (see below video), then invoke the Play method.

![ezgif-1-55c94e60bf](https://user-images.githubusercontent.com/13420668/197914750-8e2b85e5-96c1-4439-8bbb-6a22e01f0b3a.gif)

2. Place many ***Output*** components on your animator object where each ***Output*** has assigned with different animation clip. And invoke the Play method of your desired ***Output***. The manager script will handle all the blending for you.

![ezgif-1-14adca54ea](https://user-images.githubusercontent.com/13420668/197914729-0dbf31a3-20d4-414c-a649-b7df52ddb89f.gif)

Either approach are fine. Choose whats more comfortable for you.

-----

## IsStatic flag

![image](https://user-images.githubusercontent.com/13420668/197807707-9eb82316-9707-4cb3-b1dc-052f8ee6cdbc.png)

Most animation are likely to be Dynamic(fire and forgot). In this case you need to set ***IsStatic*** to ***false***.
For dynamic ***Output*** component, the manager will destroy the correspond playable on the graph once the animator jump to other state.

In other words, each time you invoke Play() on the dynamic ***Output*** component. The manager will create a new playable and blend into it (Then, kill the old playable).
You can swap dynamic ***Output***'s animation clip & setting at runtime. The new setup will take effect once you invoke Play() again.

Some animation state are design to be permanently on the animator and better keep it on the playable graph - like Idle animation.
If you set the ***IsStatic*** toggle to true. the ***Output*** component's playable animation will persist on playable graph. The downside is you can't change the clip on the component at runtime. But replay the animation has less overhead then dynamic one.

-----

## Avatar mask setup
<img src="https://user-images.githubusercontent.com/13420668/197806169-3f9cd5e0-e99a-472e-909a-685d578e122b.png" height="300"> <img src="https://user-images.githubusercontent.com/13420668/197805717-72db725e-dfc1-4819-a49c-e2b0e2f4d965.png" height="300">

By assign layer index inside the ***Output*** component. The manager will create a [AnimationLayerMixerPlayable](https://docs.unity3d.com/ScriptReference/Animations.AnimationLayerMixerPlayable.html) on it's own playable graph to handle the blending between different layer.

![image](https://user-images.githubusercontent.com/13420668/197807237-8d2d0f29-58ac-44f2-a36b-ab8f6008932b.png)
![image](https://user-images.githubusercontent.com/13420668/197807343-1767938b-cd9a-4289-a5f0-fa6d7f9b4486.png)

***Be careful not to have different avatar mask assign to a same layer, which will break the system.***

-----

## Animator Output

![image](https://user-images.githubusercontent.com/13420668/198029098-0c5113de-1a0a-4f2a-a9a1-02252e25360a.png)

In some cases you might need to blend your animator into a exist animator controller. ***AnimatorOutput*** can make the animator transition to specific animtiion controller. This even work with transition between animation clips and different animator controller at runtime.

![ezgif-3-80e3c11651](https://user-images.githubusercontent.com/13420668/198029824-1959b864-2290-4b67-b1f0-1bc6bcf062a9.gif)

***Above gif showcase mix usage of animationClipOutput and animatorOutput.***, notice the playable graph and you can see how the animationMixerManager just handle every playable automatically.


-----

## Animator Mixer Output
<img src="https://user-images.githubusercontent.com/13420668/198034568-c7c500eb-b632-4e33-8b25-43e9d60b47b3.png" height="300">

While ***AnimatorOutput*** let you transition into different animator controller. Sometimes you might need a more precise control over the transition weight between a animation clip and animator controller. For example an IDLE animation and a animator controller that using a 2D blend tree to perform strafe locomotion.
***AnimatorMixerOutput*** let you setup a animation clip and animator controller, By controll the exposed Weight variable you can 

![ezgif-3-10d7762ef7](https://user-images.githubusercontent.com/13420668/198037043-892ac160-1441-4564-84d6-aa4917b7b71f.gif)

Check the 6th example scene's player control unity-chan and see how it setup.

 
-----

## TODO

 - Improve the component's custom editor
 - Support unity animation rigging pacakage
 - More/Better example (WIP)
 - Clearer document
 - ~~Expose playable speed for easier customize~~ (Done)
 - ~~Custom framerate for each animator~~ (Done)
 
-----

## License

Code under UPlayableAnimation are MIT. 

But all the unity-chan! related asset inside the Example folder are under [UCL-2.0](https://unity-chan.com/contents/license_en/)

