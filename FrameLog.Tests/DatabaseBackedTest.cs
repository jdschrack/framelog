using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using FrameLog.Example;
using FrameLog.Example.Models;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public abstract class DatabaseBackedTest
    {
        protected ExampleContext db;
        protected User user;
        protected Random random;
        protected Action<DbContext> onSaveChanges;

        protected DatabaseBackedTest()
        {
            this.onSaveChanges = (ctx) => { };
        }

        protected DatabaseBackedTest(Action<DbContext> onSaveChanges)
        {
            this.onSaveChanges = onSaveChanges;
        }

        [TestFixtureSetUp]
        public void CreateDatabase()
        {
            var db = makeDatabase();
            db.Database.Delete();
            db.Database.Create();
            new ExampleContextInitializer().InitializeDatabase(db);

            user = new User() { Name = "TestUser" };
            db.Users.Add(user);
            db.Save(user);

            random = new Random((int) DateTime.UtcNow.Ticks);
        }

        [TestFixtureTearDown]
        public void DeleteDatabase()
        {
            var db = makeDatabase();
            db.Database.Delete();
        }

        [SetUp]
        public void CreateContext()
        {
            db = makeDatabase();
            setupWithDatabase(db);
        }
        protected virtual void setupWithDatabase(ExampleContext db)
        {
            //by default, do nothing
        }
        protected virtual ExampleContext makeDatabase()
        {
            return new ExampleContext(customSaveChangesLogic: onSaveChanges);
        }

        [TearDown]
        public void DisposeContext()
        {
            db.Dispose();
        }

        protected virtual ISaveResult<ChangeSet> save()
        {
            var result = db.Save(user);
            pause();
            return result;
        }

        protected Book makeBook()
        {
            var book = new Book() { Title = "The Hobbit" };
            db.Books.Add(book);
            save();
            return book;
        }
        protected Publisher makePublisher()
        {
            var pub = new Publisher() { Name = String.Format("Acme Publishing {0}", random.Next()) };
            db.Publishers.Add(pub);
            save();
            return pub;
        }
        protected User makeUser()
        {
            var u = new User();
            db.Users.Add(u);
            save();
            return u;
        }
        private void pause()
        {
            // We pause to ensure consistent timestamp ordering for logs
            Thread.Sleep(20);
        }
        protected ChangeSet lastChangeSet(ExampleContext db = null)
        {
            return (db ?? this.db).ChangeSets.OrderByDescending(c => c.Timestamp).First();
        }
    }
}
