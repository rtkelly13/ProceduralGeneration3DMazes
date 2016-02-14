using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GameAssets.Scripts.UI
{
    public class DropdownControl: MonoBehaviour
    {
        public Text headerText;
        public Dropdown dropdown;

        public void Initialise<TVal>(string header, List<DropdownOption<string, TVal>> values, int itemSelected, Action<string> onValueChanged)
        {
            name = header;
            headerText.text = header;
            dropdown.options.Clear();
            dropdown.AddOptions(values.Select(x => new Dropdown.OptionData(x.Key)).ToList());
            dropdown.value = itemSelected;
            dropdown.onValueChanged.AddListener(newValue =>
            {
                onValueChanged(values[newValue].Key);
            });
        }


    }

}
