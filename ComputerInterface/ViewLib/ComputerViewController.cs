using System;
using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    internal class ComputerViewController
    {
        public event Action<string> OnTextChanged;
        public event Action<ComputerViewSwitchEventArgs> OnSwitchView;
        public event Action<ComputerViewChangeBackgroundEventArgs> OnSetBackground; 

        public IComputerView CurrentComputerView { get; private set; }

        private readonly PropUpdateBinder _propUpdateBinder;

        public ComputerViewController()
        {
            _propUpdateBinder = new PropUpdateBinder();
            _propUpdateBinder.Bind("Text", RaiseOnTextChanged);
        }

        public void SetView(IComputerView computerView, object[] args)
        {
            UnregisterView(CurrentComputerView);
            RegisterView(computerView);

            CurrentComputerView = computerView;
            CurrentComputerView.OnShow(args);
        }

        public void NotifyOfKeyPress(EKeyboardKey key)
        {
            CurrentComputerView?.OnKeyPressed(key);
        }

        private void RegisterView(IComputerView view)
        {
            if (view == null)
            {
                return;
            }

            view.PropertyChanged += _propUpdateBinder.PropertyChanged;
            view.OnViewSwitchRequest += OnSwitchViewRequest;
            view.OnChangeBackgroundRequest += OnChangeBackgroundRequest;
        }

        private void UnregisterView(IComputerView view)
        {
            if (view == null)
            {
                return;
            }

            view.PropertyChanged -= _propUpdateBinder.PropertyChanged;
            view.OnViewSwitchRequest -= OnSwitchViewRequest;
            view.OnChangeBackgroundRequest -= OnChangeBackgroundRequest;

            OnChangeBackgroundRequest(null);
        }

        private void OnSwitchViewRequest(ComputerViewSwitchEventArgs args)
        {
            OnSwitchView?.Invoke(args);
        }

        private void OnChangeBackgroundRequest(ComputerViewChangeBackgroundEventArgs args)
        {
            OnSetBackground?.Invoke(args);
        }

        private void RaiseOnTextChanged()
        {
            if (CurrentComputerView == null)
            {
                return;
            }

            OnTextChanged?.Invoke(CurrentComputerView.Text);
        }
    }
}