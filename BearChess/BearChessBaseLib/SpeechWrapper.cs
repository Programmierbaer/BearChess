using System.Collections.Generic;

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
}
