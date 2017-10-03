﻿using System;
using System.Data.Entity;

namespace FrameLog.Logging.ValuePairs
{
    internal class ValuePair : IValuePair
    {
        protected readonly Func<object> originalValue;
        protected readonly Func<object> newValue;
        protected readonly string propertyName;
        protected readonly EntityState state;

        internal ValuePair(Func<object> originalValue, Func<object> newValue, string propertyName, EntityState state)
        {
            this.originalValue = checkDbNull(originalValue);
            this.newValue = checkDbNull(newValue);
            this.propertyName = propertyName;
            this.state = state;
        }

        private Func<object> checkDbNull(Func<object> value)
        {
            return () =>
            {
                var obj = (value != null ? value() : null);
                if (obj is DBNull)
                    return null;
                return obj;
            };
        }

        internal IChangeType Type
        {
            get
            {
                var value = originalValue() ?? newValue();
                return value.GetChangeType();
            }
        }

        public bool HasChanged
        {
            get
            {
                return state == EntityState.Added
                    || state == EntityState.Deleted
                    || !object.Equals(newValue(), originalValue());
            }
        }

        public string PropertyName
        {
            get { return propertyName; }
        }

        public Func<object> NewValue
        {
            get { return newValue; }
        }

        public Func<object> OriginalValue
        {
            get { return originalValue; }
        }

        public EntityState State
        {
            get { return state; }
        }
    }
}