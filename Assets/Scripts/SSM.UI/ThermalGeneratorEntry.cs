using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SSM.UI
{
    public class ThermalGeneratorEntry : MonoBehaviour
    {
        public ToolTip tooltip;
        public event EventHandler<Tuple<ThermalGenerator, int>> OnQuantityChange;
        public event EventHandler<ThermalGeneratorEntry> OnDeleteButton;
        public int Quantity => quantity;
        public string generatorName;
        public ThermalGenerator generator;

        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_InputField quantityInputField;
        [SerializeField] private Button increaseQtyButton;
        [SerializeField] private Button decreaseQtyButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button infoButton;

        private int quantity = 0;

        public void SetEntry(string name, ThermalGenerator generator, int quantity)
        {
            this.generatorName = name;
            this.generator = generator;
            this.quantity = quantity;

            nameLabel.text = generatorName;
            quantityInputField.SetTextWithoutNotify(quantity.ToString());

            tooltip.header.text = generatorName + " Specifications";
            tooltip.body.text = $"" +
                $"Rated Power: {generator.ratedPower.ToString("F2")} MW\n" +
                $"Min. Downtime: {generator.minDTime.ToString("F2")} minutes\n" +
                $"Min. Uptime: {generator.minUTime.ToString("F2")} minutes\n" +
                $"Cost Coefficient A: {generator.a} €/MW^2\n" +
                $"Cost Coefficient B: {generator.b} €/MW\n" +
                $"Cost Coefficient C: {generator.c} €\n" +
                $"Cost at Max Power: {((generator.a * generator.ratedPower * generator.ratedPower + generator.b * generator.ratedPower + generator.c) / generator.ratedPower).ToString("F2")} €/MWh\n";
        }

        public void SetQuantity(int qty)
        {
            if (qty > ushort.MaxValue)
            {
                qty = ushort.MaxValue;
            }
            else if (qty < 0)
            {
                qty = 0;
            }

            quantity = qty;
            quantityInputField.SetTextWithoutNotify(qty.ToString());
            OnQuantityChange?.Invoke(this, Tuple.Create(generator, Quantity));
        }

        protected void Awake()
        {
            increaseQtyButton.onClick.AddListener(IncreaseQuantity);
            decreaseQtyButton.onClick.AddListener(DecreaseQuantity);
            deleteButton.onClick.AddListener(Delete);
            quantityInputField.richText = false;
            quantityInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            quantityInputField.onEndEdit.AddListener(OnQuantityFieldEdit);
            quantityInputField.SetTextWithoutNotify(quantity.ToString());
            nameLabel.text = generatorName;

            infoButton.interactable = false;
        }

        private void Delete() => OnDeleteButton?.Invoke(this, this);
        private void IncreaseQuantity() => SetQuantity(quantity + 1);
        private void DecreaseQuantity() => SetQuantity(quantity - 1);
        private void OnQuantityFieldEdit(string s) => SetQuantity(int.Parse(s));
    }
}