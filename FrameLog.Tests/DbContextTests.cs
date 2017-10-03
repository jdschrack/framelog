﻿using System;
﻿using System.Data.Entity.Infrastructure;
﻿using System.Data.Entity.Validation;
using System.Linq;
﻿using FrameLog.Example.Models;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class DbContextTests : DatabaseBackedTest
    {
        [Test]
        public void InvalidEntityWillTriggerDbEntityValidationExceptionOnSaveIfValidationIsEnabled()
        {
            // Arrange...
            var entity = new ModelWithValidation()
            {
                Field = 1,
                PinCode = 123
            };

            // Act...
            db.Configuration.ValidateOnSaveEnabled = true;
            db.ModelsWithValidation.Add(entity);
            
            Assert.Throws<DbEntityValidationException>(() => db.Save(user), "Expected a validation exception, but there was none");
        }

        [Test]
        public void InvalidEntityWillSaveAndBeLoggedIfValidationIsDisabled()
        {
            // Arrange...
            var entity = new ModelWithValidation()
            {
                Field = 1,
                PinCode = 123
            };

            // Act...
            db.Configuration.ValidateOnSaveEnabled = false;
            db.ModelsWithValidation.Add(entity);
            save();

            // Assert...
            var changeset = lastChangeSet();
            Assert.IsNotNull(changeset);
            Assert.AreEqual(1, changeset.ObjectChanges.Count, "Excepted one object change, but there was {0}", changeset.ObjectChanges.Count);
            Assert.AreNotEqual(0, entity.Id, "Entity was not assigned an id, it probably was not saved");
            Assert.AreEqual(entity.Id.ToString(), changeset.ObjectChanges.First().ObjectReference, "Entity was not in the change set, but was expected");
            Assert.IsTrue(changeset.ObjectChanges.First().PropertyChanges.Any(x => x.PropertyName == "PinCode"), "Entity 'PinCode' was not changed, but was expected");
        }

        [Test]
        public void ValidEntityWillSaveAndBeLoggedIfValidationIsEnabled()
        {
            // Arrange...
            var entity = new ModelWithValidation()
            {
                Field = 1,
                PinCode = 85601
            };

            // Act...
            db.Configuration.ValidateOnSaveEnabled = true;
            db.ModelsWithValidation.Add(entity);
            save();

            // Assert...
            var changeset = lastChangeSet();
            Assert.IsNotNull(changeset);
            Assert.AreEqual(1, changeset.ObjectChanges.Count, "Excepted one object change, but there was {0}", changeset.ObjectChanges.Count);
            Assert.AreNotEqual(0, entity.Id, "Entity was not assigned an id, it probably was not saved");
            Assert.AreEqual(entity.Id.ToString(), changeset.ObjectChanges.First().ObjectReference, "Entity was not in the change set, but was expected");
            Assert.IsTrue(changeset.ObjectChanges.First().PropertyChanges.Any(x => x.PropertyName == "PinCode"), "Entity 'PinCode' was not changed, but was expected");
        }

        [Test]
        public void ViolatingConcurrencyCheckWillTriggerDbUpdateConcurrencyExceptionOnSave()
        {
            // Arrange...
            var entity = new ModelWithConcurrency() { Field = 1 };
            db.ModelsWithConcurrency.Add(entity);
            save();

            // Act...
            var dbUser1 = makeDatabase();
            var dbUser2 = makeDatabase();
            var entityInstanceUser1 = dbUser1.ModelsWithConcurrency.Find(entity.Id);
            var entityInstanceUser2 = dbUser2.ModelsWithConcurrency.Find(entity.Id);
            entityInstanceUser1.Field = 2;
            dbUser1.Save(user);
            entityInstanceUser2.Field = 3;

            // Assert...
            var ex = Assert.Throws<DbUpdateConcurrencyException>(() => dbUser2.Save(user), "Expected a concurrency violation exception, but there was none");
            Assert.IsNotNull(ex, "Expected a concurrency violation, but there was none");
            Assert.IsNotNull(ex.Entries, "Entity was not in the concurrency check, but was expected");
            Assert.IsNotEmpty(ex.Entries, "Entity was not in the concurrency check, but was expected");
            Assert.AreSame(entityInstanceUser2, ex.Entries.First().Entity, "Entity was not in the concurrency check, but was expected");
        }
    }
}