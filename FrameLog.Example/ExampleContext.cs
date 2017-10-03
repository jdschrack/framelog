using System;
using System.Linq;
using FrameLog.Contexts;
using FrameLog.Example.Models;
using FrameLog.Example.Models.Testing;
using FrameLog.Filter;
using FrameLog.History;
using System.Data.Entity;

namespace FrameLog.Example
{
    public partial class ExampleContext : DbContext
    {
        public ExampleContext(Action<DbContext> customSaveChangesLogic = null, ILoggingFilterProvider filterProvider = null)
        {
            Database.SetInitializer<ExampleContext>(new ExampleContextInitializer());
            Logger = new FrameLogModule<ChangeSet, User>(new ChangeSetFactory(), FrameLogContext, filterProvider);
            CustomSaveChangesLogic = customSaveChangesLogic;
        }

        public DbSet<Bookshelf> Bookshelves { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ModelWithPrimitiveKey> ModelsWithPrimitiveKey { get; set; }
        public DbSet<ModelWithComplexKey> ModelsWithComplexKey { get; set; }
        public DbSet<ModelWithDynamicProxy> ModelsWithDynamicProxies { get; set; }
        public DbSet<ModelWithValidation> ModelsWithValidation { get; set; }
        public DbSet<ModelWithConcurrency> ModelsWithConcurrency { get; set; }

        #region unit test support
        public DbSet<ClassWithSomeExcludedProperties> ClassesWithSomeExcludedProperties { get; set; }
        public DbSet<ClassWithDoNotLog> ClassesWithDoNotLog { get; set; }
        public DbSet<ClassWithList> ClassesWithList { get; set; }
        public DbSet<ClassWithHashSet> ClassesWithHashSet { get; set; }
        public DbSet<ClassWithCollection> ClassesWithCollection { get; set; }
        public DbSet<ClassWithEntityCollection> ClassesWithEntityCollection { get; set; }
        public DbSet<VanillaTestClass> VanillaTestClasses { get; set; }
        public DbSet<ClassWithSomeIncludedProperties> ClassesWithSomeIncludedProperties { get; set; }
        public DbSet<ClassWithProtectedProperty> ClassesWithProtectedProperty { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ClassWithProtectedProperty.ClassWithProtectedPropertyConfiguration());
            modelBuilder.Entity<Bookshelf>().HasKey(o => o.BookshelfId);
        }
        #endregion

        #region logging
        public DbSet<ChangeSet> ChangeSets { get; set; }
        public DbSet<ObjectChange> ObjectChanges { get; set; }
        public DbSet<PropertyChange> PropertyChanges { get; set; }

        public readonly FrameLogModule<ChangeSet, User> Logger;
        public IFrameLogContext<ChangeSet, User> FrameLogContext
        {
            get { return new ExampleContextAdapter(this); }
        }
        public HistoryExplorer<ChangeSet, User> HistoryExplorer
        {
            get { return new HistoryExplorer<ChangeSet, User>(FrameLogContext); }
        }
        public override int SaveChanges()
        {
            // This is an example of custom logic overriden within the EntityFramework's vanilla DbContext.SaveChanges().
            // Some developers have hooks here to perform various checks against their domain model during saves.
            // We want to make sure any code overridden here is called when using FrameLog in order to maintain a 
            // similar experereience as one would get when using vanilla EntityFramework.
            if (CustomSaveChangesLogic != null)
                CustomSaveChangesLogic(this);

            return base.SaveChanges();
        }
        public ISaveResult<ChangeSet> Save(User author)
        {
            // NOTE: This will eventually circle back and call our overridden SaveChanges() later
            return Logger.SaveChanges(author);
        }
        
        public Action<DbContext> CustomSaveChangesLogic { get; set; }

        #endregion
    }
}
