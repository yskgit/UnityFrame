using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MySelectable))]
public class MySelectableEditor : Editor
{

//    private SerializedObject _obj;
////    private MySelectable _mySelectable;
//    private SerializedProperty _internal;
//    private SerializedProperty _internalTime;
//    private SerializedProperty _scale;

//    void OnEnable()
//    {
//        Debug.Log("OnEnable_1");
//        _obj = new SerializedObject(target);
//        _internal = _obj.FindProperty("Internal");
//        _internalTime = _obj.FindProperty("InternalTime");
//        _scale = _obj.FindProperty("Scale");
//        Debug.Log("OnEnable_2");

//    }

//    public override void OnInspectorGUI()
//    {
//        Debug.Log("OnInspectorGUI_1");
//        _obj.Update();
//        EditorGUILayout.PropertyField(_internal);

////        if (_internal.boolValue)
//        {
//            EditorGUILayout.PropertyField(_internalTime);
//            EditorGUILayout.PropertyField(_scale);
//        }
//        //_mySelectable = (MySelectable)target;
//        //_mySelectable.Internal = EditorGUILayout.Toggle("Internal-", _mySelectable.Internal);

//        //if (_mySelectable.Internal)
//        //{
//        //    EditorGUILayout.PropertyField(_internalTime);
//        //}
//        Debug.Log("OnInspectorGUI_2");
//        _obj.ApplyModifiedProperties();//应用
//        Debug.Log("OnInspectorGUI_3");
//    }
}
