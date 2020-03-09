using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beats
{
    public class BuildOptionsDropdown : MonoBehaviour
    {
        public BeatMachine m_beatMachine;
        public Dropdown m_dropdown;
        List<Dropdown.OptionData> m_optionDatas;
        Button b;

        // Start is called before the first frame update
        void Start()
        {

            m_dropdown.ClearOptions();


            //Grab the button if it's a sibling
            b = m_dropdown.gameObject.transform.parent.GetComponentInChildren<Button>();

            m_optionDatas = new List<Dropdown.OptionData>();

            foreach (string s in m_beatMachine.m_defaultFilenames)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = s;
                m_optionDatas.Add(optionData);
            }

            foreach (string s in m_beatMachine.m_savedFilenames)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = s;
                m_optionDatas.Add(optionData);
            }

            m_dropdown.AddOptions(m_optionDatas);
            m_dropdown.value = 1;
        }

        public void DropdownChangeCallback(int item)
        {
            m_beatMachine.m_load = m_optionDatas[item].text;
            m_beatMachine.LoadPatternSetFromDisc();
        }
    }


}
