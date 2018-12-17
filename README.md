# It's Spidercube!
## Story
Portia is an explorer of the far reaches of space, a mother of eighty-eight, and a spider.  One day, she came across a magnificent discovery - a portal to the very furthest and strangest reaches of space and time.  But at what cost? The rift in spacetime scooped up all eighty-eight of her eggs, and now they're lost in the cold void! Can Portia rescue her babies and get back home?

## Gameplay

Spidercube's main mechanic is that Portia can climb onto any edge of a cube she desires, no matter the gravity.  This lets her gain many different views of the same level in order to plan ahead and beat it effectively.  She'll need to collect three eggs to pass this stage, and not hit the giant monster patrolling the center.  And don't think you can just slice it in two; Portia has no offensive manuevers.  Don't touch that monster, or Portia will be obliterated!

Climbing on each side of a cube is simple.  If Portia goes over an edge of the cube, she will orient herself to be on the next adjacent face in her path.  The camera can be freely manuevered, but it can also adapt to where Portia is located so the geometry doesn't block the action.  However, if you do not prefer this auto-adjust feature, it can be toggled on and off with the Space bar.

Move Portia using WASD.  W moves her forward along the face she is on and in the direction of the camera.  D moves her in the opposite direction.  A and D move left and right along the face.  You can hold down the directional button of choice then move the camera as well.  As long as you're holding your desired direction down, Portia will never go off course.

The up and down arrow keys will rotate the camera up and down.  The left and right arrow keys will do so left and right.  Use the right Shift button to zoom in and Control to zoom out.  The Space bar will toggle auto-adjustment of the camera on and off.

This is a demo level with three eggs to collect.  It is a simple cube.

As of the time of this writing, Portia's model has not properly loaded into Unity.  Hopefully I can solve this before 11:59 tonight.  For now, enjoy the placeholder model!

## Technical Component

This game uses the Unity enging and uses Blender for Portia's model.  This model was made by me.  You can view all the libraries in the zip file, as it's gargantuan and I don't recognize half the stuff in that folder.  The ones I actually used were Mathf, vectors and quaternions.  The music used is "Garden Party" by TRG Banks.  http://freemusicarchive.org/music/TRG_Banks/The_Rainbows_End/Garden_party It is in the public domain but I am not affiliated with the artist and the artist doesn't endorse me.

- Adaptive Camera/Camera Planning
  - The camera in this game will adjust to view any faces that are blocking the player's view of Portia.  The camera will also focus on the center of a platform if Portia is there or will be there, then follow Portia freely if she goes out of the center.
  - The camera's movements are done using a spherical coordinate system.  Shifting the "theta" and "phi" values let the camera orbit around Portia in a circular fashion.  
  - A nice implementation of this feature is prior input and relative direction to the camera.  I made it so even though the camera can give a better view of the player character, Portia will still move in the direction the player intended if that direction is held down.  This is crucial for a 3D platformer/puzzle game, especially one where you go on all sides of an object.
  - In the source code, there's an invisible "fairy" following Portia.  This point in space is where the camera focuses.  By having the fairy lag behind Portia just a little, this creates a more comfortable transition between Portia's locations.

## Video

https://www.youtube.com/watch?v=rUm6uc52FmA&feature=youtu.be
