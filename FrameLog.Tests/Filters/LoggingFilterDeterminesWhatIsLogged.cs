using FrameLog.Contexts;
using FrameLog.Example.Models;
using FrameLog.Filter;
using FrameLog.Helpers;
using FrameLog.History;
using Moq;
using NUnit.Framework;
using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;

namespace FrameLog.Tests.Filters
{
    public class LoggingFilterDeterminesWhatIsLogged : DatabaseBackedTest
    {
        private Mock<ILoggingFilter> filter;
        private HistoryExplorer<ChangeSet, User> history;
        private MetadataSpaceMapper mapper;

        protected override Example.ExampleContext makeDatabase()
        {
            filter = new Mock<ILoggingFilter>();
            filter.Setup(f => f.ShouldLog(It.IsAny<Type>(), It.IsAny<string>())).Returns(true);
            filter.Setup(f => f.ShouldLog(It.IsAny<NavigationProperty>())).Returns(true);
            filter.Setup(f => f.ShouldLog(It.IsAny<Type>())).Returns(true);

            var provider = new Mock<ILoggingFilterProvider>();
            provider.Setup(p => p.Get(It.IsAny<IFrameLogContext>())).Returns(filter.Object);
            return new Example.ExampleContext(filterProvider: provider.Object);
        }

        protected override void setupWithDatabase(Example.ExampleContext db)
        {
            base.setupWithDatabase(db);
            history = new HistoryExplorer<ChangeSet, User>(db.FrameLogContext);
            mapper = new MetadataSpaceMapper(db.FrameLogContext.Workspace);
        }

        [Test]
        public void CanControlWhetherClassIsLogged()
        {
            var book = makeBook();
            bool log = true;
            filter.Setup(f => f.ShouldLog(It.IsAny<Type>())).Returns(() => log);

            log = true;
            book.Title = "Logged change";
            save();

            log = false;
            book.Title = "Unlogged change";
            save();

            var titleChanges = history.ChangesTo(book, b => b.Title).Select(c => c.Value);
            TestHelpers.Contains("Logged change", titleChanges);
            TestHelpers.DoesNotContain("Unlogged change", titleChanges);
        }           

        [Test]
        public void CanControlWhetherScalarPropertyIsLogged()
        {
            var book = makeBook();
            bool log = true;
            string propertyName = nameFor(book, b => b.Title);
            filter.Setup(f => f.ShouldLog(It.IsAny<Type>(), propertyName)).Returns(() => log);

            log = true;
            book.Title = "Logged title";
            save();

            log = false;
            book.Title = "Unlogged title";
            save();

            var titleChanges = history.ChangesTo(book, b => b.Title).Select(c => c.Value);
            TestHelpers.Contains("Logged title", titleChanges);
            TestHelpers.DoesNotContain("Unlogged title", titleChanges);
        }

        [Test]
        public void CanControlWhetherNavigationPropertyIsLogged()
        {
            var book = makeBook();
            var sequelA = makeBook();
            var sequelB = makeBook();

            bool log = true;
            NavigationProperty property = mapper.Map(book, b => b.Sequel);
            filter.Setup(f => f.ShouldLog(It.Is<NavigationProperty>(p => p == property))).Returns(() => log);

            log = true;
            book.Sequel = sequelA;
            save();

            log = false;
            book.Sequel = sequelB;
            save();

            var sequelChanges = history.ChangesTo(book, b => b.Sequel).Select(c => c.Value);
            TestHelpers.Contains(sequelA, sequelChanges);
            TestHelpers.DoesNotContain(sequelB, sequelChanges);
        }

        private string nameFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> expression)
        {
            return ExpressionHelper.GetPropertyName<TModel, TValue>(expression);
        }
    }
}
