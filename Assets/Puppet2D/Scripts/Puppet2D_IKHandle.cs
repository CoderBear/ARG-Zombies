using UnityEngine;
using System.Collections;


public class Puppet2D_IKHandle: MonoBehaviour 
{

	public Transform poleVector;

    public Vector3 AimDirection;

    public Vector3 UpDirection;

	public bool Flip,SquashAndStretch, Scale;

    public Vector3[] scaleStart =new Vector3[2];

    public Transform topJointTransform, middleJointTransform, bottomJointTransform;

	public Vector3 OffsetScale = new Vector3(1,1,1);


    private Transform IK_CTRL;

    private Vector3 root2IK ;
    private Vector3 root2IK2MiddleJoint;

	private bool LargerMiddleJoint;


    /*
	void LateUpdate () 
    {
		if (!IsEnabled)
        {
			return;
		}
		CalculateIK();
	}
	*/
	public void CalculateIK()
    {
        int flipRotation;
        if (Flip)
            flipRotation = 1;
        else
            flipRotation = -1;
        IK_CTRL = transform;


        //position poleVector

        root2IK = (topJointTransform.position + IK_CTRL.position)/2;



        Vector3 IK2Root = IK_CTRL.position - topJointTransform.position;

        Quaternion quat;

        quat = Quaternion.AngleAxis(flipRotation*90,Vector3.forward);


        root2IK2MiddleJoint = quat * IK2Root;
       
        poleVector.position =root2IK -root2IK2MiddleJoint;


        // Get Angle 
        float angle = GetAngle();


        // Aim Joints

		Quaternion topJointAngleOffset = Quaternion.AngleAxis(angle*flipRotation, Vector3.forward);
						      
		if(!IsNaN( topJointAngleOffset ))
			topJointTransform.rotation = Quaternion.LookRotation(IK_CTRL.position - topJointTransform.position, AimDirection) * Quaternion.AngleAxis(90, UpDirection)* topJointAngleOffset;
		else
		{
			if(LargerMiddleJoint)
				topJointTransform.rotation = Quaternion.LookRotation(IK_CTRL.position - topJointTransform.position, AimDirection) * Quaternion.AngleAxis(-90, UpDirection);
			else
				topJointTransform.rotation = Quaternion.LookRotation(IK_CTRL.position - topJointTransform.position, AimDirection) * Quaternion.AngleAxis(90, UpDirection);
		}
        middleJointTransform.rotation = Quaternion.LookRotation(IK_CTRL.position - middleJointTransform.position, AimDirection) * Quaternion.AngleAxis(90, UpDirection);

        bottomJointTransform.rotation = IK_CTRL.rotation;
		if(Scale)
			bottomJointTransform.localScale = new Vector3(IK_CTRL.localScale.x*OffsetScale.x, IK_CTRL.localScale.y*OffsetScale.y, IK_CTRL.localScale.z*OffsetScale.z);


			
		
	}
	private bool IsNaN(Quaternion q) 
	{
		
		return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
		
	}
    private float GetAngle()
    {
        // Squash And Stretch
        if (SquashAndStretch)
        {
            topJointTransform.localScale = scaleStart[0];
            middleJointTransform.localScale = scaleStart[1];
        }

        float topLength = Vector3.Distance(topJointTransform.position, middleJointTransform.position);
        float middleLength = Vector3.Distance(middleJointTransform.position, bottomJointTransform.position);
        float length = topLength + middleLength;

        float ikLength = Vector3.Distance(topJointTransform.position, IK_CTRL.position);  

		if(middleLength>topLength)
			LargerMiddleJoint = true;
		else
			LargerMiddleJoint = false;

        // Squash And Stretch
        if (SquashAndStretch)
        {
            if (ikLength > length)
            {
                topJointTransform.localScale = new Vector3(scaleStart[0].x, (ikLength / length)*scaleStart[0].y,scaleStart[0].z);
                //bottomJointTransform.localScale = new Vector3(scaleStart[1].x, (length / ikLength)*scaleStart[1].y,scaleStart[1].z);
            }
        }

        ikLength = Mathf.Min(ikLength, length - 0.0001f); 

        float adjacent = (topLength*topLength - middleLength*middleLength + ikLength*ikLength) /(2*ikLength);

        float angle  = Mathf.Acos(adjacent/topLength) * Mathf.Rad2Deg;

        return angle;
    }
}
