﻿using System.Collections.Generic;
using Lephone.Data;
using Lephone.Data.Common;
using NUnit.Framework;

namespace Lephone.UnitTest.Data
{
    [TestFixture]
    public class PagedSelectorTest : DataTestBase
    {
        [Test]
        public void TestPagedSelecor()
        {
            var ps = new PagedSelector<SinglePerson>(null, new OrderBy((DESC)"Id"), 2, DbEntry.Context);
            Assert.AreEqual(3, ps.GetResultCount());
            var ls = (List<SinglePerson>)ps.GetCurrentPage(0);
            Assert.AreEqual(2, ls.Count);
            Assert.AreEqual("Mike", ls[0].Name);
            Assert.AreEqual("Jerry", ls[1].Name);
            ls = (List<SinglePerson>)ps.GetCurrentPage(1);
            Assert.AreEqual(1, ls.Count);
            Assert.AreEqual("Tom", ls[0].Name);
        }

        [Test]
        public void TestPagedSelecor2()
        {
            var ps = new PagedSelector<SinglePerson>(null, new OrderBy((DESC)"Id"), 3, DbEntry.Context);
            Assert.AreEqual(3, ps.GetResultCount());
            var ls = (List<SinglePerson>)ps.GetCurrentPage(0);
            Assert.AreEqual(3, ls.Count);
            Assert.AreEqual("Mike", ls[0].Name);
            Assert.AreEqual("Jerry", ls[1].Name);
            Assert.AreEqual("Tom", ls[2].Name);
        }

        [Test]
        public void TestStaticPagedSelecor()
        {
            var ps = new StaticPagedSelector<SinglePerson>(null, new OrderBy((DESC)"Id"), 2, DbEntry.Context);
            Assert.AreEqual(3, ps.GetResultCount());
            var ls = (List<SinglePerson>)ps.GetCurrentPage(1);
            Assert.AreEqual(1, ls.Count);
            Assert.AreEqual("Mike", ls[0].Name);
            ls = (List<SinglePerson>)ps.GetCurrentPage(0);
            Assert.AreEqual(2, ls.Count);
            Assert.AreEqual("Jerry", ls[0].Name);
            Assert.AreEqual("Tom", ls[1].Name);
        }

        [Test]
        public void TestStaticPagedSelecor2()
        {
            IPagedSelector ps = DbEntry.From<SinglePerson>().Where(Condition.Empty).OrderBy((DESC)"Id").PageSize(3).GetStaticPagedSelector();
            Assert.AreEqual(3, ps.GetResultCount());
            var ls = (List<SinglePerson>)ps.GetCurrentPage(0);
            Assert.AreEqual(3, ls.Count);
            Assert.AreEqual("Mike", ls[0].Name);
            Assert.AreEqual("Jerry", ls[1].Name);
            Assert.AreEqual("Tom", ls[2].Name);
        }
    }
}
