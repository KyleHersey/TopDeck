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

        public DatabaseManager() {

            // first, check if our mtgjson file exists
            CheckForJSON();

            // check if DB exists
            //createDBFile();

            // check if db is there. If not, download it
            DownloadDB();
            
        }

        // check that we have the json file
        public void CheckForJSON()
        {
            if (!File.Exists(mainJSONFileName))
            {
                Debug.WriteLine("Downloading file");
                using (WebClient myClient = new WebClient())
                {
                    myClient.DownloadFile("http://mtgjson.com/json/AllSets-x.json", mainJSONFileName);
                }
                Debug.WriteLine("Finished!");
            }

            // has the json file to compare with later downloads
            hashedJSONFile = HashJSON(mainJSONFileName);
        }

        // hashes a json file
        public int HashJSON(string fileName)
        {
            string file = File.ReadAllText(fileName);
            return file.GetHashCode();
        }

        // download a new file - if it is different from our old one,
        // updated the DB
        public bool DownloadNewFile() {
            using (WebClient myClient = new WebClient()) {
                Debug.WriteLine("Downloading new file");
                myClient.DownloadFile("http://mtgjson.com/json/AllSets-x.json", "AllSets-x2.json");
                Debug.WriteLine("Downloaded file");
            }
            if (hashedJSONFile != HashJSON("AllSets-x2.json")) {
                dbConnection.Close();

                if (File.Exists(mainJSONFileName))
                {
                    File.Delete(mainJSONFileName);
                }
                Debug.WriteLine("Copying the new file to the old file location");
                /*File.Copy("AllCards-x2.json", mainJSONFileName);
                Debug.WriteLine("Copied the new file");

                Debug.WriteLine("Deleting database file");
                File.Delete("TempDB.sqlite");*/
                return true;
            }
            Debug.WriteLine("Deleting copy of json file");
            //File.Delete("AllCards-x2.json");
            return false;
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

        public void DownloadDB()
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

        // create the database, if needed
        public void createDBFile()
        {
            if (File.Exists("SetsDB.sqlite")) {
                // connect to the database, stored in a connection object
                // the string sets up the file we will be using
                dbConnection = new SQLiteConnection("Data Source=SetsDB.sqlite;Version=3;");

                // open the database, need to close later
                dbConnection.Open();

                Debug.WriteLine("db file was found");
            }
            else
            {
                Debug.WriteLine("db file not found");

                // creates a new db file
                SQLiteConnection.CreateFile("SetsDB.sqlite");

                // connect to the database, stored in a connection object
                // the string sets up the file we will be using
                dbConnection = new SQLiteConnection("Data Source=SetsDB.sqlite;Version=3;");

                dbConnection.Open();

                try
                {
                    String mtgJSONText = File.ReadAllText("AllSets-x.json", Encoding.UTF8);

                    // build our ARTISTS table
                    string sql = @"create table ARTISTS
                                   (name TEXT NOT NULL,
                                    multiverse_id TEXT NOT NULL,
                                    artist TEXT,
                                    PRIMARY KEY(name, multiverse_id, artist))";
                    CreateTable(sql);

                    // build our RARITIES table
                    sql = @"create table RARITIES
                            (name TEXT NOT NULL,
                             multiverse_id TEXT NOT NULL,
                             rarity TEXT,
                             PRIMARY KEY(name, multiverse_id, rarity))";
                    CreateTable(sql);

                    // build our RULINGS table
                    sql = @"create table RULINGS
                                   (name TEXT NOT NULL,
                                    date TEXT NOT NULL,
                                    ruling_text TEXT NOT NULL,
                                    PRIMARY KEY(name, date, ruling_text))";
                    CreateTable(sql);

                    // build FOREIGN_NAMES table
                   sql = @"create table FOREIGN_NAMES
                            (name TEXT NOT NULL,
                             foreign_name TEXT,
                             language TEXT NOT NULL,
                             PRIMARY KEY(name, language))";
                    CreateTable(sql);

                    // build LEGALITIES table
                    sql = @"create table LEGALITIES
                            (name TEXT NOT NULL,
                             format TEXT NOT NULL,
                             legality_for_format TEXT,
                             PRIMARY KEY(name, format))";
                    CreateTable(sql);

                    // build COLORS table
                    sql = @"create table COLORS 
                            (name TEXT NOT NULL, 
                             color TEXT, 
                             PRIMARY KEY (name, color))";
                    CreateTable(sql);

                    // build SUBTYPES table
                    sql = @"create table SUBTYPES 
                            (name TEXT NOT NULL, 
                             subtype TEXT, 
                             PRIMARY KEY (name, subtype))";
                    CreateTable(sql);

                    // build TYPES table
                    sql = @"create table TYPES 
                            (name TEXT NOT NULL, 
                             type TEXT, 
                             PRIMARY KEY (name, type))";
                    CreateTable(sql);

                    // build SUPERTYPES table
                    sql = @"create table SUPERTYPES 
                            (name TEXT NOT NULL, 
                             supertype TEXT, 
                             PRIMARY KEY (name, supertype))";
                    CreateTable(sql);

                    // build MULTIVERSEID_SET table
                    sql = @"create table MULTIVERSEID_SET
                            (name TEXT NOT NULL,
                             setName TEXT NOT NULL,
                             multiverse_id TEXT,
                             PRIMARY KEY (name, setName, multiverse_id))";
                    CreateTable(sql);

                    Debug.WriteLine("Creating card table");

                    // build CARD table
                    sql = @"create table CARD 
                               (name TEXT NOT NULL, 
                                toughness TEXT,
                                manacost TEXT, 
                                hand INTEGER, 
                                cmc REAL,
                                reserved NUMERIC,
                                loyalty INTEGER,
                                flavor TEXT, 
                                power TEXT,
                                card_text TEXT, 
                                type TEXT, 
                                PRIMARY KEY (name))";
                    CreateTable(sql);

                    JToken outer = JToken.Parse(mtgJSONText);

                    List<String> elements = GetSetNames(mtgJSONText);

                    Debug.WriteLine("Number of sets " + elements.Count);

                    // this is a list of all of the cards we have seen previously
                    List<String> cardsMet = new List<String>();

                    var ser = new DataContractJsonSerializer(typeof(JSONDTO));
                    for (int i = 0; i < elements.Count; i++)
                    {
                        JObject inner = outer[elements[i]].Value<JObject>();

                        foreach (var cardInCards in inner["cards"].Children())
                        {
                            JObject cardJObject = JObject.Parse(cardInCards.ToString());
                            if (!cardsMet.Contains(cardJObject["name"].ToString()))
                            {
                                AssembleDB(cardInCards, ser, cardJObject);
                                cardsMet.Add(cardJObject["name"].ToString());
                            }
                            if (cardJObject["multiverseid"] != null)
                            {
                                AddMultiverseId(cardJObject["name"].ToString(), inner["name"].ToString(), cardJObject["multiverseid"].ToString());
                                AddArtist(cardJObject["name"].ToString(), cardJObject["multiverseid"].ToString(), cardJObject["artist"].ToString());
                                AddRarity(cardJObject["name"].ToString(), cardJObject["multiverseid"].ToString(), cardJObject["rarity"].ToString());
                            }
                        }
                    }

                    // save the last set name we saw, for updating
                    using (StreamWriter file = new StreamWriter("lastSet.txt"))
                    {
                        file.WriteLine(elements[elements.Count - 1]);
                    }
                }
                catch
                {
                    Debug.WriteLine("Unable to open AllCards-x.json file.");
                }
            }
        }

        // updates the database
        public void UpdateDB()
        {
            if (File.Exists("SetsDB.sqlite"))
            {
                // connect to the database, stored in a connection object
                // the string sets up the file we will be using
                dbConnection = new SQLiteConnection("Data Source=SetsDB.sqlite;Version=3;");

                // open the database, need to close later
                dbConnection.Open();

                Debug.WriteLine("db file was found");

                String mtgJSONText = File.ReadAllText("AllSets-x.json", Encoding.UTF8);

                JToken outer = JToken.Parse(mtgJSONText);

                List<String> elements = GetSetNames(mtgJSONText);

                using (StreamReader file = new StreamReader("lastSet.txt"))
                {
                    string lastSet = file.ReadLine();

                    // if we havn't seen the last set before, add that set to database
                    if (!lastSet.Equals(elements[elements.Count - 1])) {
                        JObject inner = outer[elements[elements.Count - 1]].Value<JObject>();
                        
                        var ser = new DataContractJsonSerializer(typeof(JSONDTO));

                        foreach (var cardInCards in inner["cards"].Children())
                        {
                            JObject cardJObject = JObject.Parse(cardInCards.ToString());

                            string sql = @"select name
                                           from CARD
                                           where name = @name";

                            
                            var cmd = dbConnection.CreateCommand();
                            cmd.CommandText = sql;
                            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = cardJObject["name"].ToString() });
                            var reader = cmd.ExecuteReader();
                            if (!reader.Read()) {
                                AssembleDB(cardInCards, ser, cardJObject);
                            }
                            if (cardJObject["multiverseid"] != null)
                            {
                                AddMultiverseId(cardJObject["name"].ToString(), inner["name"].ToString(), cardJObject["multiverseid"].ToString());
                                AddArtist(cardJObject["name"].ToString(), cardJObject["multiverseid"].ToString(), cardJObject["artist"].ToString());
                                AddRarity(cardJObject["name"].ToString(), cardJObject["multiverseid"].ToString(), cardJObject["rarity"].ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                createDBFile();
            }
        }

        // add each card to the database and its other information
        private void AssembleDB(JToken cardInCards, DataContractJsonSerializer ser, JObject cardJObject)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(cardInCards.ToString());
            MemoryStream stream1 = new MemoryStream(byteArray);

            var info = (JSONDTO)ser.ReadObject(stream1);

            if (cardJObject["colors"] != null)
                foreach (var c in cardJObject["colors"].Children())
                    AddColors(info.name, c.ToString());

            if (cardJObject["subtypes"] != null)
                foreach (var c in cardJObject["subtypes"].Children())
                    AddSubtypes(info.name, c.ToString());

            if (cardJObject["types"] != null)
                foreach (var c in cardJObject["types"].Children())
                    AddTypes(info.name, c.ToString());

            if (cardJObject["supertypes"] != null)
                foreach (var c in cardJObject["supertypes"].Children())
                    AddSupertypes(info.name, c.ToString());

            // add the rulings
            if (cardJObject["rulings"] != null)
            {
                foreach (var c in cardJObject["rulings"].Children())
                {
                    string date = (string)c["date"];
                    string text = (string)c["text"];
                    AddRuling(info.name, date, text);
                }
            }

            if (cardJObject["legalities"] != null)
            {
                var c = cardJObject["legalities"].Value<JObject>();
                string modern = "", legacy = "", vintage = "", freeform = "", prismatic = "", tribal = "", singleton = "", commander = "";
                if (c["Modern"] != null)
                    modern = (string)c["Modern"];
                if (c["Legacy"] != null)
                    legacy = (string)c["Legacy"];
                if (c["Vintage"] != null)
                    vintage = (string)c["Vintage"];
                if (c["Freeform"] != null)
                    freeform = (string)c["Freeform"];
                if (c["Prismatic"] != null)
                    prismatic = (string)c["Prismatic"];
                if (c["Tribal Wars Legacy"] != null)
                    tribal = (string)c["Tribal Wars Legacy"];
                if (c["Singleton 100"] != null)
                    singleton = (string)c["Singleton 100"];
                if (c["Commander"] != null)
                    commander = (string)c["Commander"];
                AddLegality(info.name, modern, legacy, vintage, freeform, prismatic, tribal, singleton, commander);
            }

            if (cardJObject["foreignNames"] != null)
            {
                foreach (var c in cardJObject["foreignNames"].Children())
                {
                    string language = (string)c["language"];
                    string text = (string)c["name"];
                    AddLanguage(info.name, language, text);
                }
            }

            // add this card to the CARD table
            AddToCard(info);
        }

        // gets all of the sets in this file
        private List<string> GetSetNames(string mtgJSONText)
        {
            var o = JObject.Parse(mtgJSONText);

            List<String> elements = new List<String>();
            foreach (JToken child in o.Children())
            {
                var property = child as JProperty;

                if (property != null)
                {
                    elements.Add(property.Name);
                }
                else
                {
                    Debug.WriteLine("null");
                }
            }
            return elements;
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

            if (!cardText.Equals("")) {
                sqlCard += @" and card_text like @cardText";
            }

            if (reserved == true)
            {
                sqlCard += @" and reserved = 1";
            }

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sqlCard;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = "%" + name + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@hand") { Value = "%" + hand + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@flavor") { Value = "%" + flavor + "%"});
            
            if (!cardText.Equals(""))
            {
                cmd.Parameters.Add(new SQLiteParameter("@cardText") { Value = "%" + cardText + "%"});
            }

            SQLiteDataReader reader = cmd.ExecuteReader();

            HashSet<string> cardNamesWithoutColors = new HashSet<string>();
            while (reader.Read())
                cardNamesWithoutColors.Add((string) reader["name"]);

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
            for (int i = 0; i < colors.Count; i++) {
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
                    tempCardsWithColors.Add((string) reader2["name"]);
                reader2.Close();

                if (i == 0)
                {
                    cardNamesWithColors = tempCardsWithColors;
                } else if (cardNamesWithColors != null) {
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

                c.CMC = (double) reader["cmc"];

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

        private void AddToCard(JSONDTO info)
        {
            // add this card
            string sql = @"insert into CARD(name, toughness, hand, cmc, reserved, loyalty, flavor, power, card_text, type, manacost)  values (@name, @toughness, @hand, @cmc, @reserved, @loyalty, @flavor, @power, @card_text, @type, @manacost)";

            // this is for passing in params
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = info.name });
            cmd.Parameters.Add(new SQLiteParameter("@toughness") { Value = info.toughness });
            cmd.Parameters.Add(new SQLiteParameter("@hand") { Value = info.hand });
            cmd.Parameters.Add(new SQLiteParameter("@cmc") { Value = info.cmc });
            cmd.Parameters.Add(new SQLiteParameter("@reserved") { Value = info.reserved });
            cmd.Parameters.Add(new SQLiteParameter("@loyalty") { Value = info.loyalty });
            cmd.Parameters.Add(new SQLiteParameter("@flavor") { Value = info.flavor });
            cmd.Parameters.Add(new SQLiteParameter("@power") { Value = info.power });
            cmd.Parameters.Add(new SQLiteParameter("@card_text") { Value = info.text });
            cmd.Parameters.Add(new SQLiteParameter("@type") { Value = info.type });
            cmd.Parameters.Add(new SQLiteParameter("@manacost") { Value = info.manaCost });

            cmd.ExecuteNonQuery();
        }

        private void AddRarity(string name, string multiverseId, string rarity)
        {
            string sql = @"insert into RARITIES(name, multiverse_id, rarity) values (@name, @multiverse_id, @rarity)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@multiverse_id") { Value = multiverseId });
            cmd.Parameters.Add(new SQLiteParameter("@rarity") { Value = rarity });
            cmd.ExecuteNonQuery();
        }

        private void AddArtist(string name, string multiverseId, string artist)
        {
            string sql = @"insert into ARTISTS(name, multiverse_id, artist) values (@name, @multiverse_id, @artist)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@multiverse_id") { Value = multiverseId});
            cmd.Parameters.Add(new SQLiteParameter("@artist") { Value = artist });
            cmd.ExecuteNonQuery();
        }

        private void AddMultiverseId(string name, string set, string multiverseId)
        {
            string sql = @"insert into MULTIVERSEID_SET(name, setName, multiverse_id) values (@name, @setName, @multiverse_id)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@setName") { Value = set });
            cmd.Parameters.Add(new SQLiteParameter("@multiverse_id") { Value = multiverseId });
            cmd.ExecuteNonQuery();
        }

        private void AddLanguage(string name, string language, string text)
        {
            string sql = @"insert into FOREIGN_NAMES(name, foreign_name, language) values (@name, @foreign_name, @language)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@foreign_name") { Value = text });
            cmd.Parameters.Add(new SQLiteParameter("@language") { Value = language });
            cmd.ExecuteNonQuery();
        }

        private void AddRuling(string name, string date, string text)
        {
            string sql = @"insert into RULINGS(name, date, ruling_text) values (@name, @date, @ruling_text)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@date") { Value = date });
            cmd.Parameters.Add(new SQLiteParameter("@ruling_text") { Value = text });
            cmd.ExecuteNonQuery();
        }

        private void AddLegality(string name, string modern, string legacy, string vintage, string freeform, string prismatic, string tribal, string singleton, string commander)
        {
            string sql = @"insert into LEGALITIES(name, format, legality_for_format) values (@name, @format, @legality_for_format)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;

            if (modern != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "modern" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = modern });
                cmd.ExecuteNonQuery();
            }
            if (legacy != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "legacy" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = legacy });
                cmd.ExecuteNonQuery();
            }
            if (vintage != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "vintage" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = vintage });
                cmd.ExecuteNonQuery();
            }
            if (freeform != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "freeform" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = freeform });
                cmd.ExecuteNonQuery();
            }
            if (prismatic != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "prismatic" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = prismatic });
                cmd.ExecuteNonQuery();
            }
            if (tribal != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "tribal wars legacy" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = tribal });
                cmd.ExecuteNonQuery();
            }
            if (singleton != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "singleton 100" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = singleton });
                cmd.ExecuteNonQuery();
            }
            if (commander != "")
            {
                cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
                cmd.Parameters.Add(new SQLiteParameter("@format") { Value = "commander" });
                cmd.Parameters.Add(new SQLiteParameter("@legality_for_format") { Value = commander });
                cmd.ExecuteNonQuery();
            }
        }

        private void AddColors(string name, string color)
        {
            string sql = "insert into COLORS (name, color) values (@name, @color)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@color") { Value = color });
            cmd.ExecuteNonQuery();
        }

        private void AddSupertypes(string name, string supertype)
        {
            string sql = "insert into SUPERTYPES (name, supertype) values (@name, @supertype)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@supertype") { Value = supertype });
            cmd.ExecuteNonQuery();
        }


        private void AddSubtypes(string name, string subtype)
        {
            string sql = "insert into SUBTYPES (name, subtype) values (@name, @subtype)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@subtype") { Value = subtype });
            cmd.ExecuteNonQuery();
        }

        private void AddTypes(string name, string type)
        {
            string sql = "insert into TYPES (name, type) values (@name, @type)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@type") { Value = type });
            cmd.ExecuteNonQuery();
        }

        private void CreateTable(string sql)
        {
            // create command to execute building of table
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);

            // execute the command and expect no response
            command.ExecuteNonQuery();
        }
    }

    [DataContract]
    public class JSONDTO
    {
        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "cmc")]
        public double cmc { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "text")]
        public string text { get; set; }

        [DataMember(Name = "flavor")]
        public string flavor { get; set; }

        [DataMember(Name = "power")]
        public string power { get; set; }

        [DataMember(Name = "toughness")]
        public string toughness { get; set; }

        [DataMember(Name = "loyalty")]
        public int loyalty { get; set; }

        [DataMember(Name = "multiverseid")]
        public int multiverseid { get; set; }

        [DataMember(Name = "variations")]
        public List<int> variations { get; set; }

        [DataMember(Name = "hand")]
        public int hand { get; set; }

        [DataMember(Name = "reserved")]
        public bool reserved { get; set; }

        [DataMember(Name = "manaCost")]
        public string manaCost { get; set; }
    }

    [DataContract]
    class LegalitiesDTO
    {
        [DataMember(Name = "Modern")]
        public string Modern { get; set; }

        [DataMember(Name = "Legacy")]
        public string Legacy { get; set; }

        [DataMember(Name = "Vintage")]
        public string Vintage { get; set; }

        [DataMember(Name = "Freeform")]
        public string Freeform { get; set; }

        [DataMember(Name = "Prismatic")]
        public string Prismatic { get; set; }

        [DataMember(Name = "Tribal Wars Legacy")]
        public string TribalWarsLegacy { get; set; }

        [DataMember(Name = "Singleton 100")]
        public string Singleton100 { get; set; }

        [DataMember(Name = "Commander")]
        public string Commander { get; set; }
    }
}