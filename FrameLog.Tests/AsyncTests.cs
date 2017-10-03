using System;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class AsyncTests : DatabaseBackedTest
    {
        [Test]
        public void ChangesAreLoggedOnceSaveTaskHasCompleted()
        {
            // Arrange...
            var book = makeBook();
            var lastChangeset = lastChangeSet();

            // Act...
            book.Title = "Fire and wait around awhile, we need you!";
            var saveTask = db.SaveAsync(user);
            Task.WaitAll(saveTask);
            var changeset = lastChangeSet();

            // Assert...
            Assert.NotNull(lastChangeset);
            Assert.NotNull(changeset);
            Assert.IsTrue(saveTask.IsCompleted, "Task should have completed, but was not");
            Assert.AreNotEqual(lastChangeset.Id, changeset.Id, "Changes have not been logged, but changes were expected");
        }

        [Test]
        public void ChangesAreNotALoggedImmediatelyAfterStartingSaveTask()
        {
            // Arrange...
            var book = makeBook();
            var changesDb = makeDatabase();
            var lastChangeset = lastChangeSet();

            // Act...
            book.Title = "Fire and forget, we'll check back later!";
            var saveTask = db.SaveAsync(user);
            var changeset = lastChangeSet(changesDb);

            // Assert...
            Assert.NotNull(lastChangeset);
            Assert.NotNull(changeset);
            Assert.AreEqual(lastChangeset.Id, changeset.Id, "Changes have been logged, but changes were not expected");
            Task.WaitAll(saveTask);
        }

        [Test]
        public void ChangesAreNotLoggedIfSaveTaskIsImmediatallyCancelled()
        {
            // Arrange...
            var book = makeBook();
            var lastChangeset = lastChangeSet();
            var changeset = lastChangeset;
            var cancellationTokenSource = new CancellationTokenSource();
            Task saveTask = null;

            // Act...
            book.Title = "Time Travel 101, stop tasks before they even begin";
            cancellationTokenSource.Cancel();
            saveTask = db.SaveAsync(user, cancellationTokenSource.Token);
            
            // Assert...
            // We expect that EF fails to 'Detect Changes' because we have signalled the task to cancel
            var ex = Assert.Throws<AggregateException>(() => Task.WaitAll(saveTask));
            Assert.IsTrue(ex.InnerExceptions.Any());
            Assert.IsInstanceOf<TaskCanceledException>(ex.InnerExceptions.First());
            Assert.NotNull(changeset = lastChangeSet());
            Assert.NotNull(changeset);
            Assert.NotNull(saveTask);
            Assert.IsTrue((saveTask.IsCanceled | saveTask.IsFaulted), "Task should have canceled or faulted, but didn't");
            Assert.AreEqual(lastChangeset.Id, changeset.Id, "Changes have been logged, but changes were not expected");
        }

        [Test]
        public void AffectedEntitiesAndChangeSetAreReturnedBySaveTaskUponCompletion()
        {
            // Arrange...
            var book1 = makeBook();
            var book2 = makeBook();
            var lastChangeset = lastChangeSet();

            // Act...
            book1.Title = "51 things to do with a paper clip";
            book2.Title = "51 more things to do with a paper clip";
            var saveTask = db.SaveAsync(user);
            Task.WaitAll(saveTask);
            var changeset = lastChangeSet();

            // Assert...
            Assert.NotNull(lastChangeset);
            Assert.NotNull(changeset);
            Assert.AreNotEqual(lastChangeset.Id, changeset.Id, "Changes have not been logged, but changes were expected");
            Assert.AreEqual(2, saveTask.Result.AffectedObjectCount, string.Format("Only two object changes were expected, but there were {0}", lastChangeset.ObjectChanges.Count));
            Assert.AreEqual(changeset.Id, saveTask.Result.ChangeSet.Id, "The changeset returned by the save operation is no the latest changeset, but it should have been");
        }
    }
}
