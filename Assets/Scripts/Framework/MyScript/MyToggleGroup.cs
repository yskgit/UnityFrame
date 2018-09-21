using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/MyToggle Group", 32)]
[DisallowMultipleComponent]
public class MyToggleGroup : UIBehaviour
{
    [SerializeField] private bool m_optional = true;

    [SerializeField] private bool m_AllowSwitchOff = false;
    public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

    private List<MyToggle> m_Toggles = new List<MyToggle>();

    protected MyToggleGroup()
    { }

    private void ValidateToggleIsInGroup(MyToggle toggle)
    {
        if (toggle == null || !m_Toggles.Contains(toggle))
            throw new ArgumentException(string.Format("MyToggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
    }

    public void NotifyToggleOn(MyToggle toggle)
    {
        ValidateToggleIsInGroup(toggle);

        // disable all toggles in the group

        if (!m_optional)
        {
            return;
        }

        for (var i = 0; i < m_Toggles.Count; i++)
        {
            if (m_Toggles[i] == toggle)
                continue;

            m_Toggles[i].isOn = false;
        }
    }

    public void UnregisterToggle(MyToggle toggle)
    {
        if (m_Toggles.Contains(toggle))
            m_Toggles.Remove(toggle);
    }

    public void RegisterToggle(MyToggle toggle)
    {
        if (!m_Toggles.Contains(toggle))
            m_Toggles.Add(toggle);
    }

    public bool AnyTogglesOn()
    {
        return m_Toggles.Find(x => x.isOn) != null;
    }

    public IEnumerable<MyToggle> ActiveToggles()
    {
        return m_Toggles.Where(x => x.isOn);
    }

    public void SetAllTogglesOff()
    {
        bool oldAllowSwitchOff = m_AllowSwitchOff;
        m_AllowSwitchOff = true;

        for (var i = 0; i < m_Toggles.Count; i++)
            m_Toggles[i].isOn = false;

        m_AllowSwitchOff = oldAllowSwitchOff;
    }
}
