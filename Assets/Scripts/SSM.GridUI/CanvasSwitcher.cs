using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SSM.GraphDrawing;
using System;
using SSM.Grid;

namespace SSM.GridUI
{
    public class CanvasSwitcher : MonoBehaviour
    {
        public Microgrid microgrid;
        public Dropdown dropdown;
        public Button addRemoveButton;
        public Image buttonImage;
        public Text buttonText;
        public Sprite buttonAddSprite;
        public Sprite buttonRemoveSprite;
        public Color addColor;
        public Color removeColor;
        public string addString;
        public string removeString;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        public bool buttonIsAdding;
        public MicrogridVar selectedVar;
        public GraphSubscriber gSubscriber;
        public List<MicrogridVar> allowedVars = new List<MicrogridVar>();
        public Dictionary<Dropdown.OptionData, MicrogridVar> optionDict;
        public Dictionary<MicrogridVar, string> descriptionDict;
        private bool loaded;

        public void OnButtonPress()
        {
            if (buttonIsAdding)
            {  Add(); }
            else
            {  Remove(); }
            Refresh();
        }

        public void OnValueChanged(Dropdown dropdown)
        {
            var currentOption = dropdown.options[dropdown.value];
            selectedVar = optionDict[currentOption];
            Refresh();
        }

        public void Refresh()
        {
            if (!loaded)
            {
                return;
            }

            RefreshButton();
            SetSprites();
            dropdown.RefreshShownValue();
        }

        private void Awake()
        {
            allowedVars = GetEnums();
            microgrid = microgrid ?? FindObjectOfType<Microgrid>();
        }

        private void OnEnable()
        {
            GetDescriptions();
            SetupDropdownOptions();
            SubscribeToDropdown();
            SubscribeToButton();
            loaded = true;
            OnValueChanged(dropdown);
        }

        private void Remove()
        {
            var foundLinks = gSubscriber.Links
                .Where(x => x.mvar == selectedVar).ToList();
            if (foundLinks.Count > 0)
            {
                for (int i = 0; i < foundLinks.Count; i++)
                {
                    gSubscriber.RemoveLink(foundLinks[i]);
                }
            }
            gSubscriber.LoadFromMicrogrid();
            Refresh();
        }

        private void Add()
        {
            if (gSubscriber.Links.Any(x => x.mvar == selectedVar)) { return; }
            var newLink = GraphMicrogridLink.Make(selectedVar, microgrid, -1);
            gSubscriber.AddLink(newLink);
            gSubscriber.LoadFromMicrogrid();
            Refresh();
        }

        private void RefreshButton()
        {
            var foundLinks = gSubscriber.Links.Where(x => x.mvar == selectedVar).ToList();
            if (foundLinks.Count > 0)
            {
                buttonImage.sprite = buttonRemoveSprite;
                buttonImage.color = removeColor;
                buttonText.text = removeString;
                buttonIsAdding = false;
            }
            else
            {
                buttonImage.sprite = buttonAddSprite;
                buttonImage.color = addColor;
                buttonText.text = addString;
                buttonIsAdding = true;
            }
        }

        private void SetupDropdownOptions()
        {
            var options                    = new List<Dropdown.OptionData>();
            IList<MicrogridVar> activeVars = GetActiveVars(gSubscriber.Links);

            foreach(MicrogridVar mvar in descriptionDict.Keys)
            {
                Sprite s = activeVars.Contains(mvar) ? activeSprite : inactiveSprite;
                string description = descriptionDict[mvar];
                var newOption = new Dropdown.OptionData(description, s);
                options.Add(newOption);
                optionDict.Add(newOption, mvar);
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        private void SetSprites()
        {
            IList<MicrogridVar> activeVars = GetActiveVars(gSubscriber.Links);
            foreach (Dropdown.OptionData option in dropdown.options)
            {
                MicrogridVar mvar = optionDict[option];
                Sprite s = activeVars.Contains(mvar) ? activeSprite : inactiveSprite;
                option.image = s;
            }
        }

        private List<MicrogridVar> GetEnums()
        {
            return MGMisc.accessorLists.Keys.ToList();
        }

        private void SubscribeToButton()
        {
            addRemoveButton.onClick.AddListener(delegate { OnButtonPress(); });
        }

        private void SubscribeToDropdown()
        {
            dropdown.onValueChanged.AddListener(delegate { OnValueChanged(dropdown); });
        }

        private void GetDescriptions()
        {
            descriptionDict = new Dictionary<MicrogridVar, string>();
            var mvars = (MicrogridVar[])Enum.GetValues(typeof(MicrogridVar));
            foreach (MicrogridVar mvar in mvars)
            {
                if (allowedVars.Contains(mvar))
                {
                    descriptionDict.Add(mvar, mvar.GetVarDescription());
                }
            }
        }

        private static IList<MicrogridVar> GetActiveVars(IList<GraphMicrogridLink> links)
        {
            var result = new List<MicrogridVar>();
            for (int i = 0; i < links.Count; i++)
            {
                result.Add(links[i].mvar);
            }
            return result;
        }
    }
}
