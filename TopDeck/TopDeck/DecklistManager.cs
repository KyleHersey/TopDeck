using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TopDeck
{
    public class DecklistManager
    {
        DatabaseManager DBMan;
        List<Tuple<int, string>> cardNames;

        public DecklistManager(DatabaseManager db)
        {
            DBMan = db;
        }

        public List<Tuple<int, string>> GetCardnamesFromFile(string fileName)
        {
            cardNames = new List<Tuple<int, string>>();
            StreamReader input = new StreamReader("SampleDeck.txt");

            while (input.EndOfStream == false)
            {
                String tempString = input.ReadLine();
                string[] words = tempString.Split(' ');
                int[] nums = new int[words.Length];

                //fill nums
                for (int i = 0; i < words.Length; i++)
                {
                    nums[i] = Convert.ToInt32(words[i]);
                }

                cardNames.Add(new Tuple<int, string>(nums[0], DBMan.GetAName(nums[1].ToString())));

            }

            return cardNames;
        }
    }
}
