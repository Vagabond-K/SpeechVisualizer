using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeechVisualizer
{
    public class AudioData : INotifyPropertyChanged
    {
        private bool isActivated = false;
        private double volume = 0;
        private IReadOnlyList<AudioData> bands;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActivated { get => isActivated; private set => SetProperty(ref isActivated, value); }
        public double Volume
        {
            get => volume;
            internal set
            {
                if (SetProperty(ref volume, value))
                    IsActivated = volume > 0.005;
            }
        }

        public IReadOnlyList<AudioData> Bands { get => bands; internal set => SetProperty(ref bands, value); }

        private bool SetProperty<TProperty>(ref TProperty target, in TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<TProperty>.Default.Equals(target, value))
            {
                target = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}
