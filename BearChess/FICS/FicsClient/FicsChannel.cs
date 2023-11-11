using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessWin
{

    public static class FicsChannelNames
    {
        private static Dictionary<string, string> AllNames = new Dictionary<string, string>()
                                                             {
                                                                 { "0", "Admins" },
                                                                 { "1", "Server Help and Assistance" },
                                                                 { "2", "General discussions about FICS" },
                                                                 { "3", "FICS programmers" },
                                                                 { "4", "Guests" },
                                                                 { "5", "Service Representatives [restricted]" },
                                                                 { "6", "Help with Interface and timeseal questions" },
                                                                 { "7", "OnlineTours" },
                                                                 { "20", "Forming Team games" },
                                                                 { "21", "Playing team games" },
                                                                 { "22", "Playing team games" },
                                                                 { "23", "Forming Simuls" },
                                                                 { "30", "Books and Knowledge" },
                                                                 { "31", "Computer Games" },
                                                                 { "32", "Movies" },
                                                                 { "33", "Quacking & Other Duck Topics" },
                                                                 { "34", "Sports" },
                                                                 { "35", "Music" },
                                                                 { "36", "Mathematics & Physics" },
                                                                 { "37", "Philosophy" },
                                                                 { "38", "Literature & Poetry" },
                                                                 { "39", "Politics" },
                                                                 { "48", "Mamer managers" },
                                                                 { "49", "Mamer tournament channel" },
                                                                 { "50", "The Chat channel" },
                                                                 { "51", "The Youth channel" },
                                                                 { "52", "The Old Timers channel" },
                                                                 { "53", "The Guest Chat channel" },
                                                                 { "55", "The Chess channel" },
                                                                 { "56", "Beginner Chess" },
                                                                 { "57", "Discussions on coaching and teaching chess" },
                                                                 { "58", "Chess Books" },
                                                                 { "60", "Chess Openings/Theory" },
                                                                 { "61", "Chess Endgames" },
                                                                 { "62", "Blindfold Chess channel" },
                                                                 { "63", "Chess Advisors [restricted]" },
                                                                 { "64", "Computer Chess" },
                                                                 { "65", "Special Events channel" },
                                                                 { "66", "Examine channel" },
                                                                 { "67", "Lecture channel" },
                                                                 { "68", "Ex-Yugoslav" },
                                                                 { "69", "Latin" },
                                                                 { "70", "Finnish" },
                                                                 { "71", "Scandinavian (Danish, Norwegian, Swedish)" },
                                                                 { "72", "German" },
                                                                 { "73", "Spanish" },
                                                                 { "74", "Italian" },
                                                                 { "75", "Russian" },
                                                                 { "76", "Dutch" },
                                                                 { "77", "French" },
                                                                 { "78", "Greek" },
                                                                 { "79", "Icelandic" },
                                                                 { "80", "Chinese" },
                                                                 { "81", "Turkish" },
                                                                 { "82", "Portuguese" },
                                                                 { "83", "General computer discussions" },
                                                                 { "84", "Macintosh/Apple" },
                                                                 { "85", "Unix/Linux" },
                                                                 { "86", "Windows" },
                                                                 { "87", "VMS" },
                                                                 { "88", "Programming discussions" },
                                                                 { "90", "The STC BUNCH" },
                                                                 { "91", "Suicide Chess channel" },
                                                                 { "92", "Wild Chess channel" },
                                                                 { "93", "Bughouse Chess channel" },
                                                                 { "94", "Gambit channel" },
                                                                 { "95", "Scholastic Chess channel" },
                                                                 { "96", "College Chess channel" },
                                                                 { "97", "Crazyhouse Chess channel" },
                                                                 { "98", "Losers Chess channel" },
                                                                 { "99", "Atomic Chess channel" },
                                                                 { "100", "Trivia" },
                                                             };
    
        public static string GetName(string number)
        {
            if (AllNames.ContainsKey(number))
                return AllNames[number];
            return number;

        }
    }

    public class FicsChannel
    {

        
        public string Number { get;  }
        public string Name { get;  }

        public FicsChannel(string number)
        {
            Number = number;
            Name = FicsChannelNames.GetName(number);
        }

        public override string ToString()
        {
            return $"{Number} {Name}";
        }
    }
}