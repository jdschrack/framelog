using System.Linq;
using FrameLog.Example.Models;
using FrameLog.Example.Models.Testing;
using FrameLog.Filter;
using NUnit.Framework;

namespace FrameLog.Tests.Filters
{
    public class BlacklistLoggingFilterTests : DatabaseBackedTest
    {
        protected override Example.ExampleContext makeDatabase()
        {
            return new Example.ExampleContext(filterProvider: new BlacklistLoggingFilter.Provider());
        }

        [Test]
        public void EverythingIsLoggedByDefault()
        {
            ObjectChange objectChange;

            var obj = db.VanillaTestClasses.Add(new VanillaTestClass());
            save();

            obj.ScalarProperty = 5;
            save();
            objectChange = lastChangeSet().ObjectChanges.Single();
            Assert.AreEqual(typeof(VanillaTestClass).Name, objectChange.TypeName);
            Assert.AreEqual(obj.Id.ToString(), objectChange.ObjectReference);
            Assert.AreEqual("ScalarProperty", objectChange.PropertyChanges.Single().PropertyName);

            obj.NavigationProperty = user;
            save();
            objectChange = lastChangeSet().ObjectChanges.Single();
            Assert.AreEqual(typeof(VanillaTestClass).Name, objectChange.TypeName);
            Assert.AreEqual(obj.Id.ToString(), objectChange.ObjectReference);
            Assert.AreEqual("NavigationProperty", objectChange.PropertyChanges.Single().PropertyName);
        }

        [Test]
        public void DoNotLogPreventsPropertyFromBeingLogged()
        {
            var a = makeObject();
            save();
            var creation = lastChangeSet();

            a.ExcludedScalarProperty = "George";
            save();
            var set = lastChangeSet();

            Assert.AreEqual(creation, set, "A change set was created even though the only change was to a field that should not be logged");            
        }

        [Test]
        public void DoNotLogPreventsNavigationPropertyFromBeingLogged()
        {
            var a = makeObject();
            save();
            var creation = lastChangeSet();

            a.ExcludedNavigationProperty = user;
            save();
            var set = lastChangeSet();

            Assert.AreEqual(creation, set, "A change set was created even though the only change was to a field that should not be logged");
        }

        [Test]
        public void DoNotLogInMetadataPreventsPropertyFromBeingLogged()
        {
            var a = makeObject();
            save();
            var creation = lastChangeSet();

            a.PropertyExcludedByMetadata = 5;
            save();
            var set = lastChangeSet();

            Assert.AreEqual(creation, set, "A change set was created even though the only change was to a field that should not be logged");
        }

        [Test]
        public void DoNotLogPreventsPrivatePropertyFromBeingLogged()
        {
            var a = makeObject();
            save();
            var creation = lastChangeSet();

            a.SetPrivateExcludedProperty(5);
            save();
            var set = lastChangeSet();

            Assert.AreEqual(creation, set, "A change set was created even though the only change was to a field that should not be logged");
        }

        [Test]
        public void DoNotLogPreventsClassFromBeingLogged()
        {
            var a = db.ClassesWithDoNotLog.Add(new ClassWithDoNotLog());
            a.Property = "Foo";
            save();

            foreach (var objectChange in lastChangeSet().ObjectChanges)
                Assert.AreNotEqual(typeof(ClassWithDoNotLog).Name, objectChange.TypeName);
        }

        private ClassWithSomeExcludedProperties makeObject()
        {
            return db.ClassesWithSomeExcludedProperties.Add(new ClassWithSomeExcludedProperties());
        }
    }
}
