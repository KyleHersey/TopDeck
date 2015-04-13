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
            Layout = c.layout;
            Name = c.name;
            //Names = c.names;
            CMC = c.cmc;
            //Colors = c.colors;
            Type = c.type;
            //Supertypes = c.supertypes;
            //Types = c.types;
            //Subtypes = c.subtypes;
            Rarity = c.rarity;
            Text = c.text;
            Flavor = c.flavor;
            Artist = c.artist;
            Number = c.number;
            Power = c.power;
            Toughness = c.toughness;
            Loyalty = c.loyalty;
            MultiverseId = c.multiverseid;
            Variations = c.variations;
            Watermark = c.watermark;
            Border = c.border;
            Timeshifted = c.timeshifted;
            Hand = c.hand;
            Life = c.life;
            ReleaseDate = c.releaseDate;
        }

        public Card()
        {

        }

        public string Layout { get; set; }
        public string Name { get; set; }
        public List<string> Names { get; set; }
        public double CMC { get; set; }
        public List<string> Colors { get; set; }
        public string Type { get; set; }
        public List<string> Supertypes { get; set; }
        public List<string> Types { get; set; }
        public List<string> Subtypes { get; set; }
        public string Rarity { get; set; }
        public string Text { get; set; }
        public string Flavor { get; set; }
        public string Artist { get; set; }
        public string Number { get; set; }
        public string Power { get; set; }
        public string Toughness { get; set; }
        public long Loyalty { get; set; }
        public long MultiverseId { get; set; }
        public List<int> Variations { get; set; }
        public string Watermark { get; set; }
        public string Border { get; set; }
        public bool Timeshifted { get; set; }
        public long Hand { get; set; }
        public long Life { get; set; }
        public string ReleaseDate { get; set; }
        public List<string> Rulings { get; set; }
        public List<Tuple<string, string>> MultiverseIds { get; set; }
    }
}
