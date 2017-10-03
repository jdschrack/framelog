using System;
using System.Collections.Generic;
using FrameLog.Contexts;
using FrameLog.Translation;
using FrameLog.Translation.Binders;
using FrameLog.Translation.Serializers;
using Moq;
using NUnit.Framework;

namespace FrameLog.Tests.Translation
{
    public class LegacyValueTranslationTests
    {
        private IBindManager binder;
        private ISerializationManager serializer;

        [SetUp]
        public void CreateManager()
        {
            var db = new Mock<IHistoryContext>();
            binder = new LegacyBindManager(db.Object);
            serializer = new LegacySerializationManager();
        }

        [Test]
        public void CanBindInteger()
        {
            check(5);
        }
        [Test]
        public void CanBindFloat()
        {
            check(5.6f);
        }
        [Test]
        public void CanBindDouble()
        {
            check(5.6);
        }
        [Test]
        public void CanBindDecimal()
        {
            check(5.6M);
        }
        [Test]
        public void CanBindString()
        {
            check("Foo");
            check((string)null);
        }
        [Test]
        public void CanBindBoolean()
        {
            check(true);
        }
        [Test]
        public void CanBindDateTime()
        {
            check(new DateTime(2014, 03, 26, 12, 50, 36));
        }
        [Test]
        public void CanBindGuid()
        {
            check(new Guid());
        }
        [Test]
        public void CanBindCollection()
        {
            check(new List<int>() { 1, 2, 3 }, serialized: "1, 2,3");
        }
        [Test]
        public void CanBindCollectionInterface()
        {
            check((ICollection<int>)new List<int>() { 1, 2, 3 }, serialized: "1, 2,3");
        }
        [Test]
        public void CanBindNullableInt()
        {
            check((int?)5);
            check((int?)null);
        }
        [Test]
        public void CanBindNullableGuid()
        {
            check((Guid?)new Guid());
            check((Guid?)null);
        }

        private void check<T>(T value, string serialized = null)
        {
            serialized = (serialized ?? serializer.Serialize(value));
            Assert.AreEqual(value, binder.Bind<T>(serialized));
        }
    }
}
