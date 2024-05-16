using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Json.Logic.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions.EfCore;

file class TestDatabase : IAsyncDisposable
{
	private readonly SqliteConnection _connection;
	private readonly ILoggerFactory _loggerFactory;
	public TestDbContext DbContext { get; }
	
	public TestDatabase()
	{
		_connection = new SqliteConnection("Filename=:memory:");
		_loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
		DbContext = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
			.UseSqlite(_connection)
			.EnableSensitiveDataLogging()
			.UseLoggerFactory(_loggerFactory)
			.Options);
	}

	public async Task InitialiseAsync()
	{
		await _connection.OpenAsync();
		await DbContext.Database.EnsureCreatedAsync();

		DbContext.Departments.AddRange(
			new Department
			{
				Id = 1,
				Name = "Reporting",
				Employees = [
					new Employee { Id = 1, Name = "Alice", DateOfBirth = new DateTime(1990, 1, 1), Height = 175.5, Salary = 10000m, NumberOfChildren = 1 },
					new Employee { Id = 2, Name = "Bob", DateOfBirth = new DateTime(1995, 5, 5), Height = 170, Salary = 15000m, NumberOfChildren = 2 },
				]
			},
			new Department
			{
				Id = 2,
				Name = "Management",
				Employees = [
					new Employee { Id = 4, Name = "Jane", DateOfBirth = new DateTime(1994, 4, 4), Height = 165.6, Salary = 20000.50m, NumberOfChildren = 3 },
				]
			},
			new Department
			{
				Id = 3,
				Name = "HR",
				Employees = [
					new Employee { Id = 5, Name = "John", DateOfBirth = new DateTime(2002, 2, 2), Height = 160, Salary = 30000m, NumberOfChildren = 4 },
					new Employee { Id = 6, Name = "Alice", DateOfBirth = new DateTime(1996, 6, 6), Height = 162.2, Salary = 350000m, NumberOfChildren = 5 },
					new Employee { Id = 7, Name = "Tom", DateOfBirth = new DateTime(1997, 7, 7), Height = 190.1, Salary = 40000m, NumberOfChildren = 6 },
				]
			});

		await DbContext.SaveChangesAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await DbContext.DisposeAsync();
		_loggerFactory.Dispose();
		await _connection.DisposeAsync();
	}
}

public class EfCoreTests
{
	private static readonly CreateExpressionOptions _options = new()
	{
		WrapConstants = true,
	};

	[Test]
	public async Task Reduce()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		// var rule = new StrictEqualsRule(
		// 	new ReduceRule(
		// 		new VariableRule("Employees"),
		// 		new AddRule(new VariableRule("current.NumberOfChildren"), new VariableRule("accumulator")),
		// 		0),
		// 	15);
		var rule = new StrictEqualsRule(
			new ReduceRule(
				new VariableRule("Employees"),
				new CatRule(new VariableRule("current.Name"), new VariableRule("accumulator")),
				""),
			"");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);
		
		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}
	
	[Test]
	public async Task EqualsName()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule(new VariableRule("Name"), "HR");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);
		
		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task EqualsId()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule(1, new VariableRule("Id"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);
		
		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task EqualsIdWrongTypeDecimal()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule(1M, new VariableRule("Id"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);
		
		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}
	
	[Test]
	public async Task EqualsIdWrongTypeString()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule("1", new VariableRule("Id"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);

		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}
	
	[Test]
	public async Task Or()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new OrRule(
			new StrictEqualsRule(1, new VariableRule("Id")),
			new StrictEqualsRule(new VariableRule("Name"), "HR"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);
		
		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(2, departmentCount);
	}

	[Test]
	public async Task StringConcat()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule("RunBobRun", new CatRule("Run", new VariableRule("Name"), "Run"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Employee, bool>(rule, _options);

		var departmentCount = await database.DbContext
			.Employees
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task Some()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new SomeRule(new VariableRule("Employees"), new StrictEqualsRule(new VariableRule("Name"), "Alice"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);

		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(2, departmentCount);
	}

	[Test]
	public async Task All()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new AllRule(new VariableRule("Employees"), new StrictEqualsRule(new VariableRule("Name"), "Jane"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);

		Console.WriteLine(database.DbContext
			.Departments
			.Where(expression)
			.ToQueryString());

		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task None()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new NoneRule(new VariableRule("Employees"), new StrictEqualsRule(new VariableRule("Name"), "Alice"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);

		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task ReferenceCount()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new StrictEqualsRule(new VariableRule("Employees.Count"), 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Department, bool>(rule, _options);

		var departmentCount = await database.DbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}

	[Test]
	public async Task And()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new AndRule(
			new StrictEqualsRule(new VariableRule("Name"), "Alice"),
			new MoreThanRule(new VariableRule("DateOfBirth"), "1995-01-01T00:00:00"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Employee, bool>(rule, _options);

		Console.WriteLine(database.DbContext
			.Employees
			.Where(expression)
			.ToQueryString());

		var departmentCount = await database.DbContext
			.Employees
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}
	
	[Test]
	public async Task OrderByRule()
	{
		await using var database = new TestDatabase();
		await database.InitialiseAsync();

		var rule = new VariableRule("Name");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Employee, object>(rule, _options);

		var employees = await database.DbContext
			.Employees
			.OrderBy(expression)
			.ToListAsync();
		Assert.AreEqual(6, employees.Count);
	}
}