using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beats
{
    public class ButtonName : MonoBehaviour
    {

        void Awake()
        {
            Text t = this.GetComponentInChildren<Text>();

            if (t)
                t.text = this.name;

            Destroy(this);
        }

    }
}