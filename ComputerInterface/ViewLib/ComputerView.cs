using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ComputerInterface.Interfaces;
using ComputerInterface.Views;
using JetBrains.Annotations;

namespace ComputerInterface.ViewLib
{
    public class ComputerView : IComputerView
    {
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        protected string _text;

        public Type CallerViewType { get; set; }

        /// <summary>
        /// Gets called when a key is pressed on the keyboard
        /// </summary>
        /// <param name="key"></param>
        public virtual void OnKeyPressed(EKeyboardKey key)
        {
        }

        /// <summary>
        /// Gets called when the view is shown
        /// call the base OnShow when overriding
        /// to display the current text on the computer
        /// </summary>
        public virtual void OnShow()
        {
            RaisePropertyChanged(nameof(Text));
        }

        /// <summary>
        /// Switch to another view
        /// </summary>
        /// <param name="type"></param>
        public void ShowView<T>()
        {
            ShowView(typeof(T));
        }

        /// <summary>
        /// Switch to another view
        /// </summary>
        /// <param name="type"></param>
        public void ShowView(Type type)
        {
            OnViewSwitchRequest?.Invoke(new ComputerViewSwitchEventArgs(GetType(), type));
        }

        /// <summary>
        /// Return to previous view
        /// </summary>
        public void ReturnView()
        {
            ShowView(CallerViewType);
        }

        /// <summary>
        /// Shows the main menu view
        /// </summary>
        public void ReturnToMainMenu()
        {
            ShowView<MainMenuView>();
        }

        public event ComputerViewSwitchEventHandler OnViewSwitchRequest;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}