using FrameLog.Logging.ValuePairs;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace FrameLog.Tests
{
    public class ValuePairTests
    {
        [Test]
        public void CanConstructValuePairFromSimpleChange()
        {
            var vps = ValuePairSource.Get(() => "a", () => "b", "property", EntityState.Modified);
            Assert.AreEqual(1, vps.Count());
            var vp = vps.Single();
            check(vp, EntityState.Modified, "a", "b", "property");
        }

        [Test]
        public void CanConstructValuePairsFromComplexChange()
        {
            var vps = ValuePairSource.Get(
                () => record(kv("p1", "a"), kv("p2", "x")),
                () => record(kv("p1", "b"), kv("p2", "y")),
                "property", EntityState.Modified);
            Assert.AreEqual(2, vps.Count());
            var p1 = vps.Single(vp => vp.PropertyName == "property.p1");
            var p2 = vps.Single(vp => vp.PropertyName == "property.p2");
            check(p1, EntityState.Modified, "a", "b", "property.p1");
            check(p2, EntityState.Modified, "x", "y", "property.p2");
        }

        [Test]
        public void CanConstructValuePairsFromNestedComplexChanges()
        {
            var vps = ValuePairSource.Get(
                () => record(kv("child", record(kv("grandchild", "a")))),
                () => record(kv("child", record(kv("grandchild", "b")))),
                "property", EntityState.Modified);
            Assert.AreEqual(1, vps.Count());
            var vp = vps.Single();
            check(vp, EntityState.Modified, "a", "b", "property.child.grandchild");
        }

        private void check(IValuePair actual, EntityState state, object oldValue, object newValue, string propertyName)
        {
            Assert.AreEqual(state, actual.State);
            Assert.AreEqual(oldValue, actual.OriginalValue());
            Assert.AreEqual(newValue, actual.NewValue());
            Assert.AreEqual(propertyName, actual.PropertyName);
        }

        private KeyValuePair<string, object> kv(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        private IDataRecord record(params KeyValuePair<string, object>[] pairs)
        {
            var record = new Mock<IDataRecord>();
            int index = 0;
            foreach (var pair in pairs)
            {
                var valuePair = pair;
                record.Setup(r => r[valuePair.Key]).Returns(valuePair.Value);
                record.Setup(r => r.GetName(index)).Returns(valuePair.Key);
                index++;
            }
            record.Setup(r => r.FieldCount).Returns(pairs.Length);
            return record.Object;
        }
    }
}
