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
            string result = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Willkommen zu Bärchess";
            }
            if (langCode.Contains("en"))
            {
                result = "Welcome to BearChess";
            }
            if (bool.Parse(configuration.GetConfigValue("SpeechOwnLanguage", "false")))
            {
                return configuration.GetConfigValue("SpeechWelcome", result);
            }
            return result;
        }

        public static string GetFigureName(int figureId, string langCode, Configuration configuration)
        {
            string result = string.Empty;
            string figureName = string.Empty;

            if (langCode.Contains("de"))
            {
                result = FigureId.FigureIdToDeName[figureId];
            }
            if (langCode.Contains("en"))
            {
                result = FigureId.FigureIdToEnName[figureId];
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
            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "von";
            }
            if (langCode.Contains("en"))
            {
                result = "from";
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
            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "nach";
            }
            if (langCode.Contains("en"))
            {
                result = "to";
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
         
            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Schach";
            }
            if (langCode.Contains("en"))
            {
                result = "check";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Spiel beendet";
            }
            if (langCode.Contains("en"))
            {
                result = "game finished";
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
            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Schach Matt";
            }
            if (langCode.Contains("en"))
            {
                result = "check mate";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Remis";
            }
            if (langCode.Contains("en"))
            {
                result = "draw";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "Neues Spiel";
            }
            if (langCode.Contains("en"))
            {
                result = "new game";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "schlägt";
            }
            if (langCode.Contains("en"))
            {
                result = "takes";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "kurze Rochade";
            }
            if (langCode.Contains("en"))
            {
                result = "castling king's side";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "lange Rochade";
            }
            if (langCode.Contains("en"))
            {
                result = "castling queen's side";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gegen";
            }
            if (langCode.Contains("en"))
            {
                result = "against";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gewinnt durch Matt";
            }
            if (langCode.Contains("en"))
            {
                result = "wins by mate";
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

            string result = string.Empty;
            string resultConfig = string.Empty;

            if (langCode.Contains("de"))
            {
                result = "gewinnt durch Bewertung";
            }
            if (langCode.Contains("en"))
            {
                result = "wins by score";
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
    }
}
