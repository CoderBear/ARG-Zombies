using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

//-------------------------------------------------------------------------
/// <summary>
/// Class to store collider-generation parameters of individual sprites of
/// a tk2d sprite collection in order to restore them when changing the
/// sprite selection or at application restart.
/// Will be persisted as a prefab object in the same directory as the
/// sprite collection data.
/// </summary>
[System.Serializable]
public class ColliderGenTK2DParameterStore : MonoBehaviour {
	
	public const int CURRENT_COLLIDER_GEN_VERSION = 0;
	
	public List<ColliderGenTK2DParametersForSprite> mStoredParameters;
	public int mColliderGenVersion = 0;

	//-------------------------------------------------------------------------
	public ColliderGenTK2DParameterStore() {
		mStoredParameters = new List<ColliderGenTK2DParametersForSprite>();
	}
	
	//-------------------------------------------------------------------------
	public ColliderGenTK2DParametersForSprite GetParametersForSprite(int spriteIndex) {
		
		foreach (ColliderGenTK2DParametersForSprite paramObject in mStoredParameters) {
			if (paramObject.mSpriteIndex == spriteIndex) {
				
				ColliderGenTK2DParametersForSprite deepParametersCopy = new ColliderGenTK2DParametersForSprite(paramObject);
				return deepParametersCopy;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	public void SaveParametersForSprite(int spriteIndex, ColliderGenTK2DParametersForSprite parametersToSave) {
		
		ColliderGenTK2DParametersForSprite deepParametersCopy = new ColliderGenTK2DParametersForSprite(parametersToSave);
		
		for (int count = 0; count < mStoredParameters.Count; ++count) {
			ColliderGenTK2DParametersForSprite paramObject = mStoredParameters[count];
			if (paramObject.mSpriteIndex == spriteIndex) {
				
				mStoredParameters[count] = deepParametersCopy;
				return;
			}
		}
		
		// does not exist yet - add it
		mStoredParameters.Add(deepParametersCopy);
	}
	
	//-------------------------------------------------------------------------
	public void UpdateToCurrentVersionIfNecessary() {
		for (int count = 0; count < mStoredParameters.Count; ++count) {
			ColliderGenTK2DParametersForSprite paramObject = mStoredParameters[count];
			paramObject.UpdateToCurrentVersionIfNecessary();
		}
	}
	
	//-------------------------------------------------------------------------
	public void CopyFrom(ColliderGenTK2DParameterStore src) {
		this.mStoredParameters = src.mStoredParameters;
	}
}
