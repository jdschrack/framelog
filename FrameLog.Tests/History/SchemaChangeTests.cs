using System.Linq;
using FrameLog.Example.Models;
using FrameLog.Exceptions;
using FrameLog.History;
using NUnit.Framework;

namespace FrameLog.Tests.History
{
    public class SchemaChangeTests : HistoryExplorerTests
    {
        [Test]
        public void CanRetrieveHistoryWherePropertyHasBeenRemovedDefault()
        {
            // Let's imagine a scenario where Book used to have another property, 'Coolness'
            var book = makeBook();
            // We simulate this by manually creating a fake property change
            var objectChange = lastChangeSet().ObjectChanges.Single();
            objectChange.PropertyChanges.Add(new PropertyChange() { PropertyName = "Coolness", Value = "5", ValueAsInt = 5 });

            // Now let's see if we can retrieve the other data
            var change = explorer.ChangesTo(book).Single();
            Assert.AreEqual(book.Title, change.Value.Title);
            Assert.IsTrue(change.ProblemsRetrievingData);
            Assert.IsInstanceOf<UnknownPropertyInLogException<User>>(change.Errors.Single());
        }

        [Test]
        public void CanRetrieveHistoryWherePropertyHasBeenRemovedUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveHistoryWherePropertyHasBeenRemovedDefault();
        }

        [Test]
        public void CanRetrieveHistoryWherePropertyHasBeenRemovedDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveHistoryWherePropertyHasBeenRemovedDefault();
        }
    }
}
