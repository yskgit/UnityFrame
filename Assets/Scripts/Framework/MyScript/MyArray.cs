using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyArray<T> {
    public int Count;
    private T[] _elements;

    public MyArray() {
        _elements = new T[32];
    }

    public void Add(T element) {
        _elements[Count] = element;
        Count++;
        if (Count>= _elements.Length) {
            T[] tempElements = new T[Count * 2];
            _elements.CopyTo(tempElements, 0);
        }
    }

    public T this[int index] {
        get {
            if (index<Count) {
                return _elements[index];
            }
            Debug.Log("out of index");
            return default(T);
        }
        //set {
            
        //}
    }

    public void Clear() {
        Count = 0;
    }
}
