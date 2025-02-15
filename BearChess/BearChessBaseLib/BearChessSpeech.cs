using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;

namespace www.SoLaNoSoft.com.BearChessBase
{
    public class BearChessSpeech : ISpeech
    {
        private static ISpeech _speech;

        private readonly ConcurrentQueue<string> _allTextToSpeech = new ConcurrentQueue<string>();

        private readonly ISpeech _synthesizer;

        private volatile bool _pauseSpeak;
        private volatile string _lastSpeak = string.Empty;

        public static ISpeech Instance => _speech ?? (_speech = new BearChessSpeech());

        
        private BearChessSpeech()
        {
            _pauseSpeak = false;
            var speakThread = new Thread(SpeechThread) { IsBackground = true };
            try
            {
                _synthesizer = new SpeechWrapper();
                SpeechAvailable = _synthesizer.SpeechAvailable;
                speakThread.Start();
            }
            catch
            {
                _synthesizer = null;
                SpeechAvailable = false;
            }
        }

        private void SpeechThread()
        {
            
            while (true)
            {
                if (_allTextToSpeech.TryDequeue(out string text))
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (!_pauseSpeak)
                        {
                            if (_allTextToSpeech.Count < 11)
                            {
                                var languageTag = Configuration.Instance.GetConfigValue("selectedSpeechCulture",
                                    CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
                                if (languageTag.Contains("de"))
                                {
                                    text = text.Replace("BearChess", "BärChess");
                                }

                                _synthesizer.Speak(text);
                            }
                        }
                    }
                }
                Thread.Sleep(20);
            }
        }

        public void Speak(string text)
        {
            _lastSpeak = text;
            _allTextToSpeech.Enqueue(text);
        }

        public bool SpeechAvailable { get; }

        public void SelectVoice(string voice)
        {
            _synthesizer?.SelectVoice(voice);
        }

        public ReadOnlyCollection<IInstalledVoice> GetInstalledVoices()
        {
            var installedVoices = new List<IInstalledVoice>();
            if (!SpeechAvailable)
            {
                return new ReadOnlyCollection<IInstalledVoice>(installedVoices);
            }

            foreach (var installedVoice in _synthesizer.GetInstalledVoices())
            {
                installedVoices.Add(new InstalledVoice(new VoiceInfo(installedVoice.VoiceInfo.Age.ToString()
                        , installedVoice.VoiceInfo.Description,
                        installedVoice.VoiceInfo.Gender.ToString(),
                        installedVoice.VoiceInfo.Name, installedVoice.VoiceInfo.Culture),
                    installedVoice.Enabled));
            }

            return new ReadOnlyCollection<IInstalledVoice>(installedVoices);
        }

        public void Repeat()
        {
            _synthesizer?.Repeat();
        }

        public void RepeatAsync()
        {
            _synthesizer?.RepeatAsync();
        }

        public void Clear()
        {
            while (_allTextToSpeech.TryDequeue(out _))
            {
                ;
            }

            _lastSpeak = string.Empty;
        }

        public void SpeakForce(string text)
        {
            _pauseSpeak = true;
            Clear();
            var languageTag = Configuration.Instance.GetConfigValue("selectedSpeechCulture", CultureInfo.CurrentCulture.IetfLanguageTag).ToLower();
            if (languageTag.Contains("de"))
            {
                text = text.Replace("BearChess", "BärChess");
            }
            _synthesizer?.SpeakAsync(text);
            //Clear();
            _pauseSpeak = false;
        }

        public void SetOutputToDefaultAudioDevice()
        {
            _synthesizer?.SetOutputToDefaultAudioDevice();
        }

        public void SpeakAsync(string text, bool force=false)
        {
            if (string.IsNullOrWhiteSpace(_lastSpeak))
            {
                _lastSpeak = string.Empty;
            }
            if (force || (!_pauseSpeak && !_lastSpeak.Equals(text)))
            {
                _lastSpeak = text;
                _allTextToSpeech.Enqueue(text);
            }
        }
    }
}