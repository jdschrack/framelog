using System;
using System.Data.Entity;
using System.Linq;
using FrameLog.Example.Models;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class DbContextLoggingTests : LoggingTests
    {
        private const string HowToLearnFrenchPart1 = "How to speak french (part 1)";
        private const string HowToLearnFrenchPart2 = "How to speak french (part 2)";

        [Test]
        public void CustomLogicOverriddenInSaveChangesWillGetExecuted()
        {
            // Arrange...
            var book = makeBook();

            // Act...
            book.NumberOfFans = 3;
            db.Save(user);

            // Assert...
            Assert.Greater(book.NumberOfFans, 3, "Expected custom logic in SaveChanges() to increase the number of fans, this did not happen");
        }

        [Test]
        public void CustomSaveChangesThatTriggersAnExceptionWillNotPersistTheChanges()
        {
            // Arrange...
            // NOTE: 1 million fans will trigger an overflow exception on save (see customSaveChanges() below)
            var book = new Book()
            {
                Title = "How to be really popular",
                NumberOfFans = 1000000
            };

            // Act...
            db.Books.Add(book);

            // Assert...
            var ex = Assert.Throws<OverflowException>(() => save(), "Expected the book to trigger an overflow exception, but it didn't");
            Assert.AreEqual(0, book.Id, "Expected that the book would not be assigned an id, but it was the id of \"{0}\"", book.Id);
        }

        [Test]
        public void CustomSaveChangesThatTriggersAnExceptionWillNotLogTheChanges()
        {
            // Arrange...
            var book = makeBook();
            var lastChange = lastChangeSet();

            // Act...
            // NOTE: 1 million fans will trigger an overflow exception on save (see customSaveChanges() below)
            book.NumberOfFans = 1000000;
            
            // Assert...
            var ex = Assert.Throws<OverflowException>(() => save(), "Expected the book to trigger an overflow exception, but it didn't");
            var changeSet = lastChangeSet();
            Assert.AreEqual(lastChange.Id, changeSet.Id, "Changes were logged, but none were expected");
        }

        [Test]
        public void CustomSaveChangesThatModifiesAnAlreadyModifiedPropertyWillBeLogged()
        {
            // Arrange...
            var book = makeBook();

            // Act...
            book.NumberOfFans = 3;
            save();

            // Assert...
            var changeset = lastChangeSet();
            Assert.IsNotNull(changeset, "Excepted changes to be logged, but there were none");
            var objectChange = changeset.ObjectChanges.FirstOrDefault();
            Assert.IsNotNull(objectChange, "Excepted at least one object change, but there were none");
            var numberOfFansProperty = objectChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == "NumberOfFans");
            Assert.IsNotNull(numberOfFansProperty, "Excepted the 'NumberOfFans' property to be logged, but it was not");
            Assert.AreEqual(book.NumberOfFans, numberOfFansProperty.ValueAsInt, "The value logged does not match the current property value, the modifications during SaveChanges() were not picked up by FrameLog");
        }

        [Test]
        public void CustomSaveChangesThatModifiesAnUnchangedPropertyOfAnAlreadyModifiedObjectWillBeLogged()
        {
            // Arrange...
            var book = makeBook();

            // Act...
            book.Title = String.Format("{0} 2: A New Beginning", book.Title);
            save();

            // Assert...
            var changeset = lastChangeSet();
            Assert.IsNotNull(changeset, "Excepted changes to be logged, but there were none");
            var objectChange = changeset.ObjectChanges.FirstOrDefault();
            Assert.IsNotNull(objectChange, "Excepted at least one object change, but there were none");
            var numberOfFansProperty = objectChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == "NumberOfFans");
            Assert.IsNotNull(numberOfFansProperty, "Excepted the 'NumberOfFans' property to be logged, but it was not");
            Assert.AreEqual(book.NumberOfFans, numberOfFansProperty.ValueAsInt, "The value logged does not match the current property value, the modifications during SaveChanges() were not picked up by FrameLog");
        }

        [Test]
        public void CustomSaveChangesThatModifiesAnUnchangedPropertyOfAnUnchangedObjectWillBeLogged()
        {
            // Arrange...
            var howToLearnFrenchPart1 = new Book() { Title = HowToLearnFrenchPart1 };

            // Act...
            db.Books.Add(howToLearnFrenchPart1);
            save();

            // Assert...
            var changeSet = lastChangeSet();
            var frenchPart1ObjectChange = changeSet.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.PropertyName == "Title" && y.Value == HowToLearnFrenchPart1));
            var frenchPart2ObjectChange = changeSet.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.PropertyName == "Title" && y.Value == HowToLearnFrenchPart2));
            Assert.IsNotNull(frenchPart1ObjectChange, "Expected changes to be logged for french (part 1), but there were none");
            Assert.IsNotNull(frenchPart2ObjectChange, "Expected changes to be logged for french (part 2), but there were none");
        }

        [Test]
        public void CustomSaveChangesThatLinksAnUnattachedObjectWillBeLoggedWithTheCorrectRelationshipReferences()
        {
            // Arrange...
            var howToLearnFrenchPart1 = new Book() { Title = HowToLearnFrenchPart1 };

            // Act...
            db.Books.Add(howToLearnFrenchPart1);
            save();

            // Assert...
            var changeSet = lastChangeSet();
            var frenchPart1ObjectChange = changeSet.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.PropertyName == "Title" && y.Value == HowToLearnFrenchPart1));
            var frenchPart2ObjectChange = changeSet.ObjectChanges.FirstOrDefault(x => x.PropertyChanges.Any(y => y.PropertyName == "Title" && y.Value == HowToLearnFrenchPart2));
            Assert.IsNotNull(frenchPart1ObjectChange, "Expected changes to be logged for french (part 1), but there were none");
            Assert.IsNotNull(frenchPart2ObjectChange, "Expected changes to be logged for french (part 2), but there were none");
            Assert.IsNotNull(howToLearnFrenchPart1.Sequel, "Expected sequal to be created for french (part 1) during save changes, but it was not");
            Assert.AreEqual(howToLearnFrenchPart1.Id.ToString(), frenchPart1ObjectChange.ObjectReference, "Actual object reference for french (part 1) does not match the logged reference");
            Assert.AreEqual(howToLearnFrenchPart1.Sequel.Id.ToString(), frenchPart2ObjectChange.ObjectReference, "Actual object reference for french (part 1) does not match the logged reference");
            var frenchPart1SequalPropertyChange = frenchPart1ObjectChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == "Sequel");
            Assert.IsNotNull(frenchPart1SequalPropertyChange, "Expected property change for the 'Sequel' of french (part 1), but there was none");
            Assert.AreEqual(frenchPart2ObjectChange.ObjectReference, frenchPart1SequalPropertyChange.Value, "Actual object reference for french (part 2 ) does not match the logged reference in 'Sequal' property of french (part 1)");
        }

        protected override ISaveResult<ChangeSet> save()
        {
            db.CustomSaveChangesLogic = (ctx) =>
            {
                var changedBooks = ctx.ChangeTracker.Entries<Book>().Where(e =>
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                );

                foreach (var book in changedBooks)
                {
                    // If the book is how to speak french, auto-add its sequal
                    if (book.Entity.Title == HowToLearnFrenchPart1 && book.State == EntityState.Added)
                        book.Entity.Sequel = new Book() {Title = HowToLearnFrenchPart2};

                    // Everytime a book is saved, it gains a new fan! Muhahaha!
                    book.Entity.NumberOfFans++;

                    // Whoops, we are terrible programmers and for some reason 
                    // having a book with over 1 million fans will cause an overflow exception
                    if (book.Entity.NumberOfFans > 1000000)
                        throw new OverflowException("Unable to handle the insane popularity of this book");
                }
            };

            return base.save();
        }

        protected override ISaveResult<ChangeSet> save(Action theChange)
        {
            if (theChange != null)
                db.CustomSaveChangesLogic = (ctx) => theChange.Invoke();

            return base.save();
        }
    }
}
