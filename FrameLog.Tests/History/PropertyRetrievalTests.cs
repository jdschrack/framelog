using FrameLog.Example.Models;
using FrameLog.Example.Models.Testing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using FrameLog.History;

namespace FrameLog.Tests.History
{
    public class PropertyRetrievalTests : HistoryExplorerTests
    {
        [Test]
        public void CanRetrieveChangesToStringPropertyDefault()
        {
            var book = makeBook();
            book.Title = "Foo";
            save();
            book.Title = "Bar";
            save();

            var result = explorer.ChangesTo(book, b => b.Title);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check("Bar", user, change1);
            check("Foo", user, change2);
        }

        [Test]
        public void CanRetrieveChangesToStringPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToStringPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToStringPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToStringPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyDefault()
        {
            var book = makeBook();
            var a = makeBook();
            var b = makeBook();

            book.Sequel = a;
            save();
            book.Sequel = b;
            save();

            var result = explorer.ChangesTo(book, bk => bk.Sequel);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(b, user, change1);
            check(a, user, change2);
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToNavigationPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToNavigationPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyThatHasBeenNullDefault()
        {
            var book = makeBook();
            var sequel = makeBook();
            book.Sequel = sequel;
            save();

            book.Sequel = null;
            save();

            var result = explorer.ChangesTo(book, bk => bk.Sequel);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(sequel, user, change2);
            check(null, user, change1);
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyThatHasBeenNullUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToNavigationPropertyThatHasBeenNullDefault();
        }

        [Test]
        public void CanRetrieveChangesToNavigationPropertyThatHasBeenNullDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToNavigationPropertyThatHasBeenNullDefault();
        }

        [Test]
        public void CanRetrieveChangesToCollectionPropertyDefault()
        {
            var publisher = makePublisher();
            var a = makeBook();
            var b = makeBook();

            publisher.Books.Add(a);
            save();
            publisher.Books.Add(b);
            save();

            var result = explorer.ChangesTo(publisher, bk => bk.Books);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(new List<Book> { a, b }, user, change1, TestHelpers.AreEnumerablesOrderedEqual);
            check(new List<Book> { a }, user, change2, TestHelpers.AreEnumerablesOrderedEqual);
        }

        [Test]
        public void CanRetrieveChangesToCollectionPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToCollectionPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToCollectionPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToCollectionPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToEntityCollectionPropertyDefault()
        {
            var obj = db.ClassesWithEntityCollection.Add(new ClassWithEntityCollection());
            var a = makeUser();
            var b = makeUser();

            obj.Users.Add(a);
            save();
            obj.Users.Add(b);
            save();

            var result = explorer.ChangesTo(obj, o => o.Users);
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(new EntityCollection<User> { a, b }, user, change1, 
                TestHelpers.AreEnumerablesEqual, TestHelpers.FormatEnumerable);
            check(new EntityCollection<User> { a }, user, change2,
                TestHelpers.AreEnumerablesEqual, TestHelpers.FormatEnumerable);
        }

        [Test]
        public void CanRetrieveChangesToEntityCollectionPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToEntityCollectionPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToEntityCollectionPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToEntityCollectionPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToComplexPropertyDefault()
        {
            var book = makeBook();

            book.Style.Hardcover = true;
            book.Style.HasCoverArt = false;
            save();
            book.Style.Hardcover = false;
            book.Style.HasCoverArt = true;
            save();

            var result = explorer.ChangesTo(book, bk => bk.Style).ToList();
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(new Style() { Hardcover = false, HasCoverArt = true }, user, change1);
            check(new Style() { Hardcover = true, HasCoverArt = false }, user, change2);
        }

        [Test]
        public void CanRetrieveChangesToComplexPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToComplexPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToComplexPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToComplexPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNestedComplexPropertyDefault()
        {
            var book = makeBook();

            book.Style.Format.Name = "Large print";
            save();
            book.Style.Format.Name = "Braille";
            save();

            var result = explorer.ChangesTo(book, bk => bk.Style.Format).ToList();
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check(new Format() { Name = "Braille" }, user, change1);
            check(new Format() { Name = "Large print" }, user, change2);
        }

        [Test]
        public void CanRetrieveChangesToNestedComplexPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToNestedComplexPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNestedComplexPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToNestedComplexPropertyDefault();
        }
        
        [Test]
        public void CanRetrieveChangesToNormalPropertyWithinComplexPropertyDefault()
        {
            var book = makeBook();

            book.Style.Format.Name = "Large print";
            save();
            book.Style.Format.Name = "Braille";
            save();

            var result = explorer.ChangesTo(book, bk => bk.Style.Format.Name).ToList();
            var change1 = result.First();
            var change2 = result.Skip(1).First();

            check("Braille", user, change1);
            check("Large print", user, change2);
        }

        [Test]
        public void CanRetrieveChangesToNormalPropertyWithinComplexPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToNormalPropertyWithinComplexPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNormalPropertyWithinComplexPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToNormalPropertyWithinComplexPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNonPublicPropertyDefault()
        {
            var entity = db.ClassesWithProtectedProperty.Add(new ClassWithProtectedProperty());
            save();
            entity.SetNumber(1);
            save();
            entity.SetNumber(2);
            save();

            var result = explorer.ChangesTo(entity).ToList();
            var change1 = result[0];
            var change2 = result[1];

            Assert.AreEqual(2, change1.Value.GetNumber());
            Assert.AreEqual(1, change2.Value.GetNumber());
        }

        [Test]
        public void CanRetrieveChangesToNonPublicPropertyUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            CanRetrieveChangesToNonPublicPropertyDefault();
        }

        [Test]
        public void CanRetrieveChangesToNonPublicPropertyDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            CanRetrieveChangesToNonPublicPropertyDefault();
        }

        [Test]
        public void ChangesToADifferentObjectDoNotAffectChangesToThisObjectDefault()
        {
            var a = makeBook();
            var b = makeBook();
            a.Title = "Foo";
            save();
            b.Title = "Bar";
            save();

            var result = explorer.ChangesTo(a, bk => bk.Title);
            var change1 = result.First();
            var change2 = result.Skip(1).First();
            check("Foo", user, change1);
            check("The Hobbit", user, change2);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void ChangesToADifferentObjectDoNotAffectChangesToThisObjectUsingCloneable()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.UseCloneable;
            ChangesToADifferentObjectDoNotAffectChangesToThisObjectDefault();
        }

        [Test]
        public void ChangesToADifferentObjectDoNotAffectChangesToThisObjectDeepCopy()
        {
            explorer.CloneStrategy = HistoryExplorerCloneStrategies.DeepCopy;
            ChangesToADifferentObjectDoNotAffectChangesToThisObjectDefault();
        }
    }
}
