using System;
using System.Globalization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public static class MoveExtentions
    {
        public static bool EqualMove(this Move move, Move move2)
        {
            return move.Identifier.Equals(move2.Identifier);
        }

        public static bool IsCastleMove(this Move move)
        {
            switch (move.FromField)
            {
                case Fields.FE1 when move.ToField.Equals(Fields.FG1):

                    return true;
                case Fields.FE1 when move.ToField.Equals(Fields.FC1):

                    return true;
                case Fields.FE8 when move.ToField.Equals(Fields.FG8):

                    return true;
                case Fields.FE8 when move.ToField.Equals(Fields.FC8):

                    return true;
                default:
                    return false;
            }
        }

        public static void ConfirmMove(Move move, bool speakForce = true)
        {
            var rm = SpeechTranslator.ResourceManager;
            var addInfo = rm.GetString("ConfirmMove");
            SpeakMove(move.Figure, move.CapturedFigure, move.FromFieldName, move.ToFieldName, move.PromotedFigure,
                move.ShortMoveIdentifier, addInfo, speakForce);
        }

        public static void SpeakMove(Move move, bool speakForce = true)
        {
            var rm = SpeechTranslator.ResourceManager;
            var addInfo = move.FigureColor == Fields.COLOR_WHITE
                ? rm.GetString("WhitesMove")
                : rm.GetString("BlacksMove");
            SpeakMove(move.Figure, move.CapturedFigure, move.FromFieldName, move.ToFieldName, move.PromotedFigure,
                move.ShortMoveIdentifier, addInfo, speakForce);
        }

        public static void SpeakMove(int fromFieldFigureId, int toFieldFigureId, string fromFieldFieldName,
            string toFieldFieldName, int promoteFigureId, string shortMoveIdentifier, string additionalInfo,
            bool speakForce = true)
        {
            var config = Configuration.Instance;
            var blindUser = config.GetBoolValue("blindUser", false);
            var speechIsActive = blindUser || config.GetBoolValue("speechActive", true);
            if (!speechIsActive)
            {
                return;
            }
            var speechLongMove = config.GetBoolValue("speechLongMove", true);
            var useBlindNames = config.GetBoolValue("blindUserSayFideRules", true);
            var speechLanguageTag = config
                .GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
            var synthesizer = BearChessSpeech.Instance;
            var isDone = false;
            if (fromFieldFigureId == FigureId.WHITE_KING)
            {
                if (fromFieldFieldName.Equals("E1", StringComparison.OrdinalIgnoreCase))
                {
                    if (toFieldFieldName.Equals("G1", StringComparison.OrdinalIgnoreCase))
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(speechLanguageTag, config)}");
                        }

                        isDone = true;
                    }

                    if (toFieldFieldName.Equals("C1", StringComparison.OrdinalIgnoreCase))
                    {

                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(speechLanguageTag, config)}");
                        }

                        isDone = true;
                    }
                }
            }

            if (fromFieldFigureId == FigureId.BLACK_KING)
            {
                if (fromFieldFieldName.Equals("E8", StringComparison.OrdinalIgnoreCase))
                {
                    if (toFieldFieldName.Equals("G8", StringComparison.OrdinalIgnoreCase))
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetCastleKingsSide(speechLanguageTag, config)}");
                        }

                        isDone = true;
                    }

                    if (toFieldFieldName.Equals("C8", StringComparison.OrdinalIgnoreCase))
                    {

                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetCastleQueensSide(speechLanguageTag, config)}");
                        }

                        isDone = true;
                    }
                }
            }

            if (!isDone)
            {
                if (speechLongMove && !blindUser)
                {
                    if (toFieldFigureId == FigureId.NO_PIECE)
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {SpeechTranslator.GetFrom(speechLanguageTag, config)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetTo(speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {SpeechTranslator.GetFrom(speechLanguageTag, config)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetTo(speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                    }
                    else
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {SpeechTranslator.GetFrom(speechLanguageTag, config)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetCapture(speechLanguageTag, config)} {SpeechTranslator.GetFigureName(toFieldFigureId, speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {SpeechTranslator.GetFrom(speechLanguageTag, config)} {fromFieldFieldName}, " +
                                $"{SpeechTranslator.GetCapture(speechLanguageTag, config)} {SpeechTranslator.GetFigureName(toFieldFigureId, speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                    }
                }
                else
                {
                    if (blindUser)
                    {
                        toFieldFieldName = Fields.GetBlindFieldName(toFieldFieldName);
                        shortMoveIdentifier = Fields.GetBlindFieldName(shortMoveIdentifier);
                    }

                    var figureName = SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config);
                    if (toFieldFigureId == FigureId.NO_PIECE)
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {figureName} {shortMoveIdentifier} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {figureName} {shortMoveIdentifier} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                    }
                    else
                    {
                        if (speakForce)
                        {
                            synthesizer.SpeakForce(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {shortMoveIdentifier} {SpeechTranslator.GetCapture(speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                        else
                        {
                            synthesizer.SpeakAsync(
                                $"{additionalInfo} {SpeechTranslator.GetFigureName(fromFieldFigureId, speechLanguageTag, config)} {shortMoveIdentifier} {SpeechTranslator.GetCapture(speechLanguageTag, config)} {toFieldFieldName} {SpeechTranslator.GetFigureName(promoteFigureId, speechLanguageTag, config)}");
                        }
                    }
                }
            }
        }
    }
}