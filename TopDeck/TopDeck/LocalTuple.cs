using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDeck
{
    public class LocalTuple
    {
        public LocalTuple(int count, string name, string multiverseId)
        {
            Count = count;
            Name = name;
            MultiverseId = multiverseId;
        }

        public int Count
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string MultiverseId
        {
            get;
            set;
        }
    }
}
