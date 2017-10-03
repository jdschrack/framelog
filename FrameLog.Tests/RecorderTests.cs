using System;
using System.Linq;
using FrameLog.Contexts;
using FrameLog.Example.Models;
using FrameLog.Logging;
using FrameLog.Translation;
using Moq;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class RecorderTests
    {
        private Recorder<ChangeSet, User> recorder;
        private User author;
        private DateTime now;

        [SetUp]
        public void CreateRecorder()
        {
            recorder = new Recorder<ChangeSet, User>(new ChangeSetFactory());
            author = new User() { Name = "The author" };
            now = DateTime.Now;
        }

        [Test]
        public void RecorderWithNoSerializationManagerWillDefaultToStringSerialization()
        {
            // Arrange...
            var value = new byte[] { 0x01, 0x02, 0x03 };
            var a = new TestClass() { Id = 1 };
            
            // Act...
            recorder.Record(a, () => a.Id.ToString(), "Property", () => value);
            var set = recorder.Bake(now, author);
            
            // Assert...
            Assert.AreEqual(1, set.ObjectChanges.Count());
            var objectChange = set.ObjectChanges.Single();
            Assert.AreEqual(1, objectChange.PropertyChanges.Count());
            var propertyChange = objectChange.PropertyChanges.Single();
            Assert.AreEqual(value.ToString(), propertyChange.Value);
        }

        [Test]
        public void RecorderWithSerializationManagerWillUseCustomSerializers()
        {
            // Arrange...
            var db = new Mock<IHistoryContext>();
            var serializationManager = new ValueTranslationManager(db.Object);
            var recorder = new Recorder<ChangeSet, User>(new ChangeSetFactory(), serializationManager);
            var value = new byte[] { 0x01, 0x02, 0x03 };
            var a = new TestClass() { Id = 1 };

            // Act...
            recorder.Record(a, () => a.Id.ToString(), "Property", () => value);
            var set = recorder.Bake(now, author);

            // Assert...
            Assert.AreEqual(1, set.ObjectChanges.Count());
            var objectChange = set.ObjectChanges.Single();
            Assert.AreEqual(1, objectChange.PropertyChanges.Count());
            var propertyChange = objectChange.PropertyChanges.Single();
            Assert.AreNotEqual(value.ToString(), propertyChange.Value);
        }        

        [Test]
        public void RecordAddsToObjectChanges()
        {
            var a = new TestClass() { Id = 1 };

            recorder.Record(a, () => a.Id.ToString(), "Property", () => 2);
            var set = recorder.Bake(now, author);
            Assert.AreEqual(1, set.ObjectChanges.Count());

            var change = set.ObjectChanges.Single();
            Assert.AreEqual(set, change.ChangeSet);
            Assert.AreEqual(a.Id, int.Parse(change.ObjectReference));

            var propertyChange = change.PropertyChanges.Single();
            Assert.AreEqual(change, propertyChange.ObjectChange);
            Assert.AreEqual("2", propertyChange.Value);
            Assert.AreEqual(2, propertyChange.ValueAsInt);
        }        

        [Test]
        public void MultipleRecordsWithDifferentObjectsResultInMultipleObjectChanges()
        {
            var a = new TestClass() { Id = 1 };
            var b = new TestClass() { Id = 2 };

            recorder.Record(a, () => a.Id.ToString(), "Property", () => 2);
            recorder.Record(b, () => b.Id.ToString(), "Property", () => 2);
            var set = recorder.Bake(now, author);
            Assert.AreEqual(2, set.ObjectChanges.Count());
        }

        [Test]
        public void MultipleRecordsWithSameObjectResultInSingleObjectChangeWithMultiplePropertyChanges()
        {
            var a = new TestClass() { Id = 1 };

            recorder.Record(a, () => a.Id.ToString(), "Property", () => 2);
            recorder.Record(a, () => a.Id.ToString(), "Name", () => "y");
            var set = recorder.Bake(now, author);
            Assert.AreEqual(1, set.ObjectChanges.Count());

            var change = set.ObjectChanges.Single();
            Assert.AreEqual(2, change.PropertyChanges.Count());
        }

        [Test]
        public void BakeSetsAuthor()
        {
            var a = new TestClass() { Id = 1 };
            recorder.Record(a, () => a.Id.ToString(), "Property", () => 1);
            var set = recorder.Bake(now, author);
            Assert.AreEqual(author, set.Author);
        }
        [Test]
        public void BakeSetsTimestamp()
        {
            var a = new TestClass() { Id = 1 };
            recorder.Record(a, () => a.Id.ToString(), "Property", () => 1);
            var set = recorder.Bake(now, author);
            Assert.AreEqual(now, set.Timestamp);
        }

        private class TestClass
        {
            public int Property { get; set; }
            public string Name { get; set; }
            public int Id { get; set; }
        }
    }
}
