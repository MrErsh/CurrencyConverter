using System.Xml.Serialization;

namespace CurrencyConverter.Model
{
    public class Valute
    {
        [XmlAttribute("ID")]
        public string Id { get; set; }
        public string CharCode { get; set; }
        public string Name { get; set; }

        public string _vunitRate;
        public string VunitRate
        {
            get => _vunitRate;
            set
            {
                _vunitRate = value;
                Rate = decimal.Parse(value);
            }
        }

        [XmlIgnore]
        public decimal Rate { get; private set; }
    }
}
