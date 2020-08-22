using System;
using UnityEngine;

public class TestRotation : MonoBehaviour {
	public Transform target;
	public Transform source;

	Vector2 sourcePos;
	Vector2 targetPos;
	Vector2 sourceForward;
	Vector2 targetForward;

	void Start () {
		sourcePos = source.localPosition;
		targetPos = target.localPosition;

		sourceForward = Vector2.up;
		targetForward = (targetPos - sourcePos).normalized;
	}

	void Update () {
		//[Unity中点乘和叉乘的原理及使用方法](https://gameinstitute.qq.com/community/detail/112157)
		// 简单的说: 点乘判断角度，叉乘判断方向。
		// 形象的说: 当一个敌人在你身后的时候，
		// 叉乘可以判断你是往左转还是往右转更好的转向敌人，
		// 点乘得到你当前的面朝向的方向和你到敌人的方向的所成的角度大小。

		// The red axis of the transform in world space.
		sourceForward = source.right;// new Vector2((float)Math.Cos(source.localRotation.z * Mathf.Deg2Rad),(float)Math.Sin(source.localRotation.z * Mathf.Deg2Rad));

		float turnSpeed = 1f;
		float angle = Vector2.Angle(targetForward, sourceForward);
		if(angle <= 1)
			return;
			
		float deltaTime = Time.fixedDeltaTime;
		float radianToTurn = turnSpeed * deltaTime;

		//叉积算出方向 unity是左手坐标系，所以反过来了，不知道对不对
		Vector3 cross = Vector3.Cross(targetForward, sourceForward);

		// target 在 source 的顺时针方向  
		if(cross.z > 0)
			radianToTurn = -radianToTurn;

		Vector2 newForward = Vector2RotateFromRadian(sourceForward.x, sourceForward.y, radianToTurn);
		float newAngle = Vector2.Angle(sourceForward, newForward);
		Debug.Log("sourceForward:" + sourceForward.ToString() + " targetForward:" + targetForward.ToString() + 
			" angle:" + angle.ToString() + " radianToTurn:" + radianToTurn.ToString() + " cross:" + cross.ToString() 
			+ " newForward:" + newForward.ToString() + " newAngle:" + newAngle.ToString());

		source.right = newForward;
	}

	//2维向量旋转(逆时针旋转公式) 顺时针角度为负，逆时针角度为正
	private static Vector2 Vector2RotateFromRadian(float forwardX, float forwardZ, float radian)
	{
		float sinValue = (float)Math.Sin(radian);
		float cosValue = (float)Math.Cos(radian);
		float x = forwardX * cosValue - forwardZ * sinValue;
		float z = forwardX * sinValue + forwardZ * cosValue;
		return new Vector2(x, z);
	}
}

//Vector3 cross = Vector3.Cross(a, b);
//if(cross.y > 0)
//{
//	// b 在 a 的顺时针方向  
//}
//else if(cross.y == 0)
//{
//	// b 和 a 方向相同（平行）  
//}
//else
//{
//	// b 在 a 的逆时针方向  
//}