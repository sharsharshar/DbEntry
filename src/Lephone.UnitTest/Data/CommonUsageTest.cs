using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lephone.Data;
using Lephone.Data.Common;
using Lephone.Data.Definition;
using Lephone.Data.SqlEntry;
using Lephone.MockSql.Recorder;
using Lephone.UnitTest.Data.CreateTable;
using Lephone.UnitTest.Data.Objects;
using Lephone.Util.Logging;
using NUnit.Framework;

namespace Lephone.UnitTest.Data
{
    #region Objects

    [DbTable("People")]
    public abstract class SavePeople : DbObjectModel<SavePeople>
    {
        public abstract string Name { get; set; }

        public abstract SavePeople Init(string name);
    }


    [DbTable("File")]
    public class DistinctTest : IDbObject
    {
        [DbColumn("BelongsTo_Id")] public int n;
    }

    public abstract class NotDefineRelation : DbObjectModel<NotDefineRelation>
    {
        public abstract AllowNullOnValueType Relation { get; set; }
    }

    public abstract class AllowNullOnValueType : DbObjectModel<AllowNullOnValueType>
    {
        [AllowNull]
        public abstract int Age { get; set; }
    }

    public abstract class t_user : DbObjectModel<t_user>
    {
        [Length(40)]
        public abstract string mc { get; set; }

        [SpecialName, LazyLoad]
        public abstract DateTime CreatedOn { get; set; }
    }

    [DbTable("People")]
    public class SinglePerson : DbObject
    {
        public string Name;
    }

    [DbTable("People")]
    public abstract class UniquePerson : DbObjectModel<UniquePerson>
    {
        [Index(UNIQUE = true)]
        public abstract string Name { get; set; }
    }

    public class CountTable : DbObject
    {
        [SpecialName]
        public int Count;
    }

    [DbContext("SqlServerMock")]
    public class CountTableSql : DbObject
    {
        [SpecialName]
        public int Count;
    }

    public abstract class CountTable2 : DbObjectModel<CountTable2>
    {
        public abstract string Name { get; set; }

        [SpecialName]
        public abstract int Count { get; set; }
    }

    [DbContext("SqlServerMock")]
    public abstract class CountTable2Sql : DbObjectModel<CountTable2Sql>
    {
        public abstract string Name { get; set; }

        [SpecialName]
        public abstract int Count { get; set; }
    }

    [DbTable("People")]
    public abstract class FieldPerson : DbObjectModel<FieldPerson>
    {
        [DbColumn("Name")]
        public abstract string theName { get; set; }

        public static FieldPerson FindByName(string name)
        {
            return FindOne(p => p.theName == name);
        }
    }

    [DbTable("People"), DbContext("SqlServerMock")]
    public abstract class FieldPersonSql : DbObjectModel<FieldPersonSql>
    {
        [DbColumn("Name")]
        public abstract string theName { get; set; }

        public static FieldPersonSql FindByName(string name)
        {
            return FindOne(p => p.theName == name);
        }
    }

    [DbTable("LockVersionTest")]
    public abstract class LockVersionTest : DbObjectModel<LockVersionTest>
    {
        public abstract string Name { get; set; }

        [SpecialName]
        public abstract int LockVersion { get; set; }
    }

    public class MKEY : IDbObject
    {
        [DbKey(IsDbGenerate = false)]
        public string FirstName;

        [DbKey(IsDbGenerate = false)]
        public string LastName;

        public int Age;
    }

    public abstract class InitTest : DbObjectModel<InitTest>
    {
        public abstract string Name { get; set; }
        public abstract int Age { get; set; }

        public abstract InitTest Init(string name, int age); // Ignore case
    }

    public abstract class InitTest2 : DbObjectModel<InitTest2>
    {
        public abstract string Name { get; set; }
        public abstract string FirstName { get; set; }
        public abstract string LastName { get; set; }

        public abstract InitTest2 Init(string Name, string FirstName, string LastName);
    }

    public abstract class InitTest3 : DbObjectModel<InitTest3>
    {
        public abstract string Name { get; set; }
        public abstract string FirstName { get; set; }
        public abstract string LastName { get; set; }

        public abstract InitTest3 Init(string Name, string LastName, string FirstName);
    }

    public abstract class InitTest4 : DbObjectModel<InitTest4>
    {
        public abstract string Name { get; set; }
        public abstract bool Gender { get; set; }
        public abstract int? Age { get; set; }

        public abstract InitTest4 Initialize(string Name, bool Gender, int? Age);
        public abstract InitTest4 Initialize(InitTest4 obj);
    }

    #endregion

    [TestFixture]
    public class CommonUsageTest : DataTestBase
    {
        [Test]
        public void Test1()
        {
            var p = new SinglePerson {Name = "abc"};
            Assert.AreEqual(0, p.Id);

            DbEntry.Save(p);
            Assert.IsTrue(0 != p.Id);
            var p1 = DbEntry.GetObject<SinglePerson>(p.Id);
            Assert.AreEqual(p.Name, p1.Name);

            p.Name = "xyz";
            DbEntry.Save(p);
            Assert.AreEqual(p.Id, p1.Id);

            p1 = DbEntry.GetObject<SinglePerson>(p.Id);
            Assert.AreEqual("xyz", p1.Name);

            long id = p.Id;
            DbEntry.Delete(p);
            Assert.AreEqual(0, p.Id);
            p1 = DbEntry.GetObject<SinglePerson>(id);
            Assert.IsNull(p1);
        }

        [Test]
        public void Test2()
        {
            List<SinglePerson> l = DbEntry
                .From<SinglePerson>()
                .Where(Condition.Empty)
                .OrderBy("Id")
                .Range(1, 1)
                .Select();

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(1, l[0].Id);
            Assert.AreEqual("Tom", l[0].Name);

            l = DbEntry
                .From<SinglePerson>()
                .Where(Condition.Empty)
                .OrderBy("Id")
                .Range(2, 2)
                .Select();

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(2, l[0].Id);
            Assert.AreEqual("Jerry", l[0].Name);

            l = DbEntry
                .From<SinglePerson>()
                .Where(Condition.Empty)
                .OrderBy("Id")
                .Range(3, 5)
                .Select();

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(3, l[0].Id);
            Assert.AreEqual("Mike", l[0].Name);

            l = DbEntry
                .From<SinglePerson>()
                .Where(Condition.Empty)
                .OrderBy((DESC)"Id")
                .Range(3, 5)
                .Select();

            Assert.AreEqual(1, l.Count);
            Assert.AreEqual(1, l[0].Id);
            Assert.AreEqual("Tom", l[0].Name);
        }

        [Test]
        public void Test3()
        {
            Assert.AreEqual(3, DbEntry.From<Category>().Where(Condition.Empty).GetCount());
            Assert.AreEqual(5, DbEntry.From<Book>().Where(Condition.Empty).GetCount());
            Assert.AreEqual(2, DbEntry.From<Book>().Where(CK.K["Category_Id"] == 3).GetCount());
        }

        [Test]
        public void Test4()
        {
            List<GroupByObject<long>> l = DbEntry
                .From<Book>()
                .Where(Condition.Empty)
                .OrderBy((DESC)DbEntry.CountColumn)
                .GroupBy<long>("Category_Id");

            Assert.AreEqual(2, l[0].Column);
            Assert.AreEqual(3, l[0].Count);

            Assert.AreEqual(3, l[1].Column);
            Assert.AreEqual(2, l[1].Count);
        }

        [Test]
        public void Test5()
        {
            IList l = DbEntry
                .From<Book>()
                .Where(Condition.Empty)
                .GroupBy<string>("Name");

            Assert.AreEqual(5, l.Count);

            l = DbEntry
                .From<Book>()
                .Where(CK.K["Id"] > 2)
                .GroupBy<string>("Name");

            Assert.AreEqual(3, l.Count);

            List<GroupByObject<string>> ll = DbEntry
                .From<Book>()
                .Where(CK.K["Id"] > 2)
                .OrderBy("Name")
                .GroupBy<string>("Name");

            Assert.AreEqual(3, ll.Count);
            Assert.AreEqual("Pal95", ll[0].Column);
            Assert.AreEqual("Shanghai", ll[1].Column);
            Assert.AreEqual("Wow", ll[2].Column);
        }

        [Test]
        public void TestPeopleModel()
        {
            List<PeopleModel> l = PeopleModel.FindAll();
            Assert.AreEqual(3, l.Count);
            Assert.AreEqual("Tom", l[0].Name);

            PeopleModel p = PeopleModel.FindByName("Jerry");
            Assert.AreEqual(2, p.Id);
            Assert.IsTrue(p.IsValid());

            p.Name = "llf";
            Assert.IsTrue(p.IsValid());
            p.Save();

            PeopleModel p1 = PeopleModel.FindById(2);
            Assert.AreEqual("llf", p1.Name);

            p.Delete();
            p1 = PeopleModel.FindById(2);
            Assert.IsNull(p1);

            p = PeopleModel.New;
            p.Name = "123456";
            Assert.IsFalse(p.IsValid());

            Assert.AreEqual(1, PeopleModel.CountName("Tom"));
            Assert.AreEqual(0, PeopleModel.CountName("xyz"));
        }

        [Test]
        public void TestSql()
        {
            PeopleModel p1 = DbEntry.Context.ExecuteList<PeopleModel>("Select [Id],[Name] From [People] Where [Id] = 2")[0];
            Assert.AreEqual("Jerry", p1.Name);
            p1 = DbEntry.Context.ExecuteList<PeopleModel>(new SqlStatement("Select [Name],[Id] From [People] Where [Id] = 1"))[0];
            Assert.AreEqual("Tom", p1.Name);
            p1 = PeopleModel.FindBySql("Select [Id],[Name] From [People] Where [Id] = 2")[0];
            Assert.AreEqual("Jerry", p1.Name);
            p1 = PeopleModel.FindBySql(new SqlStatement("Select [Name],[Id] From [People] Where [Id] = 3"))[0];
            Assert.AreEqual("Mike", p1.Name);
        }

        [Test]
        public void ToStringTest()
        {
            var p = new ImpPeople {Name = "tom"};
            Assert.AreEqual("{ Id = 0, Name = tom }", p.ToString());

            DArticle a = DArticle.New;
            a.Name = "long";
            Assert.AreEqual("{ Id = 0, Name = long }", a.ToString());

            var c = new ImpPCs {Name = "HP"};
            Assert.AreEqual("{ Id = 0, Name = HP, Person_Id = 0 }", c.ToString());
        }

        [Test]
        public void TestColumnCompColumn()
        {
            //Condition c = CK.K["Age"] > CK.K["Count"];
            var c = CK.K["Age"].Gt(CK.K["Count"]);
            var dpc = new DataParameterCollection();
            string s = c.ToSqlText(dpc, DbEntry.Context.Dialect);
            Assert.AreEqual(0, dpc.Count);
            Assert.AreEqual("[Age] > [Count]", s);
        }

        [Test]
        public void TestColumnCompColumn2()
        {
            var c = CK.K["Age"] > CK.K["Count"];
            var dpc = new DataParameterCollection();
            string s = c.ToSqlText(dpc, DbEntry.Context.Dialect);
            Assert.AreEqual(0, dpc.Count);
            Assert.AreEqual("[Age] > [Count]", s);
        }

        [Test]
        public void TestColumnCompColumn3()
        {
            var c = CK.K["Age"] > CK.K["Count"] && CK.K["Name"] == CK.K["theName"] || CK.K["Age"] <= CK.K["Num"];
            var dpc = new DataParameterCollection();
            string s = c.ToSqlText(dpc, DbEntry.Context.Dialect);
            Assert.AreEqual(0, dpc.Count);
            Assert.AreEqual("(([Age] > [Count]) AND ([Name] = [theName])) OR ([Age] <= [Num])", s);
        }

        [Test]
        public void TestGetSqlStetement()
        {
            SqlStatement sql = DbEntry.Context.GetSqlStatement("SELECT * FROM User WHERE Age > ? AND Age < ?", 18, 23);
            Assert.AreEqual("SELECT * FROM User WHERE Age > @p0 AND Age < @p1", sql.SqlCommandText);
            Assert.AreEqual("@p0", sql.Parameters[0].Key);
            Assert.AreEqual(18, sql.Parameters[0].Value);
            Assert.AreEqual("@p1", sql.Parameters[1].Key);
            Assert.AreEqual(23, sql.Parameters[1].Value);
        }

        [Test]
        public void TestGetSqlStetement2()
        {
            SqlStatement sql = DbEntry.Context.GetSqlStatement("SELECT * FROM User WHERE Id = ? Name LIKE '%?%' Age > ? AND Age < ? ", 1, 18, 23);
            Assert.AreEqual("SELECT * FROM User WHERE Id = @p0 Name LIKE '%?%' Age > @p1 AND Age < @p2 ", sql.SqlCommandText);
            Assert.AreEqual("@p0", sql.Parameters[0].Key);
            Assert.AreEqual(1, sql.Parameters[0].Value);
            Assert.AreEqual("@p1", sql.Parameters[1].Key);
            Assert.AreEqual(18, sql.Parameters[1].Value);
            Assert.AreEqual("@p2", sql.Parameters[2].Key);
            Assert.AreEqual(23, sql.Parameters[2].Value);
        }

        [Test]
        public void TestGetSqlStetementByExecuteList()
        {
            List<Person> ls = DbEntry.Context.ExecuteList<Person>("SELECT * FROM [People] WHERE Id > ? AND Id < ?", 1, 3);
            Assert.AreEqual(1, ls.Count);
            Assert.AreEqual("Jerry", ls[0].Name);
        }

        [Test]
        public void TestGuidKey()
        {
            GuidKey o = GuidKey.New;
            Assert.IsTrue(Guid.Empty == o.Id);

            o.Name = "guid";
            o.Save();

            Assert.IsFalse(Guid.Empty == o.Id);

            GuidKey o1 = GuidKey.FindById(o.Id);
            Assert.AreEqual("guid", o1.Name);

            o.Name = "test";
            o.Save();

            GuidKey o2 = GuidKey.FindById(o.Id);
            Assert.AreEqual("test", o2.Name);

            o2.Delete();
            GuidKey o3 = GuidKey.FindById(o.Id);
            Assert.IsNull(o3);
        }

        [Test]
        public void TestGuidColumn()
        {
            var g = Guid.NewGuid();
            var o = GuidColumn.New.Init(g);
            o.Save();

            var o1 = GuidColumn.FindById(o.Id);
            Assert.IsNotNull(o1);
            Assert.AreEqual(g, o1.TheGuid);

            var g1 = Guid.NewGuid();
            o1.TheGuid = g1;
            o1.Save();

            Assert.IsFalse(g == g1);

            var o2 = GuidColumn.FindById(o.Id);
            Assert.IsTrue(g1 == o2.TheGuid);
        }

        [Test]
        public void TestUniqueValidate()
        {
            var u = UniquePerson.New;
            u.Name = "test";
            var vh = new ValidateHandler();
            vh.ValidateObject(u);
            Assert.IsTrue(vh.IsValid);

            u.Name = "Tom";
            vh = new ValidateHandler();
            vh.ValidateObject(u);
            Assert.IsFalse(vh.IsValid);
            Assert.AreEqual("Invalid Field Name Should be UNIQUED.", vh.ErrorMessages["Name"]);

            // smart validate
            var p = DbEntry.GetObject<UniquePerson>(1);
            var n = ConsoleMessageRecorder.Count;
            Assert.IsTrue(p.IsValid());
            Assert.AreEqual(n, ConsoleMessageRecorder.Count);
            p.Name = "Jerry";
            Assert.IsFalse(p.IsValid());
            Assert.AreEqual(n + 1, ConsoleMessageRecorder.Count);
        }

        [Test]
        public void TestFindOneWithSqlServer2005()
        {
            var p = DbEntry.GetObject<Person>(CK.K["Name"] == "test", null);
            Assert.IsNull(p);
        }

        [Test]
        public void Test2ndPageWithSqlserver2005()
        {
            StaticRecorder.ClearMessages();
            DbEntry.From<PersonSql>().Where(CK.K["Age"] > 18).OrderBy("Id").Range(3, 5).Select();
            Assert.AreEqual("SELECT [Id],[Name] FROM (SELECT [Id],[Name], ROW_NUMBER() OVER ( ORDER BY [Id] ASC) AS __rownumber__ FROM [People]  WHERE [Age] > @Age_0) AS T WHERE T.__rownumber__ >= 3 AND T.__rownumber__ <= 5;\n<Text><60>(@Age_0=18:Int32)", StaticRecorder.LastMessage);
        }

        [Test]
        public void Test2ndPageWithSqlserver2005WithAlias()
        {
            StaticRecorder.ClearMessages();
            DbEntry.From<FieldPersonSql>().Where(CK.K["Age"] > 18).OrderBy("Id").Range(3, 5).Select();
            Assert.AreEqual("SELECT [Id],[theName] FROM (SELECT [Id],[Name] AS [theName], ROW_NUMBER() OVER ( ORDER BY [Id] ASC) AS __rownumber__ FROM [People]  WHERE [Age] > @Age_0) AS T WHERE T.__rownumber__ >= 3 AND T.__rownumber__ <= 5;\n<Text><60>(@Age_0=18:Int32)", StaticRecorder.LastMessage);
        }

        [Test]
        public void TestTableNameMapOfConfig()
        {
            ObjectInfo oi = ObjectInfo.GetInstance(typeof(Lephone.Data.Logging.LephoneLog));
            Assert.AreEqual("System_Log", oi.From.MainTableName);

            oi = ObjectInfo.GetInstance(typeof(LephoneEnum));
            Assert.AreEqual("Lephone_Enum", oi.From.MainTableName);
        }

        //[Test]
        //public void Test_CK_Field()
        //{
        //    var de = new DbContext("SqlServerMock");
        //    StaticRecorder.ClearMessages();
        //    de.From<PropertyClassWithDbColumn>().Where(CK<PropertyClassWithDbColumn>.Field["TheName"] == "tom").Select();
        //    Assert.AreEqual("SELECT [Id],[Name] AS [TheName] FROM [People] WHERE [Name] = @Name_0;\n<Text><60>(@Name_0=tom:String)", StaticRecorder.LastMessage);
        //}

        [Test]
        public void TestNull()
        {
            StaticRecorder.ClearMessages();
            DbEntry.From<PropertyClassWithDbColumnSql>().Where(CK.K["Name"] == null).Select();
            Assert.AreEqual("SELECT [Id],[Name] AS [TheName] FROM [People] WHERE [Name] IS NULL;\n<Text><60>()", StaticRecorder.LastMessage);
        }

        [Test]
        public void TestNotNull()
        {
            StaticRecorder.ClearMessages();
            DbEntry.From<PropertyClassWithDbColumnSql>().Where(CK.K["Name"] != null).Select();
            Assert.AreEqual("SELECT [Id],[Name] AS [TheName] FROM [People] WHERE [Name] IS NOT NULL;\n<Text><60>()", StaticRecorder.LastMessage);
        }

        [Test]
        public void TestCountTable()
        {
            StaticRecorder.ClearMessages();
            var ct = new CountTableSql {Id = 1};
            DbEntry.Save(ct);
            Assert.AreEqual("UPDATE [Count_Table_Sql] SET [Count]=[Count]+1  WHERE [Id] = @Id_0;\n<Text><30>(@Id_0=1:Int64)", StaticRecorder.LastMessage);
        }

        [Test]
        public void TestCountTable2()
        {
            StaticRecorder.ClearMessages();
            var ct = CountTable2Sql.New;
            ct.Id = 1;
            ct.Name = "tom";
            DbEntry.Save(ct);
            Assert.AreEqual("UPDATE [Count_Table2Sql] SET [Name]=@Name_0,[Count]=[Count]+1  WHERE [Id] = @Id_1;\n<Text><30>(@Name_0=tom:String,@Id_1=1:Int64)", StaticRecorder.LastMessage);
        }

        [Test]
        public void TestFieldNameMapper()
        {
            FieldPerson p = FieldPerson.FindByName("Jerry");
            Assert.AreEqual(2, p.Id);
        }

        [Test]
        public void TestLockVersion()
        {
            var item = LockVersionTest.FindById(1);
            Assert.AreEqual(1, item.LockVersion);
            item.Name = "jerry";
            item.Save();

            var item0 = LockVersionTest.FindById(1);
            Assert.AreEqual(2, item0.LockVersion);
        }

        [Test, ExpectedException(typeof(DataException))]
        public void TestLockVersionException()
        {
            var item = LockVersionTest.FindById(1);
            var item2 = LockVersionTest.FindById(1);

            item.Name = "jerry";
            item.Save();

            item2.Name = "mike";
            item2.Save();
        }

        [Test]
        public void TestDefineCrossTableName3()
        {
            var b = crxBook1.New;
            b.Name = "test";

            var c = crxCategory1.New;
            c.Name = "math";

            c.Books.Add(b);

            c.Save();

            var c1 = crxCategory1.FindById(c.Id);
            Assert.AreEqual("math", c1.Name);
            Assert.AreEqual(1, c1.Books.Count);
            Assert.AreEqual("test", c1.Books[0].Name);
        }

        [Test]
        public void TestMKEY()
        {
            DbEntry.Context.Create(typeof(MKEY));

            var p1 = new MKEY {FirstName = "test", LastName = "next", Age = 11};
            DbEntry.Insert(p1);

            var p2 = DbEntry.From<MKEY>().Where(p => p.FirstName == "test" && p.LastName == "next").Select()[0];
            Assert.AreEqual(11, p2.Age);

            p2.Age = 18;
            DbEntry.Update(p2);

            var p3 = DbEntry.From<MKEY>().Where(p => p.FirstName == "test" && p.LastName == "next").Select()[0];
            Assert.AreEqual(18, p3.Age);
        }

        [Test]
        public void TestLazyLoadCreatedOn()
        {
            try
            {
                var d = t_user.New;
                d.mc = "����";
                d.Save();
            }
            catch(DataException ex)
            {
                Assert.AreEqual("SpecialName colomn could not be LazyLoad", ex.Message);
            }
        }

        [Test]
        public void TestMKEYForUpdate()
        {
            var p = new MKEY { FirstName = "test", LastName = "next", Age = 11 };
            sqlite.Update(p);
            AssertSql(@"UPDATE [MKEY] SET [Age]=@Age_0  WHERE ([FirstName] = @FirstName_1) AND ([LastName] = @LastName_2);
<Text><30>(@Age_0=11:Int32,@FirstName_1=test:String,@LastName_2=next:String)");
        }

        [Test]
        public void TestLowerFunction()
        {
            sqlite.From<SinglePerson>().Where(CK.K["Name"].ToLower() == "tom").Select();
            AssertSql(@"SELECT [Id],[Name] FROM [People] WHERE LOWER([Name]) = @Name_0;
<Text><60>(@Name_0=tom:String)");
        }

        [Test]
        public void TestLowerForLike()
        {
            sqlite.From<SinglePerson>().Where(CK.K["Name"].ToLower().Like("%tom%")).Select();
            AssertSql(@"SELECT [Id],[Name] FROM [People] WHERE LOWER([Name]) LIKE @Name_0;
<Text><60>(@Name_0=%tom%:String)");
        }

        [Test]
        public void TestUpperFunction()
        {
            sqlite.From<SinglePerson>().Where(CK.K["Name"].ToUpper() == "tom").Select();
            AssertSql(@"SELECT [Id],[Name] FROM [People] WHERE UPPER([Name]) = @Name_0;
<Text><60>(@Name_0=tom:String)");
        }

        [Test]
        public void TestUpperForLike()
        {
            sqlite.From<SinglePerson>().Where(CK.K["Name"].ToUpper().Like("%tom%")).Select();
            AssertSql(@"SELECT [Id],[Name] FROM [People] WHERE UPPER([Name]) LIKE @Name_0;
<Text><60>(@Name_0=%tom%:String)");
        }

        [Test]
        public void TestMax()
        {
            sqlite.From<SinglePerson>().Where(Condition.Empty).GetMax("Id");
            AssertSql(@"SELECT MAX([Id]) AS [Id] FROM [People];
<Text><60>()");

            var n = DbEntry.From<SinglePerson>().Where(Condition.Empty).GetMax("Id");
            Assert.AreEqual(3, n);

            n = FieldPerson.GetMax(null, "Id");
            Assert.AreEqual(3, n);
        }

        [Test]
        public void TestMin()
        {
            sqlite.From<SinglePerson>().Where(Condition.Empty).GetMin("Id");
            AssertSql(@"SELECT MIN([Id]) AS [Id] FROM [People];
<Text><60>()");

            var n = DbEntry.From<SinglePerson>().Where(Condition.Empty).GetMin("Id");
            Assert.AreEqual(1, n);

            n = FieldPerson.GetMin(null, "Id");
            Assert.AreEqual(1, n);
        }

        [Test]
        public void TestMaxDate()
        {
            StaticRecorder.CurRow.Add(new RowInfo(new DateTime()));
            sqlite.From<DateAndTime>().Where(Condition.Empty).GetMaxDate("dtValue");
            AssertSql(@"SELECT MAX([dtValue]) AS [dtValue] FROM [DateAndTime];
<Text><60>()");

            var n = DbEntry.From<DateAndTime>().Where(Condition.Empty).GetMaxDate("dtValue");
            Assert.AreEqual(DateTime.Parse("2004-8-19 18:51:06"), n);

            n = DateAndTime.GetMaxDate(null, "dtValue");
            Assert.AreEqual(DateTime.Parse("2004-8-19 18:51:06"), n);
        }

        [Test]
        public void TestMinDate()
        {
            StaticRecorder.CurRow.Add(new RowInfo(new DateTime()));
            sqlite.From<DateAndTime>().Where(Condition.Empty).GetMinDate("dtValue");
            AssertSql(@"SELECT MIN([dtValue]) AS [dtValue] FROM [DateAndTime];
<Text><60>()");

            var n = DbEntry.From<DateAndTime>().Where(Condition.Empty).GetMinDate("dtValue");
            Assert.AreEqual(DateTime.Parse("2004-8-19 18:51:06"), n);

            n = DateAndTime.GetMinDate(null, "dtValue");
            Assert.AreEqual(DateTime.Parse("2004-8-19 18:51:06"), n);
        }

        [Test]
        public void TestSum()
        {
            sqlite.From<SinglePerson>().Where(Condition.Empty).GetSum("Id");
            AssertSql(@"SELECT SUM([Id]) AS [Id] FROM [People];
<Text><60>()");

            var n = DbEntry.From<SinglePerson>().Where(Condition.Empty).GetSum("Id");
            Assert.AreEqual(6, n);

            n = FieldPerson.GetSum(null, "Id");
            Assert.AreEqual(6, n);
        }

        [Test]
        public void TestAllowNowOnValueType()
        {
            try
            {
                ObjectInfo.GetInstance(typeof (AllowNullOnValueType));
                Assert.IsTrue(false);
            }
            catch (DataException ex)
            {
                Assert.AreEqual("Don't set AllowNull to a value type field, instead of to use nullable", ex.Message);
            }
        }

        [Test]
        public void TestNotDefineRelation()
        {
            try
            {
                ObjectInfo.GetInstance(typeof(NotDefineRelation));
                Assert.IsTrue(false);
            }
            catch (DataException ex)
            {
                Assert.AreEqual("The property 'Relation' should define as relation field and can not set lazy load attribute", ex.Message);
            }

        }

        [Test]
        public void TestDistinct()
        {
            var list = DbEntry.From<DistinctTest>().Where(Condition.Empty).OrderBy(p => p.n).SelectDistinct();
            Assert.AreEqual(9, list.Count);
            var exps = new[] {0, 1, 2, 3, 4, 9, 11, 15, 16};
            for(int i = 0; i < 9; i++)
            {
                Assert.AreEqual(exps[i], list[i].n);
            }
        }

        [Test]
        public void TestDistinctPagedSelector()
        {
            var query = DbEntry.From<DistinctTest>().Where(Condition.Empty).OrderBy(p => p.n).PageSize(3).GetDistinctPagedSelector();
            Assert.AreEqual(9, query.GetResultCount());
            var list = (List<DistinctTest>)query.GetCurrentPage(1);
            Assert.AreEqual(3, list.Count);
            var exps = new[] { 3, 4, 9 };
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(exps[i], list[i].n);
            }
        }

        [Test]
        public void TestWhereFunctionOfDbObjectModel()
        {
            var list = UniquePerson.Where(CK.K["Name"] == "Tom").Select();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list[0].Id);
        }

        [Test]
        public void TestInitialize()
        {
            var o = InitTest.New.Init("tom", 17);
            Assert.AreEqual(0, o.Id);
            Assert.AreEqual("tom", o.Name);
            Assert.AreEqual(17, o.Age);

            var o2 = InitTest2.New.Init("1", "2", "3");
            Assert.AreEqual("1", o2.Name);
            Assert.AreEqual("2", o2.FirstName);
            Assert.AreEqual("3", o2.LastName);

            var o3 = InitTest3.New.Init("1", "2", "3");
            Assert.AreEqual("1", o3.Name);
            Assert.AreEqual("2", o3.LastName);
            Assert.AreEqual("3", o3.FirstName);

            var o4 = InitTest4.New.Initialize("1", true, 18);
            Assert.AreEqual("1", o4.Name);
            Assert.AreEqual(true, o4.Gender);
            Assert.AreEqual(18, o4.Age);

            o4 = InitTest4.New.Initialize("1", true, null);
            Assert.AreEqual("1", o4.Name);
            Assert.AreEqual(true, o4.Gender);
            Assert.IsNull(o4.Age);

            var ox = InitTest4.New.Initialize(o4);
            Assert.AreEqual("1", ox.Name);
            Assert.AreEqual(true, ox.Gender);
            Assert.IsNull(ox.Age);
        }

        [Test]
        public void TestGroupbySum()
        {
            sqlite.From<SinglePerson>().Where(Condition.Empty).GroupBySum<string, long>("Name", "Id");
            AssertSql(@"SELECT [Name],SUM([Id]) AS [Id] FROM [People] GROUP BY [Name];
<Text><60>()");

            var list = DbEntry.From<Book>().Where(Condition.Empty).GroupBySum<long, long>("Category_Id", "Id");
            var sorted = (from o in list orderby o.Column select o).ToList();

            Assert.AreEqual(2, sorted[0].Column);
            Assert.AreEqual(10, sorted[0].Sum);

            Assert.AreEqual(3, sorted[1].Column);
            Assert.AreEqual(5, sorted[1].Sum);
        }
    }
}
