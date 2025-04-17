using System;
using System.Collections.Generic;
using System.Globalization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public static class ChessBoardExtension
    {
        public static string[] GetSaysForBoard(IChessBoard chessBoard)
        {
            var config = Configuration.Instance;
            var blindUser = config.GetBoolValue("blindUser", false);
            var speechIsActive = blindUser || config.GetBoolValue("speechActive", true);
            if (!speechIsActive)
            {
                return Array.Empty<string>();
            }
            var allSays = new List<string>();
            var speechLanguageTag = config
                .GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
            for (var i = 1; i < 9; i++)
            {
                var chessFigures = chessBoard.GetFiguresOnRow(i);
                foreach (var chessFigure in chessFigures)
                {
                    if (chessFigure.FigureId == FigureId.NO_PIECE)
                    {
                        continue;
                    }
                    var figureName = SpeechTranslator.GetFigureName(chessFigure.FigureId, speechLanguageTag, config);
                    var fieldName = Fields.GetBlindFieldName(Fields.GetFieldName(chessFigure.Field));
                    allSays.Add($"{figureName} {fieldName}");
                }
            }

            return allSays.ToArray();
        }

        public static void SayBoard(IChessBoard chessBoard)
        {
            var allSays = GetSaysForBoard(chessBoard);
            if (allSays.Length > 0)
            {
                var synthesizer = BearChessSpeech.Instance;
                foreach (var allSay in allSays)
                {
                    synthesizer?.SpeakAsync(allSay);
                }
            }
        }
    }
}