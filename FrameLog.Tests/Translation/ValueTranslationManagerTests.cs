using System;
using System.Collections.Generic;
using System.Globalization;
using FrameLog.Contexts;
using FrameLog.Example.Models;
using FrameLog.Translation;
using FrameLog.Translation.Binders;
using FrameLog.Translation.Serializers;
using Moq;
using NUnit.Framework;

namespace FrameLog.Tests.Translation
{
    public class ValueTranslationManagerTests
    {
        private IBindManager binder;
        private ISerializationManager serializer;

        [SetUp]
        public void CreateManager()
        {
            var db = new Mock<IHistoryContext>();
            var manager = new ValueTranslationManager(db.Object);
            binder = manager;
            serializer = manager;
        }

        [Test]
        public void CanTranslateInteger()
        {
            check(5);
        }
        [Test]
        public void CanTranslateFloat()
        {
            check(5.6f);
        }
        [Test]
        public void CanTranslateDouble()
        {
            check(5.6);
        }
        [Test]
        public void CanTranslateDecimal()
        {
            check(5.6M);
        }
        [Test]
        public void CanTranslateString()
        {
            check("Foo");
            check((string)null);
        }
        [Test]
        public void CanTranslateBoolean()
        {
            check(true);
        }
        [Test]
        public void CanTranslateDateTime()
        {
            check(new DateTime(2014, 03, 26, 12, 50, 36));
        }
        [Test]
        public void CanTranslateLongDateTime()
        {
            var dateTime = new DateTime(2013, 03, 03, 10, 12, 56);
            check(dateTime, serialized: dateTime.Ticks.ToString(CultureInfo.InvariantCulture));
        }
        [Test]
        public void CanTranslateStringDateTime()
        {
            var dateTime = new DateTime(2012, 01, 23, 04, 59, 13);
            check(dateTime, serialized: dateTime.ToString(CultureInfo.InvariantCulture));
        }
        [Test]
        public void CanTranslateDateTimeOffset()
        {
            check(new DateTimeOffset(2013, 08, 14, 15, 49, 01, TimeSpan.FromMinutes(10)));
        }
        [Test]
        public void CanTranslateLongDateTimeOffset()
        {
            var dateTimeOffset = new DateTimeOffset(2015, 10, 22, 12, 59, 11, TimeSpan.FromMinutes(20));
            var dateTicks = dateTimeOffset.Ticks.ToString(CultureInfo.InvariantCulture);
            var offsetTicks = dateTimeOffset.Offset.Ticks.ToString(CultureInfo.InvariantCulture);
            check(dateTimeOffset, serialized: String.Format("{0}+{1}", dateTicks, offsetTicks));
        }
        [Test]
        public void CanTranslateStringDateTimeOffset()
        {
            var dateTimeOffset = new DateTimeOffset(2012, 03, 19, 02, 34, 50, TimeSpan.FromMinutes(30));
            check(dateTimeOffset, serialized: dateTimeOffset.ToString(CultureInfo.InvariantCulture));
        }
        [Test]
        public void CanTranslateTimeSpan()
        {
            check(new TimeSpan(1, 2, 3, 4, 5));
        }
        [Test]
        public void CanTranslateLongTimeSpan()
        {
            var timeSpan = new TimeSpan(3, 2, 1);
            check(timeSpan, serialized: timeSpan.Ticks.ToString(CultureInfo.InvariantCulture));
        }
        [Test]
        public void CanTranslateStringTimeSpan()
        {
            var timeSpan = new TimeSpan(3, 2, 1);
            check(timeSpan, serialized: timeSpan.ToString());
        }
        [Test]
        public void CanTranslateGuid()
        {
            check(new Guid());
        }
        [Test]
        public void CanTranslateCollection()
        {
            check(new List<int>() { 1, 2, 3 });
        }
        [Test]
        public void CanTranslateMixedDelimiterCollection()
        {
            check(new List<int>() { 3, 2, 1 }, serialized: " 3, 2,1 ");
        }
        [Test]
        public void CanTranslateCollectionInterface()
        {
            check((ICollection<int>)new List<int>() { 1, 2, 3 });
        }
        [Test]
        public void CanTranslateMixedDelimiterCollectionInterface()
        {
            check((ICollection<int>)new List<int>() { 3, 2, 1 }, serialized: " 3, 2,1 ");
        }
        [Test]
        public void CanTranslateEnum()
        {
            check(BookType.Paperback);
        }
        [Test]
        public void CanTranslateFlaggedEnum()
        {
            check(BookGenre.Drama | BookGenre.Comedy);
        }
        [Test]
        public void CanTranslateBinaryBlob()
        {
            check(new byte[] { 0x01, 0x02, 0x03 });
        }
        [Test]
        public void CanTranslateNullBinaryBlob()
        {
            check((byte[])null);
        }
        [Test]
        public void CanTranslateNullableInt()
        {
            check((int?)5);
            check((int?)null);
        }
        [Test]
        public void CanTranslateNullableGuid()
        {
            check((Guid?)new Guid());
            check((Guid?)null);
        }
        [Test]
        public void CanTranslateNullableDateTime()
        {
            check((DateTime?)DateTime.Now);
            check((DateTime?)null);
        }
        [Test]
        public void CanTranslateNullableDateTimeOffset()
        {
            check((DateTimeOffset?)DateTimeOffset.Now);
            check((DateTimeOffset?)null);
        }
        [Test]
        public void CanTranslateNullableTimeSpan()
        {
            check((TimeSpan?)TimeSpan.FromDays(3));
            check((TimeSpan?)null);
        }
        [Test]
        public void CanTranslateNullableEnum()
        {
            check((BookType?)BookType.Paperback);
            check((BookType?)null);
        }
        [Test]
        public void CanTranslateNullableFlaggedEnum()
        {
            check((BookGenre?)BookGenre.Drama | BookGenre.Comedy);
            check((BookGenre?)null);
        }

        private void check<T>(T value, string serialized = null)
        {
            serialized = (serialized ?? serializer.Serialize(value));
            Assert.AreEqual(value, binder.Bind<T>(serialized));
        }
    }
}
