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
