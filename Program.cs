using System;
using System.Collections.Generic;
using System.IO;

namespace Smarttwigs_Application_Exercise
{
    public class Player
    {
        // Properties
        public string name { get; set; }
        public int wins { get; set; } = 0;
        public int points { get; set; } = 0;
        public int cp { get; set; } = 0;
        public bool isServing { get; set; }

        // Methods
        public void updatePoints()
        {
            this.points += 1;
            this.cp += 1;
        }
        public void clearPoints()
        {
            this.points = 0;
        }
    }

    public class DBAccess
    {
        string path = "C:\\Users\\Matth\\Desktop\\Smattwigs Interview Project\\Smarttwigs Application Exercise\\Smarttwigs Application Exercise\\";
        string dbName = "players_db.txt";

        public string ReadFromDB()
        {
            return File.ReadAllText(path + dbName);
        }

        public void WriteToDB(string input)
        {
            using (StreamWriter writeText = new StreamWriter(path + dbName))
            {
                writeText.WriteLine(input);
            }
        }

        public void AddPlayerToDB(Player player)
        {
            string currDB = ReadFromDB();
            List<Player> players = new List<Player>();
            players.Add(player);

            string playerStr = SetPlayersToString(players);

            currDB += playerStr;

            WriteToDB(currDB);
        }

        public List<Player> GetPlayersFromString(string input)
        {
            List<Player> players = new List<Player>();

            string[] lines = input.Split('\n');

            foreach (string line in lines)
            {
                // Error Handling
                if(line == "")
                {
                    break;
                }

                string[] data = line.Split(" ");

                Player p = new Player();
                p.name = data[0];
                p.wins = int.Parse(data[1]);
                p.cp = int.Parse(data[2]);

                players.Add(p);
            }

            return players;
        }

        public String SetPlayersToString(List<Player> players)
        {
            string output = "";

            foreach (Player p in players)
            {
                string line = $"{p.name} {p.wins} {p.cp}";
                output += "\n" + line;
            }

            return output;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            DBAccess dbAccess = new DBAccess();
            List<Player> players = dbAccess.GetPlayersFromString(dbAccess.ReadFromDB());

            bool tournamentOver = false;

            // Init who is serving
            Player user = new Player();
            Player rival = new Player();

            // Tournament loop
            while (!tournamentOver)
            {
                bool gameOver = false;
                bool creatingPlayers = true;
                int roundNum = 0;

                Console.WriteLine("Would you like to play a game?(y/n): ");
                string playAGame = Console.ReadLine();

                if (playAGame == "n")
                {
                    tournamentOver = true;
                    dbAccess.WriteToDB(dbAccess.SetPlayersToString(players));
                    break;
                }

                // Creating player list
                while (creatingPlayers)
                {
                    Console.WriteLine("Do you want to create a player (y/n)?:");
                    string answer = Console.ReadLine();

                    if (answer == "y")
                    {
                        Player newPlayer = createPlayer();
                        players.Add(newPlayer);
                        dbAccess.AddPlayerToDB(newPlayer);
                    }
                    else if (answer == "n")
                    {
                        creatingPlayers = false;
                    }
                    else
                    {
                        Console.WriteLine("Wrong input, try again");
                    }
                }

                // Choose User
                Console.WriteLine("Who is the user");

                for (int i = 0; i < players.Count; i++)
                {
                    Console.WriteLine($"[{i}] {players[i].name}");
                }

                int userIndex = int.Parse(Console.ReadLine());

                // Choose Rival
                Console.WriteLine("Who is the rival");

                for (int i = 0; i < players.Count; i++)
                {
                    Console.WriteLine($"[{i}] {players[i].name}");
                }

                int rivalIndex = int.Parse(Console.ReadLine());

                user = players[userIndex];
                rival = players[rivalIndex];

                user.isServing = true;
                rival.isServing = false;

                //Console.WriteLine("Who do you want to be the user?");

                // Main Loop
                while (!gameOver)
                {
                    roundNum += 1;
                    // Get user input
                    Console.WriteLine($"Who won round({roundNum})?: (1){user.name}, (2){rival.name}, (3)EndGame");
                    int input = Int32.Parse(Console.ReadLine());

                    gameOver = RunRound(input, user, rival);

                    // Checks every two rounds
                    if (roundNum > 0 && (roundNum % 2 == 0))
                    {
                        swapServer(user, rival);
                    }

                    if (gameOver == true)
                    {
                        break;
                    }

                    gameOver = checkEndGame(user, rival);
                }

                Console.WriteLine("Yay, game is over!");

                Console.WriteLine("This is the leaderboard");

                players = OrderList(players);

                foreach (Player player in players)
                {
                    Console.WriteLine($"{player.name} has [{player.wins}] wins and [{player.cp}] cummulative points");
                }
            }

            
        }

        public static List<Player> OrderList(List<Player> list)
        {
            List<Player> output = new List<Player>();

            while (list.Count > 0)
            {
                if(list.Count == 1)
                {
                    output.Add(list[0]);
                    return output;
                }

                int maxWins = 0;
                int index = 0;

                for (int i = 0; i < list.Count - 1; i++)
                {
                    // Check to see if player has more wins
                    if (maxWins < list[i].wins)
                    {
                        maxWins = list[i].wins;
                        index = i;
                    }
                    else if(list[i].cp == list[i+1].wins)
                    {
                        index = (list[i].cp > list[i+1].cp) ? i : i+1;
                    }
                }

                output.Add(list[index]);
                list.RemoveAt(index);
            }
            

            return output;
        }

        public static void swapServer(Player user, Player rival)
        {
            if (user.isServing)
            {
                user.isServing = false;
                rival.isServing = true;
            }
            else
            {
                user.isServing = true;
                rival.isServing = false;
            }

            Console.WriteLine($"{user.name} is server = {user.isServing} and " +
                              $"{rival.name} is serving = {rival.isServing}");
        }

        // Returns the state of the game
        public static bool RunRound(int input, Player user, Player rival)
        {
            if(input == 1)
            {
                user.updatePoints();
                Console.WriteLine($"{user.name} points is now: {user.points}");
                return false;
            } else if(input == 2)
            {
                rival.updatePoints();
                Console.WriteLine($"{rival.name} points is now: {rival.points}");
                return false;
            }
            else if (input == 3)
            {
                Console.WriteLine($"Game is now over");
                return true;
            }
            else
            {
                Console.WriteLine("Wrong input. Try again");
                return false;
            }
        }

        public static Player createPlayer()
        {
            Player player = new Player();

            Console.WriteLine("What is the name of the player?: ");
            string name = Console.ReadLine();

            player.name = name;

            return player;
        }
        public static bool checkEndGame(Player user, Player rival)
        {
            bool result = false;

            // If user is above 10 points and exceeds rival by 2 points, user wins
            if (user.points > 10 && (user.points - rival.points >= 2))
            {
                Console.WriteLine($"{user.name} won");
                Console.WriteLine($"{user.name} had a total of {user.points} points," +
                                  $" and {rival.name} had a total of {rival.points} points");
                user.clearPoints();
                user.wins += 1;
                Console.WriteLine($"User has a a total number of {user.wins} wins");
                rival.clearPoints();
                result = true;
            }
            // If rival is  above 10 points and exceeds rival by 2 points, rival wins
            else if(rival.points > 10 && (rival.points - user.points >= 2))
            {
                Console.WriteLine($"{rival.name} won");
                Console.WriteLine($"{user.name} had a total of {user.points} points," +
                                  $" and {rival.name} had a total of {rival.points} points");
                user.clearPoints();
                rival.clearPoints();
                rival.wins += 1;
                Console.WriteLine($"{rival.name} has a a total number of {rival.wins} wins");
                result = true;
            }
            else
            {

            }


            return result;
        }
    }
}
