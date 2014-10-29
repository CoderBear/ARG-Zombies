======================================================================
  Flash Deployment Help

  Currently support for the Flash build target is experimental.
  We are sorry for the inconvenience.
======================================================================

To deploy to the Flash platform, please copy your project directory
to a separate deployment location and either:
a) delete the 2DColliderGen directory completely or
b) rename the extension of the 2DColliderGen_Runtime.dll in
   ./2DColliderGen/Plugins/2DColliderGen/ to .old in order to hide
   it from being included in the build.
   
You can then build your Flash target without any further limitations.
Ignore any errors that might occur because of missing scripts.
