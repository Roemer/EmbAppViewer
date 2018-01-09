﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EmbAppViewer.Core
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> _backingFieldValues = new Dictionary<string, object>();

        /// <summary>
        /// Gets a property value from the internal backing field
        /// </summary>
        protected T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            if (_backingFieldValues.TryGetValue(propertyName, out var value))
                return (T) value;
            return default(T);
        }

        /// <summary>
        /// Saves a property value to the internal backing field
        /// </summary>
        protected bool SetValue<T>(T newValue, [CallerMemberName] string propertyName = "")
        {
            var oldValue = GetValue<T>(propertyName);
            if (IsEqual(oldValue, newValue)) return false;
            _backingFieldValues[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets a property value to the backing field
        /// </summary>
        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (IsEqual(field, newValue)) return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(GetNameFromExpression(selectorExpression)));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsEqual<T>(T field, T newValue)
        {
            return Equals(field, newValue);
        }

        private string GetNameFromExpression<T>(Expression<Func<T>> selectorExpression)
        {
            var body = (MemberExpression)selectorExpression.Body;
            var propertyName = body.Member.Name;
            return propertyName;
        }
    }
}
