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

            // check if the file exists

            // if not, we want to grab the file and create the db
            createDBFile();
            
        }

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
            hashedJSONFile = HashJSON(mainJSONFileName);
        }

        public int HashJSON(string fileName)
        {
            string file = File.ReadAllText(fileName);
            return file.GetHashCode();
        }

        public void DownloadNewFile() {
            using (WebClient myClient = new WebClient()) {
                Debug.WriteLine("Downloading new file");
                myClient.DownloadFile("http://mtgjson.com/json/AllSets-x.json", "AllSets-x.json");
                Debug.WriteLine("Downloaded file");
            }
            if (hashedJSONFile != HashJSON("AllSets-x.json")) {
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
            }
            Debug.WriteLine("Deleting copy of json file");
            //File.Delete("AllCards-x2.json");
        }

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

                // open the database, need to close later
                dbConnection.Open();

                // don't forget to check if they match.
                try
                {
                    String mtgJSONText = File.ReadAllText("AllSets-x.json", Encoding.UTF8);
                    // build our RULINGS table
                    string sql = @"create table RULINGS
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

                    // build TYPES table
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
                                layout TEXT, 
                                toughness TEXT,
                                manacost TEXT, 
                                hand INTEGER, 
                                cmc REAL, 
                                timeshifted NUMERIC, 
                                reserved NUMERIC, 
                                release_date TEXT, 
                                source TEXT, 
                                border TEXT, 
                                watermark TEXT,
                                loyalty INTEGER, 
                                rarity TEXT, 
                                original_type TEXT, 
                                flavor TEXT, 
                                artist TEXT, 
                                number TEXT, 
                                power TEXT, 
                                life INTEGER,
                                card_text TEXT, 
                                type TEXT, 
                                PRIMARY KEY (name))";
                    CreateTable(sql);

                    JToken outer = JToken.Parse(mtgJSONText);

                    var o = JObject.Parse(mtgJSONText);

                    List<String> elements = new List<String>();
                    foreach (JToken child in o.Children())
                    {
                        var property = child as JProperty;

                        if (property != null)
                        {
                            // gets the set names
                            elements.Add(property.Name);
                        }
                        else
                        {
                            Debug.WriteLine("null");
                        }
                    }

                    Debug.WriteLine("Number of sets " + elements.Count);

                    // this is a list of all of the cards we have met previously
                    List<String> cardsMet = new List<String>();

                    var ser = new DataContractJsonSerializer(typeof(JSONDTO));
                    for (int i = 0; i < elements.Count; i++)
                    {
                        JObject inner = outer[elements[i]].Value<JObject>();

                        // save the set information
                        // get the card information

                        foreach (var cardInCards in inner["cards"].Children())
                        {
                            JObject cardJObject = JObject.Parse(cardInCards.ToString());
                            if (!cardsMet.Contains(cardJObject["name"].ToString()))
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

                                AddToCard(info); // add border info
                                cardsMet.Add(cardJObject["name"].ToString());
                            }
                            if (cardJObject["multiverseid"] != null)
                            {
                                AddMultiverseId(cardJObject["name"].ToString(), inner["name"].ToString(), cardJObject["multiverseid"].ToString());
                            }
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine("Unable to open AllCards-x.json file.");
                    Debug.WriteLine("Deleting database file");
                    //File.Delete("SetsDB.sqlite");

                }
            }
        }

        public List<string> GetCards(string name, string toughness, string hand, 
            string cmc, string releaseDate, string border, 
            string watermark, string multiverseId, string loyalty, string rarity, string flavor,
            string artist, string number, string power, string life, string imageName, string cardText, 
            string type, bool? timeShifted, bool requireMultiColor, List<string> colors, string subtype, string supertype)
        {
            // id
            // timeshifted
            Debug.WriteLine("in getcards");

            string sqlCard = @"select name 
                           from CARD
                           where name like @name
                           and toughness like @toughness
                           and hand like @hand
                           and cmc like @cmc
                           and (release_date like @releaseDate or release_date is null)
                           and (border like @border or border is null)
                           and (watermark like @watermark or watermark is null)
                           and loyalty like @loyalty
                           and (rarity like @rarity or rarity is null)
                           and (flavor like @flavor or flavor is null)
                           and (artist like @artist or artist is null)
                           and (number like @number or number is null)
                           and power like @power
                           and life like @life
                           and card_text like @cardText
                           and type like @type";

            if (timeShifted != null)
            {
                sqlCard += "and timeshifted like @timeshifted";
            }

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sqlCard;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = "%" + name + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@toughness") { Value = "%" + toughness + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@hand") { Value = "%" + hand + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@cmc") { Value = "%" + cmc + "%" });
            cmd.Parameters.Add(new SQLiteParameter("@releaseDate") { Value = "%" + releaseDate + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@border") { Value = "%" + border + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@watermark") { Value = "%" + watermark  + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@loyalty") { Value = "%" + loyalty + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@rarity") { Value = "%" + rarity + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@flavor") { Value = "%" + flavor + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@artist") { Value = "%" + artist + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@number") { Value = "%" + number + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@power") { Value = "%" + power + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@life") { Value = "%" + life + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@cardText") { Value = "%" + cardText + "%"});
            cmd.Parameters.Add(new SQLiteParameter("@type") { Value = "%" + type + "%"});

            if (timeShifted != null)
            {
                cmd.Parameters.Add(new SQLiteParameter("@timeshifted") { Value = "%" + timeShifted + "%" });
            }

            SQLiteDataReader reader = cmd.ExecuteReader();

            HashSet<string> cardNamesWithoutColors = new HashSet<string>();
            while (reader.Read())
                cardNamesWithoutColors.Add((string) reader["name"]);

            reader.Close();

            // what about the original color of the card? fix
            /*List<string> allColors = new List<string>();
            allColors.Add("red");
            allColors.Add("blue");
            allColors.Add("black");
            allColors.Add("green");
            allColors.Add("white");

            List<string> unwantedColors = allColors.Except(colors).ToList<string>(); */

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

                if (i == 0)
                {
                    cardNamesWithColors = tempCardsWithColors;
                } else if (cardNamesWithColors != null) {
                    if (requireMultiColor)
                        cardNamesWithColors.IntersectWith(tempCardsWithColors);
                    else
                        cardNamesWithColors.UnionWith(tempCardsWithColors);
                }
            }

            /*if (cardNamesWithColors != null && cardNamesWithColors.Count > 0 && requireMultiColor)
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
                    List<string> cardNamesWithColorsList = cardNamesWithColors.Except(cardNamesWithColors).ToList<string>();
                    cardNamesWithColors = new HashSet<string>(cardNamesWithColorsList);
                }
                cardNamesWithoutColors.IntersectWith(cardNamesWithColors);
            }
            else*/ if (cardNamesWithColors != null && cardNamesWithColors.Count > 0)
            {
                cardNamesWithoutColors.IntersectWith(cardNamesWithColors);
            }

            // foreign names?
            // should be able to turn on or off searching by foreign names
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
            // also printings?


            // search by types, subtypes, supertypes
            if (type != "")
            {
                string sqlType = @"select name
                                   from TYPES
                                   where type like @type";
                var cmd3 = dbConnection.CreateCommand();
                cmd3.CommandText = sqlType;
                cmd3.Parameters.Add(new SQLiteParameter("@type") { Value = "%" + type + "%" });
                SQLiteDataReader reader3 = cmd3.ExecuteReader();

                HashSet<string> cardNamesWithTypes = new HashSet<string>();

                while (reader3.Read())
                    cardNamesWithTypes.Add((string)reader3["name"]);

                cardNamesWithoutColors.IntersectWith(cardNamesWithTypes);
            }

            if (subtype != "")
            {
                string sqlSubtype = @"select name
                                   from SUBTYPES
                                   where subtype like @subtype";
                var cmd4 = dbConnection.CreateCommand();
                cmd4.CommandText = sqlSubtype;
                cmd4.Parameters.Add(new SQLiteParameter("@subtype") { Value = "%" + subtype + "%" });
                SQLiteDataReader subtypeReader = cmd4.ExecuteReader();

                HashSet<string> cardNamesWithSubtypes = new HashSet<string>();

                while (subtypeReader.Read())
                    cardNamesWithSubtypes.Add((string)subtypeReader["name"]);

                cardNamesWithoutColors.IntersectWith(cardNamesWithSubtypes);
            }

            if (supertype != "")
            {
                string sqlSupertype = @"select name
                                   from SUPERTYPES
                                   where supertype like @supertype";
                var cmd5 = dbConnection.CreateCommand();
                cmd5.CommandText = sqlSupertype;
                cmd5.Parameters.Add(new SQLiteParameter("@supertype") { Value = "%" + supertype + "%" });
                SQLiteDataReader supertypeReader = cmd5.ExecuteReader();

                HashSet<string> cardNamesWithSupertypes = new HashSet<string>();

                while (supertypeReader.Read())
                    cardNamesWithSupertypes.Add((string)supertypeReader["name"]);

                cardNamesWithoutColors.IntersectWith(cardNamesWithSupertypes);
            }

            return cardNamesWithoutColors.ToList(); 
        }

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
                if (!(reader["artist"] is DBNull))
                    c.Artist = (string) reader["artist"];

                if (!(reader["border"] is DBNull))
                    c.Border = (string) reader["border"];
                c.CMC = (double) reader["cmc"];

                if (!(reader["flavor"] is DBNull))
                    c.Flavor = (string)reader["flavor"];

                c.Hand = (long)reader["hand"];

                if (!(reader["layout"] is DBNull))
                    c.Layout = (string)reader["layout"];

                c.Life = (long)reader["life"];

                c.Loyalty = (long)reader["loyalty"];

                c.Name = (string)reader["name"];

                if (!(reader["number"] is DBNull))
                    c.Number = (string)reader["number"];
                c.Power = (string)reader["power"];

                if (!(reader["rarity"] is DBNull))
                    c.Rarity = (string)reader["rarity"];

                if (!(reader["release_date"] is DBNull))
                    c.ReleaseDate = (string)reader["release_date"];

                if (!(reader["card_text"] is DBNull))
                    c.Text = (string)reader["card_text"];

                if ((decimal)reader["timeshifted"] == 0)
                    c.Timeshifted = false;
                else
                    c.Timeshifted = true;

                c.Toughness = (string)reader["toughness"];

                c.Type = (string)reader["type"];

                if (!(reader["watermark"] is DBNull))
                    c.Watermark = (string)reader["watermark"];
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

            // add foreign names
            // HOW DO WE WANT TO REPRESENT FOREIGN NAMES AND OTHER OBJECTS

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
                    where name like @name";

            cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });

            reader = cmd.ExecuteReader();
            c.Types = new List<string>();

            while (reader.Read())
            {
                c.Types.Add((string)reader["type"]);
            }

            // return multiverse_id and set as dictionary, set is key, multiverse_id is value

            // add variations
            // legalities?
            return c;
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

        public string GetAMultiverseId(string name)
        {
            string sql = @"select multiverse_id
                           from MULTIVERSEID_SET
                           where name like @name";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return (string)reader["multiverse_id"];
            }
            return null;
        }

        public void AddToCard(JSONDTO info)
        {
            // add this card
            string sql = @"insert into CARD(name, toughness, hand, cmc, timeshifted, reserved, release_date, border, watermark, loyalty, rarity, flavor, artist, number, power, life, card_text, type)  values (@name, @toughness, @hand, @cmc, @timeshifted, @reserved, @release_date, @border, @watermark, @loyalty, @rarity, @flavor, @artist, @number, @power, @life, @card_text, @type)";

            // this is for passing in params
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = info.name });
            cmd.Parameters.Add(new SQLiteParameter("@toughness") { Value = info.toughness });
            cmd.Parameters.Add(new SQLiteParameter("@hand") { Value = info.hand });
            cmd.Parameters.Add(new SQLiteParameter("@cmc") { Value = info.cmc });
            cmd.Parameters.Add(new SQLiteParameter("@timeshifted") { Value = info.timeshifted });
            cmd.Parameters.Add(new SQLiteParameter("@reserved") { Value = info.reserved });
            cmd.Parameters.Add(new SQLiteParameter("@release_date") { Value = info.releaseDate });
            cmd.Parameters.Add(new SQLiteParameter("@border") { Value = info.border });
            cmd.Parameters.Add(new SQLiteParameter("@watermark") { Value = info.watermark });
            cmd.Parameters.Add(new SQLiteParameter("@loyalty") { Value = info.loyalty });
            cmd.Parameters.Add(new SQLiteParameter("@rarity") { Value = info.rarity });
            cmd.Parameters.Add(new SQLiteParameter("@flavor") { Value = info.flavor });
            cmd.Parameters.Add(new SQLiteParameter("@artist") { Value = info.artist });
            cmd.Parameters.Add(new SQLiteParameter("@number") { Value = info.number });
            cmd.Parameters.Add(new SQLiteParameter("@power") { Value = info.power });
            cmd.Parameters.Add(new SQLiteParameter("@life") { Value = info.life });
            cmd.Parameters.Add(new SQLiteParameter("@card_text") { Value = info.text });
            cmd.Parameters.Add(new SQLiteParameter("@type") { Value = info.type });

            cmd.ExecuteNonQuery();
        }

        // rulings
        // foreign names
        // legalities
        public void AddMultiverseId(string name, string set, string multiverseId)
        {
            string sql = @"insert into MULTIVERSEID_SET(name, setName, multiverse_id) values (@name, @setName, @multiverse_id)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@setName") { Value = set });
            cmd.Parameters.Add(new SQLiteParameter("@multiverse_id") { Value = multiverseId });
            cmd.ExecuteNonQuery();
        }

        public void AddLanguage(string name, string language, string text)
        {
            string sql = @"insert into FOREIGN_NAMES(name, foreign_name, language) values (@name, @foreign_name, @language)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@foreign_name") { Value = text });
            cmd.Parameters.Add(new SQLiteParameter("@language") { Value = language });
            cmd.ExecuteNonQuery();
        }

        public void AddRuling(string name, string date, string text)
        {
            string sql = @"insert into RULINGS(name, date, ruling_text) values (@name, @date, @ruling_text)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@date") { Value = date });
            cmd.Parameters.Add(new SQLiteParameter("@ruling_text") { Value = text });
            cmd.ExecuteNonQuery();
        }

        public void AddLegality(string name, string modern, string legacy, string vintage, string freeform, string prismatic, string tribal, string singleton, string commander)
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

        public void AddColors(string name, string color)
        {
            string sql = "insert into COLORS (name, color) values (@name, @color)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@color") { Value = color });
            cmd.ExecuteNonQuery();
        }

        public void AddSupertypes(string name, string supertype)
        {
            string sql = "insert into SUPERTYPES (name, supertype) values (@name, @supertype)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@supertype") { Value = supertype });
            cmd.ExecuteNonQuery();
        }


        public void AddSubtypes(string name, string subtype)
        {
            string sql = "insert into SUBTYPES (name, subtype) values (@name, @subtype)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@subtype") { Value = subtype });
            cmd.ExecuteNonQuery();
        }

        public void AddTypes(string name, string type)
        {
            string sql = "insert into TYPES (name, type) values (@name, @type)";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SQLiteParameter("@name") { Value = name });
            cmd.Parameters.Add(new SQLiteParameter("@type") { Value = type });
            cmd.ExecuteNonQuery();
        }

        public void CreateTable(string sql)
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
        [DataMember(Name = "layout")]
        public string layout { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "cmc")]
        public double cmc { get; set; }

        [DataMember(Name = "type")]
        public string type { get; set; }

        [DataMember(Name = "rarity")]
        public string rarity { get; set; }

        [DataMember(Name = "text")]
        public string text { get; set; }

        [DataMember(Name = "flavor")]
        public string flavor { get; set; }

        [DataMember(Name = "artist")]
        public string artist { get; set; }

        [DataMember(Name = "number")]
        public string number { get; set; }

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

        [DataMember(Name = "watermark")]
        public string watermark { get; set; }

        [DataMember(Name = "border")]
        public string border { get; set; }

        [DataMember(Name = "timeshifted")]
        public bool timeshifted { get; set; }

        [DataMember(Name = "hand")]
        public int hand { get; set; }

        [DataMember(Name = "life")]
        public int life { get; set; }

        [DataMember(Name = "reserved")]
        public bool reserved { get; set; }

        [DataMember(Name = "releaseDate")]
        public string releaseDate { get; set; }

        [DataMember(Name = "manaCost")]
        public string manaCost { get; set; }

        // release date
        // source
        // border
        // watermark
        // original type
        // number
        // remove image name
    }

    [DataContract]
    public class LegalitiesDTO
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