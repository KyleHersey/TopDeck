using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace TopDeck
{
    public class DatabaseManager
    {
        SQLiteConnection dbConnection;
        int hashedJSONFile;
        string mainJSONFileName = "AllSets-x.json";

        public DatabaseManager()
        {
            // check if db is there. If not, download it
            LoadDB();

        }

        public void Update()
        {
            dbConnection.Close();
            if (File.Exists("SetsDB.sqlite"))
            {
                File.Delete("SetsDB.sqlite");
            }
            using (WebClient myClient = new WebClient())
            {
                Debug.WriteLine("Downloading new file");
                myClient.DownloadFile("http://Emerald-Baldwin.github.io/SetsDB.sqlite", "SetsDB.sqlite");
                Debug.WriteLine("Downloaded file");
            }

            // connect to the database, stored in a connection object
            // the string sets up the file we will be using
            dbConnection = new SQLiteConnection("Data Source=SetsDB.sqlite;Version=3;");

            // open the database, need to close later
            dbConnection.Open();
        }

        public void LoadDB()
        {
            if (!File.Exists("SetsDB.sqlite"))
            {
                using (WebClient myClient = new WebClient())
                {
                    Debug.WriteLine("Downloading new file");
                    myClient.DownloadFile("http://Emerald-Baldwin.github.io/SetsDB.sqlite", "SetsDB.sqlite");
                    Debug.WriteLine("Downloaded file");
                }
            }
            // connect to the database, stored in a connection object
            // the string sets up the file we will be using
            dbConnection = new SQLiteConnection("Data Source=SetsDB.sqlite;Version=3;");

            // open the database, need to close later
            dbConnection.Open();

            Debug.WriteLine("db file was opened");
        }

        // use filters to get a list of cards from the DB
        public List<string> GetCards(string name, string toughness, string hand,
            string cmc, string multiverseId, string loyalty, List<string> rarities,
            string flavor, string artist, string power, string cardText,
            List<string> types, bool reserved, bool requireMultiColor, bool excludeUnselected,
            string legality, List<string> setNames, List<string> colors, List<string> subtypes, List<string> supertypes)
        {
            if (multiverseId != null)
            {
                string cardWanted = GetAName(multiverseId);
                if (cardWanted != null)
                {
                    List<string> cardFound = new List<string>();
                    cardFound.Add(cardWanted);
                    return cardFound;
                }
            }

            Debug.WriteLine("in getcards");

            string sqlCard = @"select name 
                           from CARD
                           where name like @name
                           and hand like @hand
                           and (flavor like @flavor or flavor is null)";

            if (!cardText.Equals(""))
            {
                sqlCard += @" and card_text like @cardText";
            }

            if (reserved == true)
            {
                sqlCard += @" and reserved = 1";
            }

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sqlCard;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = "%" + name + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@hand") { Value = "%" + hand + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@flavor") { Value = "%" + flavor + "%" });

            if (!cardText.Equals(""))
            {
                cmd.Parameters.Add(new SQLiteParameter("@cardText") { Value = "%" + cardText + "%" });
            }

            SQLiteDataReader reader = cmd.ExecuteReader();

            HashSet<string> cardNamesWithoutColors = new HashSet<string>();
            while (reader.Read())
                cardNamesWithoutColors.Add((string)reader["name"]);

            reader.Close();

            // have a list of all colors so that we can remove cards with the wrong colors0
            List<string> allColors = new List<string>();
            allColors.Add("red");
            allColors.Add("blue");
            allColors.Add("black");
            allColors.Add("green");
            allColors.Add("white");

            // remove all of the colors we don't want
            List<string> unwantedColors = allColors.Except(colors).ToList<string>();

            // finds the cards with the wanted colors
            HashSet<string> cardNamesWithColors = null;
            for (int i = 0; i < colors.Count; i++)
            {
                string sqlColor = @"select name
                                from COLORS
                                where color like @color";
                var cmd2 = dbConnection.CreateCommand();
                cmd2.CommandText = sqlColor;
                string color = colors[i];
                cmd2.Parameters.Add(new SQLiteParameter("@color") { Value = "%" + color + "%" });
                SQLiteDataReader reader2 = cmd2.ExecuteReader();

                HashSet<string> tempCardsWithColors = new HashSet<string>();

                while (reader2.Read())
                    tempCardsWithColors.Add((string)reader2["name"]);
                reader2.Close();

                if (i == 0)
                {
                    cardNamesWithColors = tempCardsWithColors;
                }
                else if (cardNamesWithColors != null)
                {
                    // if we require multicolor, only have the cards that overlap in colors
                    // else, just have all of the cards
                    if (requireMultiColor)
                        cardNamesWithColors.IntersectWith(tempCardsWithColors);
                    else
                        cardNamesWithColors.UnionWith(tempCardsWithColors);
                }
            }

            // if we want to remove all cards that aren't our requested colors
            if (cardNamesWithColors != null && cardNamesWithColors.Count > 0 && excludeUnselected)
            {
                for (int i = 0; i < unwantedColors.Count; i++)
                {
                    string sqlColor = @"select name
                                from COLORS
                                where color like @color";
                    var cmd2 = dbConnection.CreateCommand();
                    cmd2.CommandText = sqlColor;
                    string color = unwantedColors[i];
                    cmd2.Parameters.Add(new SQLiteParameter("@color") { Value = "%" + color + "%" });
                    SQLiteDataReader reader2 = cmd2.ExecuteReader();

                    HashSet<string> tempCardsWithColors = new HashSet<string>();

                    while (reader2.Read())
                        tempCardsWithColors.Add((string)reader2["name"]);
                    List<string> cardNamesWithColorsList = cardNamesWithColors.Except(tempCardsWithColors).ToList<string>();
                    cardNamesWithColors = new HashSet<string>(cardNamesWithColorsList);
                }
                cardNamesWithoutColors.IntersectWith(cardNamesWithColors);
            }
            else if (cardNamesWithColors != null && cardNamesWithColors.Count > 0)
            {
                cardNamesWithoutColors.IntersectWith(cardNamesWithColors);
            }

            // TODO
            /*if (foreignName != "")
            {
                string sqlForeignName = @"select name
                                          from FOREIGN_NAMES
                                          where foreign_name like @foreign_name";
                var foreignNamecmd = dbConnection.CreateCommand();
                foreignNamecmd.CommandText = sqlForeignName;
                foreignNamecmd.Parameters.Add(new SQLiteParameter("@foreign_name") { Value = "%" + foreignName + "%" });

                SQLiteDataReader foreignNameReader = foreignNamecmd.ExecuteReader();

                while (foreignNameReader.Read())
                    cardNamesWithoutColors.Add((string)foreignNameReader["name"])

            } */


            // search by types, subtypes, supertypes
            if (types != null && types.Count > 0)
            {
                string sqlType = @"select name
                                   from TYPES
                                   where ";
                for (int i = 0; i < types.Count; i++)
                {
                    if (i != types.Count - 1)
                        sqlType += "type like \"%" + types[i] + "%\" or ";
                    else
                        sqlType += "type like \"%" + types[i] + "%\"";
                }
                var cmd3 = dbConnection.CreateCommand();
                cmd3.CommandText = sqlType;
                SQLiteDataReader reader3 = cmd3.ExecuteReader();

                HashSet<string> cardNamesWithTypes = new HashSet<string>();

                while (reader3.Read())
                    cardNamesWithTypes.Add((string)reader3["name"]);

                reader3.Close();

                cardNamesWithoutColors.IntersectWith(cardNamesWithTypes);
            }

            if (subtypes != null && subtypes.Count > 0)
            {
                string sqlSubtype = @"select name
                                   from SUBTYPES
                                   where ";
                for (int i = 0; i < subtypes.Count; i++)
                {
                    if (i != subtypes.Count - 1)
                        sqlSubtype += "subtype like \"%" + subtypes[i] + "%\" or ";
                    else
                        sqlSubtype += "subtype like \"%" + subtypes[i] + "%\"";
                }
                var cmd4 = dbConnection.CreateCommand();
                cmd4.CommandText = sqlSubtype;
                SQLiteDataReader subtypeReader = cmd4.ExecuteReader();

                HashSet<string> cardNamesWithSubtypes = new HashSet<string>();

                while (subtypeReader.Read())
                    cardNamesWithSubtypes.Add((string)subtypeReader["name"]);

                subtypeReader.Close();

                cardNamesWithoutColors.IntersectWith(cardNamesWithSubtypes);
            }

            if (supertypes != null && supertypes.Count > 0)
            {
                string sqlSupertype = @"select name
                                   from SUPERTYPES
                                   where ";
                for (int i = 0; i < supertypes.Count; i++)
                {
                    if (i != supertypes.Count - 1)
                        sqlSupertype += "supertype like \"%" + supertypes[i] + "%\" or ";
                    else
                        sqlSupertype += "supertype like \"%" + supertypes[i] + "%\"";
                }
                var cmd5 = dbConnection.CreateCommand();
                cmd5.CommandText = sqlSupertype;
                SQLiteDataReader supertypeReader = cmd5.ExecuteReader();

                HashSet<string> cardNamesWithSupertypes = new HashSet<string>();

                while (supertypeReader.Read())
                    cardNamesWithSupertypes.Add((string)supertypeReader["name"]);

                supertypeReader.Close();

                cardNamesWithoutColors.IntersectWith(cardNamesWithSupertypes);
            }

            if (rarities != null && rarities.Count > 0)
            {
                string sqlRarity = @"select name
                                   from RARITIES
                                   where ";
                for (int i = 0; i < rarities.Count; i++)
                {
                    if (i != rarities.Count - 1)
                        sqlRarity += "rarity like \"%" + rarities[i] + "%\" or ";
                    else
                        sqlRarity += "rarity like \"%" + rarities[i] + "%\"";
                }
                var cmd5 = dbConnection.CreateCommand();
                cmd5.CommandText = sqlRarity;
                SQLiteDataReader rarityReader = cmd5.ExecuteReader();

                HashSet<string> cardNamesWithRarities = new HashSet<string>();

                while (rarityReader.Read())
                    cardNamesWithRarities.Add((string)rarityReader["name"]);

                rarityReader.Close();

                cardNamesWithoutColors.IntersectWith(cardNamesWithRarities);
            }

            string sqlArtist = @"select name
                                 from ARTISTS
                                 where artist like @artist";
            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sqlArtist;
            cmd.Parameters.Add(new SQLiteParameter("@artist") { Value = "%" + artist + "%" });

            reader = cmd.ExecuteReader();

            HashSet<string> cardNamesWithArtists = new HashSet<string>();
            while (reader.Read())
                cardNamesWithArtists.Add((string)reader["name"]);

            reader.Close();

            cardNamesWithoutColors.IntersectWith(cardNamesWithArtists);

            if (!power.Equals(""))
            {
                List<string> withOps = ParseOperators("power", power);
                if (withOps != null)
                {
                    cardNamesWithoutColors.IntersectWith(withOps);
                }
            }

            if (!toughness.Equals(""))
            {
                List<string> withOps = ParseOperators("toughness", toughness);
                if (withOps != null)
                {
                    cardNamesWithoutColors.IntersectWith(withOps);
                }
            }

            if (!loyalty.Equals(""))
            {
                List<string> withOps = ParseOperators("loyalty", loyalty);
                if (withOps != null)
                {
                    cardNamesWithoutColors.IntersectWith(withOps);
                }
            }

            if (!cmc.Equals(""))
            {
                List<string> withOps = ParseOperators("cmc", cmc);
                if (withOps != null)
                {
                    cardNamesWithoutColors.IntersectWith(withOps);
                }
            }

            if (setNames.Count > 0)
            {
                HashSet<string> allCardNamesFromSets = new HashSet<string>();
                for (int i = 0; i < setNames.Count; i++)
                {
                    string sqlSetName = @"select name
                                          from MULTIVERSEID_SET
                                          where setName like @set";
                    cmd = dbConnection.CreateCommand();
                    cmd.CommandText = sqlSetName;
                    cmd.Parameters.Add(new SQLiteParameter("@set") { Value = "%" + setNames[i] + "%" });

                    reader = cmd.ExecuteReader();

                    HashSet<string> cardNamesFromSet = new HashSet<string>();
                    while (reader.Read())
                    {
                        cardNamesFromSet.Add((string)reader["name"]);
                    }

                    reader.Close();

                    allCardNamesFromSets.UnionWith(cardNamesFromSet);
                }

                cardNamesWithoutColors.IntersectWith(allCardNamesFromSets);
            }

            // if card is not in legalities table, it is banned
            // if card is restricted, it is legal

            // first check for restricted or legal
            if (!legality.Equals(""))
            {
                string sqlLegality = @"select name, legality_for_format
                                 from LEGALITIES
                                 where name like @name
                                 and format like @legality";
                cmd = dbConnection.CreateCommand();
                cmd.CommandText = sqlLegality;
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = "%" + name + "%" });
                cmd.Parameters.Add(new SQLiteParameter("@legality") { Value = "%" + legality + "%" });

                reader = cmd.ExecuteReader();

                HashSet<string> cardNamesWithLegality = new HashSet<string>();
                while (reader.Read())
                {
                    if (((string)reader["legality_for_format"]).Equals("Legal") || ((string)reader["legality_for_format"]).Equals("Restricted"))
                        cardNamesWithLegality.Add((string)reader["name"]);
                }

                reader.Close();

                cardNamesWithoutColors.IntersectWith(cardNamesWithLegality);
            }

            return cardNamesWithoutColors.ToList();
        }

        public List<string> ParseOperators(string fieldType, string query)
        {
            List<string> cardsFound = new List<string>();
            string searchBy = "";
            string ops = "";
            query = query.Trim();
            if (query[0] == '<' || query[0] == '>')
            {
                if (query[1] == '=' && Regex.IsMatch(query.Substring(2), @"^\d+$"))
                {
                    searchBy = query.Substring(2);
                    ops = query.Substring(0, 2);
                }
                else if (Regex.IsMatch(query.Substring(1), @"^\d+$"))
                {
                    searchBy = query.Substring(1);
                    ops = query.Substring(0, 1);
                }
                else
                {
                    return null;
                }
            }
            else if (Regex.IsMatch(query, @"^\d+$"))
            {
                cardsFound.AddRange(GetMatches(fieldType, Int32.Parse(query)));
                return cardsFound;
            }
            else
            {
                return null;
            }


            if (ops[0] == '<')
            {
                for (int i = 0; i < Int32.Parse(searchBy); i++)
                {
                    cardsFound.AddRange(GetMatches(fieldType, i));
                }
            }
            else if (ops[0] == '>')
            {
                for (int i = Int32.Parse(searchBy) + 1; i < 16; i++)
                {
                    cardsFound.AddRange(GetMatches(fieldType, i));
                }
            }

            if (ops.Length > 1 && ops[1] == '=')
            {
                cardsFound.AddRange(GetMatches(fieldType, Int32.Parse(searchBy)));
            }

            return cardsFound;
        }

        public List<String> GetMatches(string fieldType, int fieldValue)
        {
            List<string> cardsFound = new List<string>();
            string sql = @"select name
                                    from CARD
                                    where " + fieldType + " = @fieldtype";

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@fieldtype") { Value = fieldValue });

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
                cardsFound.Add((string)reader["name"]);

            return cardsFound;
        }

        // gets a card object by name
        public Card GetCard(string name)
        {
            Card c = null;
            string sql = @"select *
                           from CARD
                           where name = @name";

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            SQLiteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                c = new Card();

                c.CMC = (double)reader["cmc"];

                if (!(reader["flavor"] is DBNull))
                    c.Flavor = (string)reader["flavor"];

                c.Name = (string)reader["name"];

                if (!(reader["power"] is DBNull))
                    c.Power = (string)reader["power"];

                if (!(reader["card_text"] is DBNull))
                    c.Text = (string)reader["card_text"];

                if (!(reader["toughness"] is DBNull))
                    c.Toughness = (string)reader["toughness"];
            }

            // add colors from table
            sql = @"select color
                    from COLORS
                    where name like @name";

            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            c.Colors = new List<string>();

            while (reader.Read())
            {
                c.Colors.Add((string)reader["color"]);
            }

            // foriegn names - TODO

            // add subtypes from subtypes table
            sql = @"select subtype
                    from SUBTYPES
                    where name like @name";

            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            c.Subtypes = new List<string>();

            while (reader.Read())
            {
                c.Subtypes.Add((string)reader["subtype"]);
            }

            // add supertypes from supertypes table
            sql = @"select supertype
                    from SUPERTYPES
                    where name like @name";

            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            c.Supertypes = new List<string>();

            while (reader.Read())
            {
                c.Supertypes.Add((string)reader["supertype"]);
            }

            // add types
            sql = @"select type
                    from TYPES
                    where name = @name";

            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            c.Types = new List<string>();

            while (reader.Read())
            {
                c.Types.Add((string)reader["type"]);
            }

            // store all multiverse Ids
            List<Tuple<string, string>> multiverseIds = new List<Tuple<string, string>>();
            sql = @"select setName, multiverse_id
                    from MULTIVERSEID_SET
                    where name = @name";
            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                multiverseIds.Add(Tuple.Create((string)reader["setName"], (string)reader["multiverse_id"]));
            }
            c.MultiverseIds = multiverseIds;

            sql = @"select date, ruling_text
                    from RULINGS
                    where name = @name";
            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            List<string> rulings = new List<string>();
            while (reader.Read())
            {
                rulings.Add((string)reader["date"] + ";\t" + (string)reader["ruling_text"]);
            }
            c.Rulings = rulings;

            // legalities - TODO
            return c;
        }

        // get a card object from multiverse id
        public Card GetACardFromMultiverseId(string multiverseId)
        {
            string cardName = GetAName(multiverseId);
            if (cardName != null)
            {
                return GetCard(cardName);
            }
            return null;
        }

        //gets a name from a multiverse ID
        public string GetAName(string multiverseID)
        {
            string sql = @"select name
                            from MULTIVERSEID_SET
                            where multiverse_id = @mv";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@mv") { Value = multiverseID });
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return (string)reader["name"];
            }
            return null;
        }

        // get the earliest multiverse id
        public string GetHighestMultiverseId(string name)
        {
            string sql = @"select multiverse_id
                           from MULTIVERSEID_SET
                           where name like @name";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            var reader = cmd.ExecuteReader();
            string largestMultiverseId = "";
            if (reader.Read())
            {
                string currentId = (string)reader["multiverse_id"];
                if (largestMultiverseId.CompareTo(currentId) < 0)
                {
                    largestMultiverseId = currentId;
                }
            }
            return largestMultiverseId;
        }
    }
}