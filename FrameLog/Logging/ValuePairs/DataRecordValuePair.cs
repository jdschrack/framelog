using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FrameLog.Logging.ValuePairs
{
    /// <summary>
    /// A special kind of ValuePair where the type of the property is IDataRecord.
    /// This is what EntityFramework records when tracking changes to properties of
    /// complex types.
    /// 
    /// This subclass adds functionality for retrieving the changes to the properties
    /// of the complex type as ordinary ValuePairs.
    /// 
    /// Complex types can have further complex types within them, so this class
    /// recursively retrieves nested properties. 
    /// </summary>
    internal class DataRecordValuePair : ValuePair
    {
        internal DataRecordValuePair(IValuePair pair)
            : base(pair.OriginalValue, pair.NewValue, pair.PropertyName, pair.State) { }

        private Func<IDataRecord> originalRecord
        {
            get { return () => ((IDataRecord)OriginalValue()); }
        }
        private Func<IDataRecord> newRecord
        {
            get { return () => ((IDataRecord)NewValue()); }
        }

        internal IEnumerable<IValuePair> SubValuePairs
        {
            get
            {
                foreach (var fieldName in fieldNames)
                {
                    var o = getOrNull(originalRecord, fieldName);
                    var n = getOrNull(newRecord, fieldName);
                    string name = string.Format("{0}.{1}", propertyName, fieldName);
                    foreach (var child in ValuePairSource.Get(o, n, name, state))
                    {
                        yield return child;
                    }
                }
            }
        }

        private Func<object> getOrNull(Func<IDataRecord> record, string fieldName)
        {
            return () =>
            {
                var obj = (record != null ? record() : null);
                if (obj != null)
                    return obj[fieldName];
                else
                    return null;
            };
        }

        private IEnumerable<string> fieldNames
        {
            get
            {
                // All the field names that exist in at least one of the data records
                return fieldNamesFor(originalRecord).Union(fieldNamesFor(newRecord));
            }
        }

        private IEnumerable<string> fieldNamesFor(Func<IDataRecord> record)
        {
            var obj = record();
            if (obj != null)
            {
                foreach (int index in Enumerable.Range(0, obj.FieldCount))
                {
                    yield return obj.GetName(index);
                }
            }
        }
    }
}
