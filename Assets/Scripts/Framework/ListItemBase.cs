using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListItemBase <T>: MonoBehaviour
{
    public T Data;

    public Action HandleSelect;
    public Action HandleDeselect;
    public Action HandleClick;

    protected MyButton[] _buttons;
    protected GameObject[] _objs;
    protected Text[] _texts;
    protected Sprite[] _sprites;
    protected Image[] _images;
    protected MyToggle[] _toggles;
    protected MyToggleGroup[] _toggleGroups;

    protected virtual void Awake()
    {
        var buttonCon = GetComponent<MyButtonContainer>();
        _buttons = buttonCon == null ? null : buttonCon.MyButtons;
        var objCon = GetComponent<GameObjectContainer>();
        _objs = objCon == null ? null : objCon.Objs;
        var textCon = GetComponent<TextContainer>();
        _texts = textCon == null ? null : textCon.Texts;
        var spriteCon = GetComponent<SpriteContainer>();
        _sprites = spriteCon == null ? null : spriteCon.Sprites;
        var imageCon = GetComponent<ImageContainer>();
        _images = imageCon == null ? null : imageCon.Images;
        var toggleCon = GetComponent<MyToggleContainer>();
        _toggles = toggleCon == null ? null : toggleCon.MyToggles;
        var toggleGroupCon = GetComponent<MyToggleGroupContainer>();
        _toggleGroups = toggleGroupCon == null ? null : toggleGroupCon.ToggleGroups;
    }

    public virtual void InitData(T data)
    {
        Data = data;
    }
}
