using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEmoji : MonoBehaviour 
{
	// 测试emoji：😀、😁
	void Start () {
		SpriteModule.Instance.InitEmoji();
	}
}
