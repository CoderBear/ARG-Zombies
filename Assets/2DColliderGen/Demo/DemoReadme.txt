=====================================
  2D ColliderGen - Demo Scene Help
=====================================

Demo scenes come in two versions for each 2D framework:
- a _Final version to demonstrate what the scene behaves like when all colliders
  have been added and
- a _Raw version for you to experiment with and practice adding and tweaking
  your own AlphaMeshColliders.

--------------------------------------------------------
Workflow at a _Raw scene with SmoothMoves and Orthello
--------------------------------------------------------
Note: Rigidbody Physics Components have already been added to movable objects
      (found in the "Movable Objects" group in the scene) for you.

1) Select all objects that you wish to add a generated MeshCollider to.

2) Choose "2D ColliderGen" - "Add AlphaMeshCollider" from the menu bar.
   Colliders will be generated automatically.
   An AlphaMeshColliderRegistry object is automatically added to the scene
   which keeps track of all AlphaMeshColliders and offers convenient access.
   
3) Select objects where you are unhappy with the collider shape and tweak the
   "Outline Vertex Count" parameter of the AlphaMeshCollider component.
   
4) The movable objects don't collide with other MeshCollider objects yet -
   one of two colliding MeshColliders need to be declared "convex" to enable
   collision testing:
 4.1) Select the box and rolling_rock objects and tick "Force Convex" at the
      AlphaMeshCollider component.
 4.2) Tick "Convex" at the MeshCollider component.
      Now they are ready for collisions.

5) When using an OTSpriteBatch (Orthello Pro framework only):
   Make sure to untick "Deactivate Sprites" at the OTSpriteBatch component,
   otherwise the MeshColliders will be disabled at runtime.
	  
You are done - enjoy physics :-).

--------------------------------------------------------
Workflow at a _Raw scene with 2D Toolkit
--------------------------------------------------------
Note: Rigidbody Physics Components have already been added to movable objects
      (found in the "Movable Objects" group in the scene) for you.

1) Open ColliderGen TK2D Window.
   ("2D ColliderGen" - "2D Toolkit Specific" - "Show ColliderGen TK2D Window")
	  
2) Open a Sprite Collection in the Sprite Collection Editor window.
   (In 'Project' window select e.g."Demo/Demo2DToolkit/ForegroundCollection_Raw"
    then in Inspector window hit "Open Editor...")
   
3) Select the sprites in the Sprite Collection Editor window that you wish to
   add colliders to.

4) Hit "Update Collider" in the ColliderGen TK2D window to automatically
   generate polygon colliders for all of them.
   
5) Select sprites where you are unhappy with the collider shape and tweak the
   "Outline Vertex Count" parameter.

6) The movable objects don't collide with other MeshCollider objects yet -
   one of two colliding MeshColliders need to be declared "convex" to enable
   collision testing:
 6.1) Select the box sprite in the Sprite Collection Editor
      window and tick "Force Convex" at the ColliderGen TK2D window.
 6.2) Tick "Convex" at the Sprite Collection Editor window.
 6.3) Repeat (6.1) and (6.2) for the rolling_rock sprite.
      Now they are ready for collisions.
	  
7) Close the Sprite Collection Editor window first, then the ColliderGen TK2D
   window. (The Sprite Collection Editor window won't close as long as the
   ColliderGen TK2D window is open.)

8) Commit the changes in the Sprite Collection Editor window.
	  
You are done - enjoy physics :-).
