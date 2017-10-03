using System;
using System.Collections.Generic;
using FrameLog.Example;
using FrameLog.Example.Models;
using FrameLog.History;
using NUnit.Framework;

namespace FrameLog.Tests.History
{
    public abstract class HistoryExplorerTests : DatabaseBackedTest
    {
        protected HistoryExplorer<ChangeSet, User> explorer { get; private set; }

        protected override void setupWithDatabase(ExampleContext db)
        {
            base.setupWithDatabase(db);
            explorer = new HistoryExplorer<ChangeSet, User>(db.FrameLogContext);
        }

        protected void check<T>(T value, User author, IChange<T, User> change,
                   Func<T, T, bool> equalityCheck = null, Func<T, string> formatter = null)
        {
            equalityCheck = equalityCheck ?? EqualityComparer<T>.Default.Equals;
            formatter = formatter ?? defaultFormatter;

            Assert.True(equalityCheck(value, change.Value),
                string.Format("Values were not equal. Expected: '{0}'. Actual: '{1}'",
                formatter(value), formatter(change.Value)));
            Assert.AreEqual(author, change.Author);
            TestHelpers.IsRecent(change.Timestamp, TimeSpan.FromSeconds(5));
        }
        private string defaultFormatter<T>(T x)
        {
            if (x != null)
                return x.ToString();
            else
                return "<null>";
        }
    }
}
