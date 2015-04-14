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
        List<Tuple<int, string, string>> cardNames;

        public DecklistManager(DatabaseManager db)
        {
            DBMan = db;
        }

        public List<Tuple<int, string, string>> GetCardnamesFromFile(string fileName)
        {
            cardNames = new List<Tuple<int, string, string>>();
            StreamReader input = new StreamReader(fileName);

            while (input.EndOfStream == false)
            {
                String tempString = input.ReadLine();
                string[] words = tempString.Split(' ');
               // int[] nums = new int[words.Length];           used in previous parsing method
                int multiverseID = -1;

                if (words.Length == 2)   //if there's a possibility we have a multiverse id...
                {
                    try
                    {
                        multiverseID = Convert.ToInt32(words[1]);
                    }
                    catch
                    {
                        try
                        {
                            multiverseID = Convert.ToInt32(DBMan.GetHighestMultiverseId(words[1]));
                        }
                        catch
                        {
                            multiverseID = -1;
                        }
                    }
                }

                if (multiverseID != -1) //if we got a multiverseID
                {
                    cardNames.Add(new Tuple<int, string, string>(Convert.ToInt32(words[0]), DBMan.GetAName(words[1].ToString()), Convert.ToString(words[1])));
                }
                else //if we didn't, assume #, cardname
                {
                    int num = Convert.ToInt32(words[0]);
                    string name = words[1];
                    for (int i = 2; i < words.Length; i++)
                    {
                        name += " ";
                        name += words[i];
                    }
                    cardNames.Add(new Tuple<int, string, string>(num, name, DBMan.GetHighestMultiverseId(name)));
                }


                /*
                 * previous parsing method
                 * 
                //fill nums
                for (int i = 0; i < words.Length; i++)
                {
                    nums[i] = Convert.ToInt32(words[i]);
                }

                cardNames.Add(new Tuple<int, string, string>(nums[0], DBMan.GetAName(nums[1].ToString()), nums[1].ToString()));
                */
            }

            return cardNames;
        }
    }
}
