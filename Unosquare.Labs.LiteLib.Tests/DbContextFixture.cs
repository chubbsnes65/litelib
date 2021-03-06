﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Labs.LiteLib.Tests.Database;
using System.Data.SQLite;

namespace Unosquare.Labs.LiteLib.Tests
{
    /// <summary>
    /// A TestFixture to test the included methods in LiteDbSet
    /// </summary>
    [TestFixture]
    public class DbContextFixture
    {
        /// <summary>
        /// The data source for all Test
        /// </summary>
        private readonly Order[] dataSource =
        {
             new Order { CustomerName = "John", ShipperCity = "Guadalajara", Amount = 4 },
             new Order { CustomerName = "Peter", ShipperCity = "Leon", Amount = 6 },
             new Order { CustomerName = "Margarita", ShipperCity = "Boston", Amount = 7}
        };

        /// <summary>
        /// Tests the select all.
        /// </summary>
        [Test]
        public void TestSelectAll()
        {
            using (var context = new TestDbContext(nameof(TestSelectAll)))
            {
                foreach (var item in dataSource)
                {
                    context.Orders.Insert(item);
                }

                var list = context.Orders.SelectAll();
                Assert.AreEqual(dataSource.Count(), list.Count(), "Same set");
            }
        }


        /// <summary>
        /// Tests the delete data.
        /// </summary>
        [Test]
        public void TestDeleteData()
        {
            using (var context = new TestDbContext(nameof(TestDeleteData)))
            {

                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        context.Orders.Insert(item);
                    }
                }

                var incomingData = context.Orders.SelectAll();
                foreach (var item in incomingData)
                {
                    context.Orders.Delete(item);
                }
                Assert.AreEqual(0, context.Orders.Count());
            }
        }

        /// <summary>
        /// Tests the insert data.
        /// </summary>
        [Test]
        public void TestInsertData()
        {
            using (var _context = new TestDbContext(nameof(TestInsertData)))
            {
                while (_context.Orders.Count() != 0)
                {
                    var incomingData = _context.Orders.SelectAll();
                    foreach (var item in incomingData)
                    {
                        _context.Orders.Delete(item);
                    }
                }

                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                var list = _context.Orders.SelectAll();
                Assert.AreEqual(dataSource.Count(), list.Count());
            }

        }


        /// <summary>
        /// Tests the update data.
        /// </summary>
        [Test]
        public void TestUpdateData()
        {
            using (var _context = new TestDbContext(nameof(TestUpdateData)))
            {
                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                var list = _context.Orders.Select("CustomerName = @CustomerName", new { CustomerName = "John" });
                foreach (var item in list)
                {
                    item.ShipperCity = "Atlanta";
                    _context.Orders.Update(item);
                }

                var updatedList = _context.Orders.Select("ShipperCity = @ShipperCity", new { ShipperCity = "Atlanta" });
                foreach (var item in updatedList)
                {
                    Assert.AreEqual("Atlanta", item.ShipperCity);
                }
            }
        }


        /// <summary>
        /// Tests the select data.
        /// </summary>
        [Test]
        public void TestSelectData()
        {
            using (var _context = new TestDbContext(nameof(TestSelectData)))
            {
                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                var selectingData = _context.Orders.Select("CustomerName = @CustomerName", new { CustomerName = "Peter" });
                foreach (var item in selectingData)
                {
                    Assert.AreEqual("Peter", item.CustomerName);
                }
            }
        }


        /// <summary>
        /// Tests the count data.
        /// </summary>
        [Test]
        public void TestCountData()
        {
            using (var _context = new TestDbContext(nameof(TestCountData)))
            {
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        _context.Orders.Insert(item);
                    }
                }

                var selectingData = _context.Orders.Select("CustomerName = @CustomerName", new { CustomerName = "Margarita" });
                Assert.AreEqual(10, selectingData.Count());
            }
        }

        /// <summary>
        /// Tests the single data.
        /// </summary>
        [Test]
        public void TestSingleData()
        {
            using (var _cotext = new TestDbContext(nameof(TestSingleData)))
            {
                var k = 0;
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        item.UniqueId = (k++).ToString();
                        _cotext.Orders.Insert(item);
                    }
                }

                var singleSelect = _cotext.Orders.Single(3);
                Assert.AreEqual("Margarita", singleSelect.CustomerName);
            }
        }

        /// <summary>
        /// Tests the query data.
        /// </summary>
        [Test]
        public void TestQueryData()
        {
            using (var _context = new TestDbContext(nameof(TestQueryData)))
            {
                var k = 0;
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        item.UniqueId = (k++).ToString();
                        _context.Orders.Insert(item);
                    }
                }

                var selectedData = _context.Orders.Query($"{_context.Orders.SelectDefinition} WHERE CustomerName = @CustomerName", new Order { CustomerName = "Margarita" });
                foreach (var item in selectedData)
                {
                    Assert.IsTrue(item.CustomerName == "Margarita");
                }
            }
        }

        /// <summary>
        /// Called when [before insert test].
        /// </summary>
        [Test]
        public void OnBeforeInsertTest()
        {
            using (var _context = new TestDbContext(nameof(OnBeforeInsertTest)))
            {
                _context.Orders.OnBeforeInsert += (s, e) =>
                {
                    if (e.Entity.CustomerName == "Peter")
                    {
                        e.Entity.CustomerName = "Charles";
                    }
                };

                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                var updatedList = _context.Orders.Select("ShipperCity = @ShipperCity", new { ShipperCity = "Leon" });
                foreach (var item in updatedList)
                {
                    Assert.AreNotEqual("Peter", item.CustomerName);
                }
            }
        }

        /// <summary>
        /// Called when [after insert].
        /// </summary>
        [Test]
        public void OnAfterInsert()
        {
            using (var _context = new TestDbContext(nameof(OnAfterInsert)))
            {
                _context.Orders.OnAfterInsert += (s, e) =>
                {
                    if (e.Entity.CustomerName == "John" || e.Entity.CustomerName == "Peter")
                    {
                        _context.Orders.Delete(e.Entity);
                    }
                };

                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        _context.Orders.Insert(item);
                    }
                }

                var afterList = _context.Orders.SelectAll();
                foreach (var item in afterList)
                {
                    Assert.AreEqual("Margarita", item.CustomerName);
                }
                Assert.AreEqual(10, afterList.Count());
            }
        }

        /// <summary>
        /// Called when [before update test].
        /// </summary>
        [Test]
        public void OnBeforeUpdateTest()
        {
            using (var _context = new TestDbContext(nameof(OnBeforeUpdateTest)))
            {
                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                _context.Orders.OnBeforeUpdate += (s, e) =>
                {
                    if (e.Entity.ShipperCity == "Leon")
                    {
                        e.Entity.ShipperCity = "Atlanta";
                    }
                };

                foreach (var item in _context.Orders.SelectAll())
                {
                    _context.Orders.Update(item);
                }

                var updatedList = _context.Orders.Select("CustomerName = @CustomerName", new { CustomerName = "Peter" });
                foreach (var item in updatedList)
                {
                    Assert.AreEqual("Atlanta", item.ShipperCity);
                }
            }
        }

        /// <summary>
        /// Called when [after update test].
        /// </summary>
        [Test]
        public void OnAfterUpdateTest()
        {
            using (var _context = new TestDbContext(nameof(OnAfterUpdateTest)))
            {
                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                var newDataSource = new List<Order>();
                _context.Orders.OnAfterUpdate += (s, e) =>
                {
                    if (e.Entity.ShipperCity == "Guadalajara")
                    {
                        newDataSource.Add(e.Entity);
                    }
                };

                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in _context.Orders.SelectAll())
                    {
                        _context.Orders.Update(item);
                    }
                }

                Assert.AreEqual(10, newDataSource.Count());
            }
        }

        /// <summary>
        /// Called when [before delete test].
        /// </summary>
        [Test]
        public void OnBeforeDeleteTest()
        {
            using (var _context = new TestDbContext(nameof(OnBeforeDeleteTest)))
            {
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        _context.Orders.Insert(item);
                    }
                }

                var deletedList = new List<Order>();
                _context.Orders.OnBeforeDelete += (s, e) =>
                {
                    deletedList.Add(e.Entity);
                };

                foreach (var item in _context.Orders.SelectAll())
                {
                    if (item.CustomerName == "John")
                    {
                        _context.Orders.Delete(item);
                    }
                }

                Assert.AreEqual(10, deletedList.Count());
            }
        }

        //Test OnAfterDelete
        [Test]
        public void OnAfterDeleteTest()
        {
            using (var _context = new TestDbContext(nameof(OnAfterDeleteTest)))
            {
                foreach (var item in dataSource)
                {
                    _context.Orders.Insert(item);
                }

                _context.Orders.OnAfterDelete += (s, e) =>
                {
                    e.Entity.CustomerName = "Jessy";
                    _context.Orders.Insert(e.Entity);
                };

                foreach (var item in _context.Orders.SelectAll())
                {
                    if (item.CustomerName == "Margarita")
                    {
                        _context.Orders.Delete(item);
                    }
                }

                foreach (var item in _context.Orders.Select("CustomerName = @CustomerName", new { CustomerName = "Jessy" }))
                {
                    Assert.AreEqual("Jessy", item.CustomerName);
                }
            }
        }

        /// <summary>
        /// Asynchronouses the test select all.
        /// </summary>
        [Test]
        public async void AsyncTestSelectAll()
        {
            using (var context = new TestDbContext(nameof(AsyncTestSelectAll)))
            {
                foreach (var item in dataSource)
                {
                    await context.Orders.InsertAsync(item);
                }

                var list = await context.Orders.SelectAllAsync();
                Assert.AreEqual(dataSource.Count(), list.Count(), "Same set");
            }
        }

        /// <summary>
        /// Asynchronouses the test delete data.
        /// </summary>
        [Test]
        public async void AsyncTestDeleteData()
        {
            using (var context = new TestDbContext(nameof(AsyncTestDeleteData)))
            {
                foreach (var item in dataSource)
                {
                    await context.Orders.InsertAsync(item);
                }

                var incomingData = context.Orders.SelectAll();
                foreach (var item in incomingData)
                {
                    await context.Orders.DeleteAsync(item);
                }
                Assert.AreEqual(0, context.Orders.Count());
            }
        }

        /// <summary>
        /// Asynchronouses the test insert data.
        /// </summary>
        [Test]
        public async void AsyncTestInsertData()
        {
            using (var _context = new TestDbContext(nameof(AsyncTestInsertData)))
            {
                while (_context.Orders.Count() != 0)
                {
                    var incomingData = _context.Orders.SelectAll();
                    foreach (var item in incomingData)
                    {
                        await _context.Orders.DeleteAsync(item);
                    }
                }

                foreach (var item in dataSource)
                {
                    await _context.Orders.InsertAsync(item);
                }
                var list = await _context.Orders.SelectAllAsync();
                Assert.AreEqual(dataSource.Count(), list.Count());
            }

        }

        /// <summary>
        /// Asynchronouses the test update data.
        /// </summary>
        [Test]
        public async void AsyncTestUpdateData()
        {
            using (var _context = new TestDbContext(nameof(AsyncTestUpdateData)))
            {
                foreach (var item in dataSource)
                {
                    await _context.Orders.InsertAsync(item);
                }

                var list = await _context.Orders.SelectAsync("CustomerName = @CustomerName", new { CustomerName = "John" });
                foreach (var item in list)
                {
                    item.ShipperCity = "Atlanta";
                    await _context.Orders.UpdateAsync(item);
                }

                var updatedList = await _context.Orders.SelectAsync("ShipperCity = @ShipperCity", new { ShipperCity = "Atlanta" });
                foreach (var item in updatedList)
                {
                    Assert.AreEqual("Atlanta", item.ShipperCity);
                }
            }
        }

        /// <summary>
        /// Asynchronouses the test select data.
        /// </summary>
        [Test]
        public async void AsyncTestSelectData()
        {
            using (var _context = new TestDbContext(nameof(AsyncTestSelectData)))
            {
                foreach (var item in dataSource)
                {
                    await _context.Orders.InsertAsync(item);
                }
                // Selecting Data By name
                var selectingData = await _context.Orders.SelectAsync("CustomerName = @CustomerName", new { CustomerName = "Peter" });

                foreach (var item in selectingData)
                {
                    Assert.AreEqual("Peter", item.CustomerName);
                }
            }
        }

        /// <summary>
        /// Asynchronouses the test count data.
        /// </summary>
        [Test]
        public async void AsyncTestCountData()
        {
            using (var _context = new TestDbContext(nameof(AsyncTestCountData)))
            {
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        await _context.Orders.InsertAsync(item);
                    }
                }

                var selectingData = await _context.Orders.SelectAsync("CustomerName = @CustomerName", new { CustomerName = "Peter" });
                Assert.AreEqual(10, selectingData.Count());
            }
        }

        /// <summary>
        /// Asynchronouses the test single data.
        /// </summary>
        [Test]
        public async void AsyncTestSingleData()
        {
            using (var _cotext = new TestDbContext(nameof(AsyncTestSingleData)))
            {
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        await _cotext.Orders.InsertAsync(item);
                    }
                }
                var singleSelect = await _cotext.Orders.SingleAsync(3);
                Assert.AreEqual("Margarita", singleSelect.CustomerName);
            }
        }

        /// <summary>
        /// Asynchronouses the test query data.
        /// </summary>
        [Test]
        public async void AsyncTestQueryData()
        {
            using (var _context = new TestDbContext(nameof(AsyncTestQueryData)))
            {
                for (var i = 0; i < 10; i++)
                {
                    foreach (var item in dataSource)
                    {
                        await _context.Orders.InsertAsync(item);
                    }
                }

                var selectedData = await _context.Orders.QueryAsync($"{_context.Orders.SelectDefinition} WHERE CustomerName = @CustomerName", new Order { CustomerName = "John" });
                foreach (var item in selectedData)
                {
                    Assert.IsTrue(item.CustomerName == "John");
                }
            }
        }

        /// <summary>
        /// Tests the entity unique.
        /// </summary>
        [Test]
        public void TestEntityUnique()
        {
            using (var _context = new TestDbContext(nameof(TestEntityUnique)))
            {
                for (var i = 0; i < 3; i++)
                {
                    var id = 1 + i;
                    dataSource[i].UniqueId = id.ToString();
                    _context.Orders.Insert(dataSource[i]);
                }
                Assert.Throws<SQLiteException>(delegate ()
                {
                    var newOrder = new Order { CustomerName = "John", Amount = 2, ShipperCity = "Atlanta", UniqueId = "1" };
                    _context.Orders.Insert(newOrder);
                });
            }
        }

        /// <summary>
        /// Tests the string lengt.
        /// </summary>
        [Test]
        public void TestStringLengt()
        {
            using (var _context = new TestDbContext(nameof(TestStringLengt)))
            {
                Assert.Throws<SQLiteException>(delegate ()
                {
                    var newOrder = new Order { CustomerName = "John", Amount = 2, ShipperCity = "StringStringStringStringStringStringStringString" };
                    _context.Orders.Insert(newOrder);
                    Console.Write(_context.Orders.Count());
                });
            }
        }
    }
}
