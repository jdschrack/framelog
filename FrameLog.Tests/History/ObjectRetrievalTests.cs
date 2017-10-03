using FrameLog.Example.Models;
using FrameLog.Example.Models.Testing;
using FrameLog.Exceptions;
using FrameLog.History;
using NUnit.Framework;
using System.Linq;

namespace FrameLog.Tests.History
{
    public class ObjectRetrievalTests : HistoryExplorerTests
    {
        [Test]
        public void CanRetrieveObjectChangeDefault()
        {
            var book = makeBook();
            book.Title = "New title";
            save();

            var expected = lastChangeSet().ObjectChanges.Single();
            var actual = explorer.ChangesTo(book).First().ObjectChange;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanRetrieveObjectChangeUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveObjectChangeDefault();
        }

        [Test]
        public void CanRetrieveObjectChangeDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveObjectChangeDefault();
        }

        [Test]
        public void CanRetrieveChangesToAWholeObjectDefault()
        {
            var entity = makeBook();
            var book1 = makeBook();
            var book2 = makeBook();

            entity.Title = "Foo";
            entity.NumberOfFans = 0;
            entity.Sequel = book1;
            entity.Style.Hardcover = true;
            entity.Style.Format.Name = "Large print";
            var a = entity.Copy();
            save();

            entity.Title = "Bar";
            entity.NumberOfFans = 1;
            entity.Sequel = book2;
            entity.Style.Hardcover = false;
            entity.Style.Format.Name = null;
            var b = entity.Copy();
            save();

            var result = explorer.ChangesTo(entity);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(b, user, change1, (x, y) => compare(x, y));
            check(a, user, change2, (x, y) => compare(x, y));
        }

        [Test]
        public void CanRetrieveChangesToAWholeObjectUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAWholeObjectDefault();
        }

        [Test]
        public void CanRetrieveChangesToAWholeObjectDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAWholeObjectDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAListDefault()
        {
            var obj = db.ClassesWithList.Add(new ClassWithList());
            var a = makeUser();
            var b = makeUser();
            save();

            obj.Users.Add(a);
            save();
            obj.Users.Add(b);
            save();

            var result = explorer.ChangesTo(obj).ToList();
            var change1 = result[0];
            var change2 = result[1];

            check(ClassWithList.New(new User[] { a, b }), user, change1);
            check(ClassWithList.New(new User[] { a }), user, change2);
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAListUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAnObjectWithAListDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAListDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAnObjectWithAListDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAHashSetDefault()
        {
            var obj = db.ClassesWithHashSet.Add(new ClassWithHashSet());
            var a = makeUser();
            var b = makeUser();
            save();

            obj.Users.Add(a);
            save();
            obj.Users.Add(b);
            save();

            var result = explorer.ChangesTo(obj).ToList();
            var change1 = result[0];
            var change2 = result[1];

            check(ClassWithHashSet.New(new User[] { a, b }), user, change1);
            check(ClassWithHashSet.New(new User[] { a }), user, change2);
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAHashSetUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAnObjectWithAHashSetDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAHashSetDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAnObjectWithAHashSetDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithACollectionDefault()
        {
            var obj = db.ClassesWithCollection.Add(new ClassWithCollection());
            var a = makeUser();
            var b = makeUser();
            save();

            obj.Users.Add(a);
            save();
            obj.Users.Add(b);
            save();

            var result = explorer.ChangesTo(obj).ToList();
            var change1 = result[0];
            var change2 = result[1];

            check(ClassWithCollection.New(new User[] { a, b }), user, change1);
            check(ClassWithCollection.New(new User[] { a }), user, change2);
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithACollectionUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAnObjectWithACollectionDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithACollectionDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAnObjectWithACollectionDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAnEntityCollectionDefault()
        {
            var obj = db.ClassesWithEntityCollection.Add(new ClassWithEntityCollection());
            var a = makeUser();
            var b = makeUser();
            save();

            obj.Users.Add(a);
            save();
            obj.Users.Add(b);
            save();

            var result = explorer.ChangesTo(obj).ToList();
            var change1 = result[0];
            var change2 = result[1];

            check(ClassWithEntityCollection.New(new User[] { a, b }), user, change1);
            check(ClassWithEntityCollection.New(new User[] { a }), user, change2);
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAnEntityCollectionUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAnObjectWithAnEntityCollectionDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectWithAnEntityCollectionDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAnObjectWithAnEntityCollectionDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectByReferenceDefault()
        {
            var entity = makeBook();
            entity.Title = "Foo";
            save();

            var expected = explorer.ChangesTo(entity).First();
            var actual = explorer.ChangesTo<Book>(entity.Id.ToString()).First();
            Assert.AreEqual(expected.Author, actual.Author);
            Assert.AreEqual(expected.Timestamp, actual.Timestamp);
            Assert.AreEqual(expected.Value.Title, actual.Value.Title);
        }

        [Test]
        public void CanRetrieveChangesToAnObjectByReferenceUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToAnObjectByReferenceDefault();
        }

        [Test]
        public void CanRetrieveChangesToAnObjectByReferenceDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToAnObjectByReferenceDefault();
        }

        [Test]
        public void CanRetrieveChangesToADeletedObjectDefault()
        {
            var entity = makeBook();
            db.Books.Remove(entity);
            save();

            var changes = explorer.ChangesTo(entity).ToList();
            Assert.IsNull(changes[0].Value);
            Assert.IsNotNull(changes[1].Value);
        }

        [Test]
        public void CanRetrieveChangesToADeletedObjectUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToADeletedObjectDefault();
        }

        [Test]
        public void CanRetrieveChangesToADeletedObjectDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToADeletedObjectDefault();
        }

        [Test]
        public void CanRetrieveObjectCreationInformation()
        {
            var book = makeBook();
            save();
            book.Title = "Foo";
            save();

            var result = explorer.ChangesTo(book);
            // The "first" change is the most recent one (setting the title) whereas the 
            // next one is the original creation of the book. We expect the GetCreation
            // method to return the same information as this object.
            var change = result.Skip(1).First();

            var creation = explorer.GetCreation(book);
            Assert.AreEqual(book, creation.Value);
            Assert.AreEqual(change.Author, creation.Author);
            Assert.AreEqual(change.Timestamp, creation.Timestamp);
        }

        [Test]
        public void CanRetrieveTypeOfChange()
        {
            var book = makeBook();
            book.Title = "New title"; save();
            db.Books.Remove(book); save();

            var changes = db.ChangeSets
                .OrderByDescending(c => c.Timestamp)
                .Take(3).AsEnumerable()
                .OrderBy(c => c.Timestamp)
                .Select(c => c.ObjectChanges.Single())
                .ToList();
            Assert.AreEqual(ChangeType.Add, explorer.TypeOfChange<Book>(changes[0]));
            Assert.AreEqual(ChangeType.Modify, explorer.TypeOfChange<Book>(changes[1]));
            Assert.AreEqual(ChangeType.Delete, explorer.TypeOfChange<Book>(changes[2]));
        }

        [Test]
        public void IfObjectCreationInformationIsNotAvailableExceptionIsThrown()
        {
            db.Logger.Enabled = false;
            var book = makeBook();
            save();
            db.Logger.Enabled = true;
            book.Title = "Foo";
            save();

            var changes = explorer.ChangesTo(book);
            Assert.AreEqual(1, changes.Count(), "Expected only one change to be logged - the change in title. It looks like maybe the creation event was also logged?");
            Assert.Throws<CreationDoesNotExistInLogException>(() => explorer.GetCreation(book));
        }

        private bool compare(Book a, Book b)
        {
            return a.Title == b.Title
                && a.NumberOfFans == b.NumberOfFans
                && a.Sequel == b.Sequel
                && a.Style.Hardcover == b.Style.Hardcover;
        }
    }
}
