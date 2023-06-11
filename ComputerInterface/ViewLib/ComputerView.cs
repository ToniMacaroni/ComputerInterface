using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ComputerInterface.Interfaces;
using ComputerInterface.Views;
using JetBrains.Annotations;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class ComputerView : IComputerView
    {
        /// <summary>
        /// How many characters fit in the x axis of the screen
        /// </summary>
        public static int SCREEN_WIDTH = 53;

        /// <summary>
        /// How many characters fit in the y axis of the screen
        /// </summary>
        public static int SCREEN_HEIGHT = 13;

        public string PrimaryColor = "ed6540";

        /// <summary>
        /// Text that is shown on screen
        /// assigning to it automatically updates the text
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        protected string _text;

        public Type CallerViewType { get; set; }

        /// <summary>
        /// Set text from a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="str"></param>
        public virtual void SetText(StringBuilder str)
        {
            Text = str.ToString();
        }

        /// <summary>
        /// Set text from a <see cref="StringBuilder"/> the the callback is providing
        /// </summary>
        /// <param name="builderCallback"></param>
        public virtual void SetText(Action<StringBuilder> builderCallback)
        {
            var str = new StringBuilder();
            builderCallback(str);
            SetText(str);
        }

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
        public virtual void OnShow(object[] args)
        {
            RaisePropertyChanged(nameof(Text));
        }

        /// <summary>
        /// Switch to another view
        /// </summary>
        /// <param name="type"></param>
        public void ShowView<T>(params object[] args)
        {
            ShowView(typeof(T), args);
        }

        /// <summary>
        /// Switch to another view
        /// </summary>
        /// <param name="type"></param>
        public void ShowView(Type type, params object[] args)
        {
            OnViewSwitchRequest?.Invoke(new ComputerViewSwitchEventArgs(GetType(), type, args));
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

        public void SetBackground(Texture texture, Color? color = null)
        {
            var args = new ComputerViewChangeBackgroundEventArgs(texture, color);
            OnChangeBackgroundRequest?.Invoke(args);
        }

        public void RevertBackground()
        {
            OnChangeBackgroundRequest?.Invoke(null);
        }

        public async Task ShowSplashForDuration(Texture texture, int ms)
        {
            var text = Text;
            Text = "";
            SetBackground(texture);
            await Task.Delay(ms);
            RevertBackground();
            Text = text;
        }

        public event ComputerViewSwitchEventHandler OnViewSwitchRequest;
        public event ComputerViewChangeBackgroundEventHandler OnChangeBackgroundRequest;

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