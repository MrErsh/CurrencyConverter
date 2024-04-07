using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CurrencyConverter.Model
{
    [Serializable]
    public class ValCurs
    {
        [XmlAttribute("Date")]
        public string Date { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Valute")]
        public List<Valute> Valutes { get; set; }
    }
}
