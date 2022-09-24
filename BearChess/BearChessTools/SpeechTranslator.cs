using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public static class SpeechTranslator
    {

        public static string GetWelcome(string langCode, Configuration configuration)
        {
            string result = "Welcome to BearChess";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Willkommen zu Bärchess";
            }
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechWelcome", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
          
        }

        public static string GetFigureName(int figureId, string langCode, Configuration configuration)
        {
            string result = FigureId.FigureIdToEnName[figureId];
            string figureName = string.Empty;

            if (langCode.Contains("de"))
            {
                result = FigureId.FigureIdToDeName[figureId];
            }
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                switch (figureId)
                {
                    case FigureId.WHITE_KING: figureName = configuration.GetConfigValue("SpeechKing", string.Empty);
                        break;
                    case FigureId.WHITE_QUEEN: figureName = configuration.GetConfigValue("SpeechQueen", string.Empty);
                        break;
                    case FigureId.WHITE_ROOK: figureName = configuration.GetConfigValue("SpeechRook", string.Empty);
                        break;
                    case FigureId.WHITE_BISHOP: figureName = configuration.GetConfigValue("SpeechBishop", string.Empty);
                        break;
                    case FigureId.WHITE_KNIGHT: figureName = configuration.GetConfigValue("SpeechKnight", string.Empty);
                        break;
                    case FigureId.WHITE_PAWN: figureName = configuration.GetConfigValue("SpeechPawn", string.Empty);
                        break;
                    case FigureId.BLACK_KING: figureName = configuration.GetConfigValue("SpeechKing", string.Empty);
                        break;
                    case FigureId.BLACK_QUEEN: figureName = configuration.GetConfigValue("SpeechQueen", string.Empty);
                        break;
                    case FigureId.BLACK_ROOK: figureName = configuration.GetConfigValue("SpeechRook", string.Empty);
                        break;
                    case FigureId.BLACK_BISHOP: figureName = configuration.GetConfigValue("SpeechBishop", string.Empty);
                        break;
                    case FigureId.BLACK_KNIGHT: figureName = configuration.GetConfigValue("SpeechKnight", string.Empty);
                        break;
                    case FigureId.BLACK_PAWN: figureName = configuration.GetConfigValue("SpeechPawn", string.Empty);
                        break;
                    default: figureName = string.Empty;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(figureName))
            {
                return result;
            }
            return figureName;
        }

        public static string GetFrom(string langCode, Configuration configuration)
        {
            string result = "from";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "von";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig =  configuration.GetConfigValue("SpeechFrom", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetTo(string langCode, Configuration configuration)
        {
            string result = "to";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "nach";
            }
           
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechTo", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetCheck(string langCode, Configuration configuration)
        {

            string result = "check";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Schach";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechCheck", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetGameEnd(string langCode, Configuration configuration)
        {

            string result = "game finished";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Spiel beendet";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechGameFinished", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetMate(string langCode, Configuration configuration)
        {
            string result = "check mate";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Schach Matt";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechCheckMate", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetDraw(string langCode, Configuration configuration)
        {

            string result = "draw";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Remis";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechDraw", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetNewGame(string langCode, Configuration configuration)
        {

            string result = "new game";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Neues Spiel";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechNewGame", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetCapture(string langCode, Configuration configuration)
        {

            string result = "takes";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "schlägt";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechTakes", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetCastleKingsSide(string langCode, Configuration configuration)
        {

            string result = "castling king's side";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "kurze Rochade";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechCastleKing", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetCastleQueensSide(string langCode, Configuration configuration)
        {

            string result = "castling queen's side";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "lange Rochade";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechCastleQueen", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetAgainst(string langCode, Configuration configuration)
        {

            string result = "against";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gegen";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechAgainst", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetWinsByMate(string langCode, Configuration configuration)
        {

            string result = "wins by mate";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gewinnt durch Matt";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechWinsByMate", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetWinsByScore(string langCode, Configuration configuration)
        {

            string result = "wins by score";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gewinnt durch Bewertung";
            }
            
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechWinsByScore", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
            return resultConfig;
        }

        public static string GetFICSWelcome(string langCode, Configuration configuration)
        {
            string result = "Welcome to Free Internet Chess Server";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Willkommen beim Free Internet Chess Server";
            }
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechFICSWelcome", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }
           
            return resultConfig;
        }
        public static string GetFICSConnectedAs(string langCode, Configuration configuration)
        {
            string result = "Your are connected as %USERNAME%";
            string resultConfig = string.Empty;
            if (langCode.Contains("de"))
            {
                result = "Sie sind angemeldet als %USERNAME%";
            }
           
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechFICSConnectedAs", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }

            return resultConfig;
            
        }

        public static string GetFICSChallenge(string langCode, Configuration configuration)
        {
            string result = "%OPPONENT% challenges you to a game";
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "%OPPONENT% fordert Sie zu einer Partie heraus";
            }

            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                resultConfig = configuration.GetConfigValue("SpeechFICSChallenge", string.Empty);
            }

            if (string.IsNullOrWhiteSpace(resultConfig))
            {
                return result;
            }

            return resultConfig;


          
        }
    }
}
