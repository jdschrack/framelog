using FrameLog.Logging;
using NUnit.Framework;

namespace FrameLog.Tests
{
    public class ChangeTypeTests
    {
        [Test]
        public void TypeEqualityIsRespected()
        {
            var parent = new Parent().GetChangeType();
            var child = new Child().GetChangeType();
            Assert.IsTrue(parent.IsA(typeof(Parent)));
            Assert.IsTrue(child.IsA(typeof(Child)));
        }

        [Test]
        public void InheritanceIsRespected()
        {
            var parent = new Parent().GetChangeType();
            var child = new Child().GetChangeType();
            Assert.IsFalse(parent.IsA(typeof(Child)));
            Assert.IsTrue(child.IsA(typeof(Parent)));
        }

        [Test]
        public void UnknownTypeIsNotAnything()
        {
            var type = ((Child)null).GetChangeType();
            Assert.IsFalse(type.IsA(typeof(Child)));
            Assert.IsFalse(type.IsA(typeof(Parent)));
        }

        private class Parent { }
        private class Child : Parent { }
    }
}
