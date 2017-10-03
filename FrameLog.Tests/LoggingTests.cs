using System;
using System.Data.Entity;
using System.Linq;
using FrameLog.Example.Models;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class LoggingTests : DatabaseBackedTest
    {
        [Test]
        public void AuthorIsLogged()
        {
            makeABookAndChangeItsName();
            var changeset = lastChangeSet();
            Assert.AreEqual(user, changeset.Author);
        }

        [Test]
        public void TimestampIsLogged()
        {
            makeABookAndChangeItsName();
            var changeset = lastChangeSet();
            TestHelpers.IsRecent(changeset.Timestamp, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void ChangesToTextPropertyAreLogged()
        {
            var book = makeABookAndChangeItsName();
            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "Title", book.Title);
        }

        [Test]
        public void ChangesToIntegerPropertyAreLogged()
        {
            var book = makeBook();
            Action theChange = () =>
            {
                book.NumberOfFans = 100;
            };
            save(theChange);

            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "NumberOfFans", book.NumberOfFans);
        }

        [Test]
        public void ChangesToCustomMappedPrimaryKeyAreLogged()
        {
            var bookshelf = new Bookshelf() { BookshelfId = 0, Name = "Good books" };
            Action theChange = () =>
            {
                db.Bookshelves.Add(bookshelf);
            };
            save(theChange);

            var propertyChange = expectPropertyChangeForEntity(bookshelf, "BookshelfId");
            checkPropertyChange(propertyChange, "BookshelfId", bookshelf.BookshelfId);
        }

        [Test]
        public void ChangesToMultipleObjectsWithCustomEqualityComparatorsAreLogged()
        {
            var bookshelf1 = new Bookshelf() { BookshelfId = 0, Name = "Comedy books" };
            var bookshelf2 = new Bookshelf() { BookshelfId = 0, Name = "Horror books" };
            Action theChange = () =>
            {
                db.Bookshelves.Add(bookshelf1);
                db.Bookshelves.Add(bookshelf2);
            };
            save(theChange);

            var changeset = lastChangeSet();
            Assert.AreEqual(2, changeset.ObjectChanges.Count, "Expected two object changes, but there were {0}", changeset.ObjectChanges.Count);
            var changeBookshelf1 = changeset.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.Value == bookshelf1.Name));
            var changeBookshelf2 = changeset.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.Value == bookshelf2.Name));
            Assert.IsNotNull(changeBookshelf1, "Expected changes for bookshelf 1, but there were none");
            Assert.AreEqual(bookshelf1.BookshelfId.ToString(), changeBookshelf1.ObjectReference, "Object references for bookshelf 1 do not match, but they should");
            Assert.IsNotNull(changeBookshelf2, "Expected changes for bookshelf 2, but there were none");
            Assert.AreEqual(bookshelf2.BookshelfId.ToString(), changeBookshelf2.ObjectReference, "Object references for bookshelf 2 do not match, but they should");
        }

        [Test]
        public void ChangesToRelationshipPropertyAreLogged()
        {
            var book = makeBook();
            var sequel = makeBook();
            Action theChange = () =>
            {
                book.Sequel = sequel;
            };
            save(theChange);

            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "Sequel", sequel.Id);
        }

        [Test]
        public void ChangesToComplexPropertyAreLogged()
        {
            var book = makeBook();
            Action theChange = () =>
            {
                book.Style.Hardcover = true;
            };
            save(theChange);
            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "Style.Hardcover", true);
        }

        [Test]
        public void ChangesToNestedComplexPropertyAreLogged()
        {
            var book = makeBook();
            Action theChange = () =>
            {
                book.Style.Format.Name = "Reference";
            };
            save(theChange);
            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "Style.Format.Name", "Reference");
        }

        [Test]
        public void ChangesToPrimitiveKeyAsIdentityAreLogged()
        {
            // Arrange...
            var entity = new ModelWithPrimitiveKey()
            {
                Id = 0, // Note: This is assigned by our database from the INSERT statement
                Field = 1
            };

            // Act...
            db.ModelsWithPrimitiveKey.Add(entity);
            db.Save(user);

            // Assert...
            Assert.AreNotEqual(Guid.Empty, entity.Id, "Entity was not assigned an id, it was expected that the database would assign an id");
            var propertyChange = expectPropertyChangeForEntity(entity, "Id");
            checkPropertyChange(propertyChange, "Id", entity.Id);
        }

        [Test]
        public void ChangesToComplexKeyAsIdentityAreLogged()
        {
            // Arrange...
            var entity = new ModelWithComplexKey()
            {
                Id = Guid.Empty, // Note: This is assigned by our database from the INSERT statement
                Field = 1
            };

            // Act...
            db.ModelsWithComplexKey.Add(entity);
            db.Save(user);

            // Assert...
            Assert.AreNotEqual(Guid.Empty, entity.Id, "Entity was not assigned an id, it was expected that the database would assign an id");
            var propertyChange = expectPropertyChangeForEntity(entity, "Id");
            checkPropertyChange(propertyChange, "Id", entity.Id.ToString());
        }

        [Test]
        public void CanSetPropertyToNull()
        {
            var book = makeBook();
            Action theChange1 = () =>
            {
                book.PublicationDate = DateTime.Now;
            };
            save(theChange1);
            Action theChange2 = () =>
            {
                book.PublicationDate = null;
            };
            save(theChange2);

            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "PublicationDate", null);
        }

        [Test]
        public void SettingRelationshipPropertyToNullIsLogged()
        {
            var book = makeBook();
            var sequel = makeBook();
            Action theChange1 = () =>
            {
                book.Sequel = sequel;
            };
            save(theChange1);
            Action theChange2 = () =>
            {
                book.Sequel = null;
            };
            save(theChange2);

            var propertyChange = expectOnePropertyChangeForEntity(book);
            checkPropertyChange(propertyChange, "Sequel", null);
        }

        [Test]
        public void ChangesToSetAreLogged()
        {
            var b1 = makeBook();
            var b2 = makeBook();
            var b3 = makeBook();
            var p = makePublisher();

            // Can add one
            Action theChange1 = () =>
            {
                p.Books.Add(b1);
            };
            save(theChange1);
            checkSetLog(p, b1);

            // Can add more than one at once
            Action theChange2 = () =>
            {
                p.Books.Add(b2);
                p.Books.Add(b3);
            };
            save(theChange2);
            checkSetLog(p, b1, b2, b3);

            // Can remove one
            Action theChange3 = () =>
            {
                p.Books.Remove(b1);
            };
            save(theChange3);
            checkSetLog(p, b2, b3);

            // Can add and remove at once
            Action theChange4 = () =>
            {
                p.Books.Add(b1);
                p.Books.Remove(b2);
            };
            save(theChange4);
            checkSetLog(p, b1, b3);

            // Can remove more than one at once
            Action theChange5 = () =>
            {
                p.Books.Remove(b1);
                p.Books.Remove(b3);
            };
            save(theChange5);
            checkSetLog(p);
        }
        private void checkSetLog(Publisher pub, params Book[] books)
        {
            var change = expectOnePropertyChangeForEntity(pub);
            Assert.AreEqual("Books", change.PropertyName);
            var expected = books.Select(i => db.FrameLogContext.GetReferenceForObject(i));
            var actual = change.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            TestHelpers.EnumerablesAreEqual(expected, actual);
        }

        [Test]
        public void MultipleChangesWithASingleSaveResultInASingleChangeset()
        {
            var a = makeBook();
            var b = makeBook();
            var count = db.ChangeSets.Count();
            Action theChange = () =>
            {
                a.NumberOfFans++;
                b.Title += ": An unexpected journey";
            };

            save(theChange);

            Assert.AreEqual(1, db.ChangeSets.Count() - count, "Expected just one changeset");
        }

        [Test]
        public void MultipleSavesResultInMultipleChangesets()
        {
            var book = makeBook();
            var count = db.ChangeSets.Count();
            Action theChange = () =>
            {
                book.NumberOfFans++;
            };

            save(theChange);
            save(theChange);

            Assert.AreEqual(2, db.ChangeSets.Count() - count, "Expected two changesets");
        }

        [Test]
        public void ChangesAreNotLoggedWhenLoggingIsDisabled()
        {
            db.Logger.Enabled = false;
            var book = makeBook();

            string reference = book.Id.ToString();
            string typeName = typeof(Book).Name;
            var matches = db.ObjectChanges.Where(o => o.TypeName == typeName && o.ObjectReference == reference);
            Assert.IsFalse(matches.Any(), "Found matching log items, expected none");
        }

        [Test]
        public void SaveResultReturnsAffectedEntitiesAndChangeSet()
        {
            var book = makeBook();
            Action theChange = () =>
            {
                book.Title += "Test";
            };
            var result = save(theChange);
            Assert.AreEqual(1, result.AffectedObjectCount);
            Assert.AreEqual(lastChangeSet(), result.ChangeSet);
        }

        private void checkPropertyChange(PropertyChange propertyChange, string expectedName, object value)
        {
            Assert.AreEqual(expectedName, propertyChange.PropertyName);
            Assert.AreEqual(toString(value), propertyChange.Value);

            if (value is int)
                Assert.AreEqual(value, propertyChange.ValueAsInt);
            else
                Assert.Null(propertyChange.ValueAsInt);
        }

        private PropertyChange expectOnePropertyChangeForEntity(object entity)
        {
            var changeset = lastChangeSet();
            string reference = db.FrameLogContext.GetReferenceForObject(entity);

            Assert.AreEqual(1, changeset.ObjectChanges.Count(),
                "Expected just one object change. Instead we had the following: {0}", string.Join(", ", changeset.ObjectChanges));
            var objectChange = changeset.ObjectChanges.Single();
            Assert.AreEqual(entity.GetType().Name, objectChange.TypeName, "The object change was for an entity of the wrong type");
            Assert.AreEqual(reference, objectChange.ObjectReference, "The object change was for the wrong entity (ID mismatch)");

            Assert.AreEqual(1, objectChange.PropertyChanges.Count(),
                "Expected just one property change. Instead we had the following: {0}", string.Join(", ", objectChange.PropertyChanges));
            return objectChange.PropertyChanges.Single();
        }

        private PropertyChange expectPropertyChangeForEntity(object entity, string propertyName)
        {
            var changeset = lastChangeSet();

            Assert.IsNotNull(changeset);
            var objectChange = changeset.ObjectChanges.FirstOrDefault();
            Assert.NotNull(objectChange, "Excepted at least one object change, but none were logged");
            Assert.NotNull(objectChange.PropertyChanges, "Excepted at least one property change, but none were logged");
            var propertyChange = objectChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == propertyName);
            Assert.NotNull(propertyChange, string.Format("There is no property change for \"{0}\", it was expected that this property would be logged", propertyName));
            return propertyChange;
        }

        private string toString(object value)
        {
            if (value == null)
                return null;
            else
                return value.ToString();
        }

        private Book makeABookAndChangeItsName()
        {
            var book = makeBook();
            Action theChange = () =>
            {
                string oldTitle = book.Title;
                string newTitle = book.Title = oldTitle + "New";
            };
            save(theChange);
            return book;
        }

        protected virtual ISaveResult<ChangeSet> save(Action theChange)
        {
            if (theChange != null)
                theChange();

            return save();
        }
    }
}
