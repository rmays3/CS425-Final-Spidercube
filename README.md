# It's Spidercube!
## Story
Portia is an explorer of the far reaches of space, a mother of eighty-eight, and a spider.  One day, she came across a magnificent discovery - a portal to the very furthest and strangest reaches of space and time.  But at what cost? The rift in spacetime scooped up all eighty-eight of her eggs, and now they're lost in the cold void! Can Portia rescue her babies and get back home? Or will the bizarre, abstract monsters of space get her first?

## Gameplay

Spidercube's main mechanic is that Portia can climb onto any edge of a cube she desires, no matter the gravity.  This lets her gain many different views of the same level in order to plan ahead and beat it effectively.  She'll need to collect three eggs to pass this stage, and not hit the giant monster patrolling the center.  And don't think you can just slice it in two; Portia has no offensive manuevers.  Don't touch that monster, or Portia will be obliterated!

Climbing on each side of a cube is simple.  If Portia goes over an edge of the cube, she will orient herself to be on the next adjacent face in her path.  The camera can be freely manuevered, but it can also adapt to where Portia is located so the geometry doesn't block the action.  However, if you do not prefer this auto-adjust feature, it can be toggled on and off with the Space bar.

Move Portia using WASD.  W moves her forward along the face she is on and in the direction of the camera.  D moves her in the opposite direction.  A and D move left and right along the face.  You can hold down the directional button of choice then move the camera as well.  As long as you're holding your desired direction down, Portia will never go off course.

The up and down arrow keys will rotate the camera up and down.  The left and right arrow keys will do so left and right.  Use the right Shift button to zoom in and Control to zoom out.  The Space bar will toggle auto-adjustment of the camera on and off.

This is a demo level with three eggs to collect.  It is a simple cube.

As of the time of this writing, Portia's model has not properly loaded into Unity.  Hopefully I can solve this before 11:59 tonight.  For now, enjoy the placeholder model!

## Technical Component

This game uses the Unity enging and uses Blender for Portia's model.  This model was made by me.  You can view all the libraries in the zip file, as it's gargantuan and I don't recognize half the stuff in that folder.  The ones I actually used were Mathf, vectors and quaternions.

- Adaptive Camera/Camera Planning
 
