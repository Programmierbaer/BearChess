using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations.CTG
{
    [Serializable]
    public class BookInfo : INotifyPropertyChanged
    {
        private bool _isDefaultBook;
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public int PositionsCount { get; set; }
        public int MovesCount { get; set; }
        public int GamesCount { get; set; }

        public bool IsInternalBook { get; set; }
        public bool IsHidddenInternalBook { get; set; }

        public bool IsDefaultBook
        {
            get => _isDefaultBook;
            set
            {
                if (value == _isDefaultBook)
                {
                    return;
                }

                _isDefaultBook = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}