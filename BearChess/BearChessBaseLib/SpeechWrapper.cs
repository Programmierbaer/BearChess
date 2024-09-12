using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace www.SoLaNoSoft.com.BearChessBase
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Speech.Synthesis;

    public interface IVoiceInfo
    {
        string Age { get; }
        string Description { get; }

        string Gender { get; }

        string Name { get; }

        CultureInfo Culture { get; }
    }

    public class VoiceInfo : IVoiceInfo
    {
        public string Age { get; }
        public string Description { get; }
        public string Gender { get; }
        public string Name { get; }
        public CultureInfo Culture { get; }

        public VoiceInfo(string age, string description, string gender, string name, CultureInfo culture)
        {
            Age = age;
            Description = description;
            Gender = gender;
            Name = name;
            Culture = culture;
        }
    }

    public interface IInstalledVoice
    {
        bool Enabled { get; }
        IVoiceInfo VoiceInfo { get; }
    }

    public class InstalledVoice : IInstalledVoice
    {

        public bool Enabled { get; }
        public IVoiceInfo VoiceInfo { get; }

        public InstalledVoice(IVoiceInfo voiceInfo, bool enabled)
        {
            VoiceInfo = voiceInfo;
            Enabled = enabled;
        }

    }

    public interface ISpeech
    {
        bool SpeechAvailable { get; }
        void SetOutputToDefaultAudioDevice();
        void SpeakAsync(string text, bool force = false);
        void Speak(string text);
        void SelectVoice(string voice);
        ReadOnlyCollection<IInstalledVoice> GetInstalledVoices();

        void Repeat();
        void RepeatAsync();

        void Clear();
        void SpeakForce(string text);
    }



    public class SpeechWrapper : ISpeech
    {
        private readonly SpeechSynthesizer _synthesizer;
        private string _lastSpeak;

        public SpeechWrapper()
        {
            _lastSpeak = string.Empty;
            try
            {
                _synthesizer = new SpeechSynthesizer();
                SpeechAvailable = true;
            }
            catch
            {
                _synthesizer = null;
                SpeechAvailable = false;
            }
        }

        public void Speak(string text)
        {
            _lastSpeak = text;
            _synthesizer?.Speak(text);
        }

        public bool SpeechAvailable { get; }

        public void SelectVoice(string voice)
        {
            _synthesizer?.SelectVoice(voice);
        }

        public ReadOnlyCollection<IInstalledVoice> GetInstalledVoices()
        {
            var installedVoices = new List<IInstalledVoice>();
            if (SpeechAvailable)
            {
                foreach (var installedVoice in _synthesizer.GetInstalledVoices())
                {
                    installedVoices.Add(new InstalledVoice(new VoiceInfo(installedVoice.VoiceInfo.Age.ToString()
                                                                         , installedVoice.VoiceInfo.Description,
                                                                         installedVoice.VoiceInfo.Gender.ToString(),
                                                                         installedVoice.VoiceInfo.Name, installedVoice.VoiceInfo.Culture),
                                                           installedVoice.Enabled));
                }
            }

            return new ReadOnlyCollection<IInstalledVoice>(installedVoices);
        }

        public void Repeat()
        {
            _synthesizer?.Speak(_lastSpeak);
        }

        public void RepeatAsync()
        {
            _synthesizer?.SpeakAsync(_lastSpeak);
        }

        public void Clear()
        {
            //
        }

        public void SpeakForce(string text)
        {
            //
        }

        public void SetOutputToDefaultAudioDevice()
        {
            _synthesizer?.SetOutputToDefaultAudioDevice();
        }

        public void SpeakAsync(string text, bool force)
        {
            _lastSpeak = text;
            _synthesizer?.SpeakAsync(text);
        }
    }

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
            Thread speakThread = new Thread(SpeechThread) { IsBackground = true };
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
                    if (!_pauseSpeak )
                    {
                        if (_allTextToSpeech.Count < 11)
                        {
                            _synthesizer.Speak(text);
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
            if (SpeechAvailable)
            {
                foreach (var installedVoice in _synthesizer.GetInstalledVoices())
                {
                    installedVoices.Add(new InstalledVoice(new VoiceInfo(installedVoice.VoiceInfo.Age.ToString()
                                                                         , installedVoice.VoiceInfo.Description,
                                                                         installedVoice.VoiceInfo.Gender.ToString(),
                                                                         installedVoice.VoiceInfo.Name, installedVoice.VoiceInfo.Culture),
                                                           installedVoice.Enabled));
                }
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
            while (_allTextToSpeech.TryDequeue(out _));
            _lastSpeak = string.Empty;
        }

        public void SpeakForce(string text)
        {
            _pauseSpeak = true;
            Clear();
            _synthesizer?.Speak(text);
            Clear();
            _pauseSpeak = false;
        }

        public void SetOutputToDefaultAudioDevice()
        {
            _synthesizer?.SetOutputToDefaultAudioDevice();
        }

        public void SpeakAsync(string text, bool force=false)
        {
            if (force || !_lastSpeak.Equals(text))
            {
                _lastSpeak = text;
                _allTextToSpeech.Enqueue(text);
            }
        }
    }
}
