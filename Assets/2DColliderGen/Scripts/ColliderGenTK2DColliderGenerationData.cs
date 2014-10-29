using UnityEngine;
using System.Collections;

//-------------------------------------------------------------------------
/// <summary>
/// Class to group general (shared for all regions) parameters for
/// use in GenerateColliderTK2DHelper class. Needed in the scripts directory
/// instead of the editor directory to be able to attach it to a
/// prefab-savetodisk-object.
/// </summary>
[System.Serializable]
public class RegionIndependentParametersTK2D : RegionIndependentParametersBase {
	// No additional variables needed.
	
	// Default constructor.
	public RegionIndependentParametersTK2D() : base() {}
	// Deep-copy constructor.
	public RegionIndependentParametersTK2D(RegionIndependentParametersTK2D src) : base(src) {}
}

//-------------------------------------------------------------------------
/// <summary>
/// Class to group collider region specific parameters for
/// use in GenerateColliderTK2DHelper class. Needed in the scripts directory
/// instead of the editor directory to be able to attach it to a
/// prefab-savetodisk-object.
/// </summary>
[System.Serializable]
public class ColliderRegionParametersTK2D : ColliderRegionParametersBase {
	// No additional parameters needed for now.
	
	// Default constructor.
	public ColliderRegionParametersTK2D() : base() {}
	// Deep-copy constructor.
	public ColliderRegionParametersTK2D(ColliderRegionParametersTK2D src) : base(src) {}
}
