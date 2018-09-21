using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomWord : MonoBehaviour {
    public string[] Words;
	// Use this for initialization
	void Start () {
	    Text text = GetComponent<Text>();
	    int randomInt = Random.Range(0, Words.Length);
	    text.text = Words[randomInt];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
