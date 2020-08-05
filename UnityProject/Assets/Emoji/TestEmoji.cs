using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEmoji : MonoBehaviour {

	public TextEx testText;


	// Use this for initialization
	void Start () {
		SpriteModule.Instance.InitEmoji();

		//testText.text = 😀;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
