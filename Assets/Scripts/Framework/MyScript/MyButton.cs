// Button that's meant to work with mouse or touch-based devices.
//[AddComponentMenu("UI/Button", 30)]

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("UI/MyButton")]
public class MyButton : MySelectable
{
    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }

    // Event delegates triggered on click.
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();
    
    protected MyButton()
    {
    }

    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        base.OnPointerClick(eventData);
        if (onClick != null)
        {
            onClick.Invoke();
        }
    }
}