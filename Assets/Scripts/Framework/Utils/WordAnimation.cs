using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordAnimation : MonoBehaviour {
    public int Speed = 2;
    public string[] WordsArray;

    private Text _text;
    private int _index;
    private float _timer;

    private void Awake() {
        _text = GetComponent<Text>();
        //WordsArray = new[] { ".", "..", "...", "....", "....." };
    }
	
	// Update is called once per frame
	void Update () {
	    _timer += Time.deltaTime;
	    if (_timer >= 1f/Speed) {
	        _timer = 0;
            if (_index>= WordsArray.Length) {
	            _index = 0;
	        }
            _text.text = WordsArray[_index];
            _index++;
        }
    }
}
