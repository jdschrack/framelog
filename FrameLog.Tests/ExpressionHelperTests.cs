using FrameLog.Helpers;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using FrameLog.Exceptions;

namespace FrameLog.Tests
{
    public class ExpressionHelperTests
    {
        [Test]
        public void CanGetPropertyNameForSimplePropertyLookup()
        {
            Expression<Func<Helper, Helper>> expr = h => h.Property;
            Assert.AreEqual("Property", expr.GetPropertyName());
        }
        [Test]
        public void CanGetPropertyNamesForPropertyLookupChain()
        {
            Expression<Func<Helper, Helper>> expr = h => h.Property.Property;
            Assert.AreEqual("Property.Property", expr.GetPropertyName());
        }
        [Test]
        public void ExceptionIsThrownForInvalidExpression()
        {
            Expression<Func<Helper, Helper>> expr = h => h.Method();
            Assert.Throws<InvalidPropertyExpressionException>(() => expr.GetPropertyName());
        }

        private class Helper
        {
            public Helper Property { get; set; }
            public Helper Method() { throw new NotImplementedException(); }
        }
    }
}
