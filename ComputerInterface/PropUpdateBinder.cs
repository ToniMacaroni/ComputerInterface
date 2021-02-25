using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ComputerInterface
{
    internal class PropUpdateBinder
    {
        private readonly Dictionary<string, Action> _actions = new Dictionary<string, Action>();

        public void Bind(string name, Action callback)
        {
            _actions.Add(name, callback);
        }

        public void Clear()
        {
            _actions.Clear();
        }

        public void PropertyChanged(object src, PropertyChangedEventArgs args)
        {
            if (_actions.TryGetValue(args.PropertyName, out var action))
            {
                action.Invoke();
            }
        }
    }
}