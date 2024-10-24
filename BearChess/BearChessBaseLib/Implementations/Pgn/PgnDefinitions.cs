﻿using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.pgn
{
    public static class PgnDefinitions
    {
        public static Dictionary<string, string> SymbolToNag = new Dictionary<string, string>()
                                                          {
                                                              {"!", "$1"},
                                                              {"?", "$2"},
                                                              {"!!", "$3"},
                                                              {"??", "$4"},
                                                              {"!?", "$5"},
                                                              {"?!", "$6"},
                                                              {"□", "$7"},
                                                              {"=", "$10"},
                                                              {"∞", "$13"},
                                                              {"+=", "$14"},
                                                              {"=+", "$15"},
                                                              {"±", "$16"},
                                                              {"∓", "$17"},
                                                              {"+-", "$18"},
                                                              {"-+", "$19"},
                                                              {"⟳", "$26"},
                                                              {"↑", "$36"},
                                                              {"→", "$40"},
                                                              {"∞=", "$44"},
                                                              {"⨁", "$138"},
                                                              {"∆", "$140"},
                                                              {"∇", "$141"},
                                                              {"⌓", "$142"},
                                                              {"<=", "$143"},
                                                              {"==", "$144"},
                                                              {"○", "$238"},
                                                              {"⇔", "$239"},
                                                          };
        public static Dictionary<string, string> NagToSymbol = new Dictionary<string, string>()
                                                          {
                                                              {"$1","!"},
                                                              {"$2","?"},
                                                              {"$3","!!"},
                                                              {"$4","??"},
                                                              {"$5","!?"},
                                                              {"$6","?!"},
                                                              {"$7","□"},
                                                              {"$10","="},
                                                              {"$13","∞"},
                                                              {"$14","+="},
                                                              {"$15","=+"},
                                                              {"$16","±"},
                                                              {"$17","∓"},
                                                              {"$18","+-"},
                                                              {"$19","-+"},
                                                              {"$26","⟳"},
                                                              {"$27","⟳"},
                                                              {"$36","↑"},
                                                              {"$37","↑"},
                                                              {"$40","→"},
                                                              {"$41","→"},
                                                              {"$44","∞="},
                                                              {"$138","⨁"},
                                                              {"$139","⨁"},
                                                              {"$140","∆"},
                                                              {"$141","∇"},
                                                              {"$142","⌓"},
                                                              {"$143","<="},
                                                              {"$144","=="},
                                                              {"$238","○"},
                                                              {"$239","⇔"},
                                                          };
    }
}
