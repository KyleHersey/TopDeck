using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDeck
{
    public class Card
    {
        public Card(JSONDTO c)
        {
            Name = c.name;
            CMC = c.cmc;
            Text = c.text;
            Flavor = c.flavor;
            Power = c.power;
            Toughness = c.toughness;
            MultiverseId = c.multiverseid;
        }

        public Card()
        {

        }

        public string Name { get; set; }
        public double CMC { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Supertypes { get; set; }
        public List<string> Types { get; set; }
        public List<string> Subtypes { get; set; }
        public string Text { get; set; }
        public string Flavor { get; set; }
        public string Power { get; set; }
        public string Toughness { get; set; }
        public long MultiverseId { get; set; }
        public List<string> Rulings { get; set; }
        public List<Tuple<string, string>> MultiverseIds { get; set; }
    }
}
