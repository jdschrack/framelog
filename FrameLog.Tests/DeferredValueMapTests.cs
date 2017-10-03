using System.Linq;
using FrameLog.Logging;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class DeferredValueMapTests
    {
        private DeferredValueMap map;
        
        [SetUp]
        public void CreateBlankMap()
        {
            map = new DeferredValueMap();
        }

        [Test]
        public void CanStoreAndRetrieveSingleValue()
        {
            map.Store("A", () => 2);
            var result = map.CalculateAndRetrieve();

            Assert.AreEqual(1, result.Keys.Count);
            Assert.AreEqual("A", result.Keys.Single());
            Assert.AreEqual(2, result["A"]);
        }

        [Test]
        public void CanStoreAndRetrieveMultipleValues()
        {
            map.Store("A", () => 1);
            map.Store("B", () => 2);
            var result = map.CalculateAndRetrieve();
            Assert.AreEqual(2, result.Keys.Count);
            Assert.AreEqual(1, result["A"]);
            Assert.AreEqual(2, result["B"]);
        }

        [Test]
        public void LaterValuesOverwriteEarlierValues()
        {
            map.Store("A", () => 1);
            map.Store("A", () => 2);
            var result = map.CalculateAndRetrieve();
            Assert.AreEqual(1, result.Keys.Count);
            Assert.AreEqual(2, result["A"]);
        }

        [Test]
        public void WorkIsDeferred()
        {
            const string errorMessage = "The work was not deferred, it was calculated on storage";
            // Note simultaneous assignment and return in these delegates

            int value = 0;
            map.Store("A", () => value = 1);
            Assert.AreEqual(0, value, errorMessage);
            map.Store("A", () => value = 2);
            Assert.AreEqual(0, value, errorMessage);
            var result = map.CalculateAndRetrieve();
            
            Assert.AreEqual(2, value, @"If this value is 1, then the original delegate was never overwritten.
If it is 3, then when we invoked calculation for container 1, container 2 was invoked as well/instead");
            Assert.AreEqual(2, result["A"]);
        }
    }
}
