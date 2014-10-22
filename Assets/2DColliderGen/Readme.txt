================================================================================
  2D ColliderGen - Help
================================================================================

Thank you for choosing 2D ColliderGen!
This framework allows you to automatically generate precise polygon colliders
for your game objects from a sprite image's alpha-channel.
The colliders are created within the Unity editor and add no additional
performance cost at runtime.
It integrates with the 2D Toolkit, SmoothMoves and Orthello 2D frameworks
as well as the built-in Unity 4.3 sprites.

Below you find an explanation of the typical workflow for each framework.
For an extensive documentation and video tutorials, please visit
http://pixelcloudgames.com/tools/2d-collidergen/documentation.

--------------------------------------------------------------------------------
Workflow at a typical scene with Unity4.3 Sprites, SmoothMoves or Orthello
--------------------------------------------------------------------------------
When using the SmoothMoves and Orthello frameworks, the workflow is centered
around adding AlphaMeshCollider components to your game objects which will
generate MeshColliders or PolygonCollider2D. At these components you can tweak
parameters such as the number of outline vertices, convexity of the mesh, etc.

Please visit the reference page at
http://pixelcloudgames.com/tools/2d-collidergen/documentation
for further information on all parameters or to watch the video tutorials which
demonstrate the effect of the different parameters.

1) Add 2D sprites to the scene.
   We assume that you already know how to add and set up 2D sprites.
   If you are not familiar with the SmoothMoves or Orthello frameworks yet, we
   recommend to check out the documentation of these great packages first.

2) Prepare movable game objects by adding Rigidbody or Rigidbody2D physics
   components to them. Without Rigidbody components, your objects will remain
   static. 

3) Select all objects that you wish to add a generated collider to.

4) Choose "2D ColliderGen" - "Add AlphaMeshCollider" from the menu bar.
   Colliders will be generated automatically.
   An AlphaMeshColliderRegistry object is automatically added to the scene
   which keeps track of all AlphaMeshColliders and offers convenient access.
   
5) Select objects where you are unhappy with the collider shape and tweak the
   "Outline Vertex Count" parameter of the AlphaMeshCollider component.
   
6) The movable objects don't collide with other MeshCollider objects yet -
   one of two colliding MeshColliders needs to be declared "convex" to enable
   collision testing:
 6.1) Select the movable objects and tick "Force Convex" at the
      AlphaMeshCollider component.
 6.2) Tick "Convex" at the MeshCollider component.
      Now they are ready for collisions.

7) When using an OTSpriteBatch (Orthello Pro framework only):
   Make sure to untick "Deactivate Sprites" at the OTSpriteBatch component,
   otherwise the MeshColliders will be disabled at runtime.
	  
You are done - enjoy physics.

--------------------------------------------------------------------------------
Additional workflow with animated Unity4.3 Sprites
--------------------------------------------------------------------------------

1) Select all objects that you wish to add animated colliders to.

2) Choose "2D ColliderGen" - "Add AlphaMeshCollider" from the menu bar.
   A collider for the first frame of the animation will be generated
   automatically. A RuntimeAnimatedColliderSwitch component is automatically
   added as well.

3) Adjust the parameters of the AlphaMeshCollider to your liking.
   
4) Hit "Recalculate All Frames" to generate colliders for all frames.

5) Hit Play with your object selected to see the animated colliders in action.

--------------------------------------------------------------------------------
Workflow at a typical scene with 2D Toolkit (TK2D)
--------------------------------------------------------------------------------
When using the 2D Toolkit framework, the workflow is centered around an
additional "ColliderGen TK2D" window, which extends the 2D Toolkit
Sprite Collection Editor window by functionality to automatically generate
MeshColliders. This "ColliderGen TK2D" window offers parameters such as
the number of outline vertices, convexity of the mesh, etc.

Please visit the reference page at
http://pixelcloudgames.com/tools/2d-collidergen/documentation
for further information on all parameters or to watch the video tutorials which
demonstrate the effect of the different parameters.

1) Add 2D sprites to the scene.
   We assume that you already know how to add and set up 2D sprites.
   If you are not familiar with the 2D Toolkit framework yet, we recommend to
   check out the documentation of this great framework first.

2) Prepare movable game objects by adding Rigidbody physics components to them.
   Without Rigidbody components, your objects will remain static. 

3) Open the "ColliderGen TK2D" window.
   ("2D ColliderGen" - "2D Toolkit Specific" - "Show ColliderGen TK2D Window")
	  
3) Open a Sprite Collection in the Sprite Collection Editor window.
   (In 'Project' window select your SpriteCollection prefab,
    then in Inspector window hit "Open Editor...")
   
4) Select the sprites in the Sprite Collection Editor window that you wish to
   add colliders to.

5) Hit "Update Collider" in the ColliderGen TK2D window to automatically
   generate polygon colliders for all of them.
   
6) Select sprites where you are unhappy with the collider shape and tweak the
   "Outline Vertex Count" parameter.

7) The movable objects don't collide with other MeshCollider objects yet -
   one of two colliding MeshColliders need to be declared "convex" to enable
   collision testing:
 7.1) Select a movable objects' sprite in the Sprite Collection Editor
      window and tick "Force Convex" at the ColliderGen TK2D window.
 7.2) Tick "Convex" at the Sprite Collection Editor window.
 7.3) Repeat (7.1) and (7.2) for all other movable sprites
      Multi-Object editing is supported however, so you can select multiple
	  sprites at once as well.
   Now the objects are ready for collisions.
	  
8) Close the Sprite Collection Editor window first, then close the
   ColliderGen TK2D window. (The Sprite Collection Editor window won't close as
   long as the ColliderGen TK2D window is open.)

9) Commit the changes in the Sprite Collection Editor window.
	  
You are done - enjoy physics.
