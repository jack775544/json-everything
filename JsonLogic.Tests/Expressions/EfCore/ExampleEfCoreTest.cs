using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Logic.Rules;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions.EfCore;

public class ExampleEfCoreTest
{
	[Test]
	public async Task TestSomething()
	{
		await using var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync();
		await using var dbContext = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
			.UseSqlite(connection)
			.Options);
		await dbContext.Database.EnsureCreatedAsync();

		dbContext.Departments.AddRange(
			new Department
			{
				Id = 1,
				Name = "Reporting",
				Employees = [
					new Employee { Id = 1, Name = "Alice", DateOfBirth = new DateTime(1990, 1, 1) },
					new Employee { Id = 2, Name = "Bob", DateOfBirth = new DateTime(1995, 5, 5) },
				]
			},
			new Department
			{
				Id = 2,
				Name = "Management",
				Employees = [
					new Employee { Id = 4, Name = "Jane", DateOfBirth = new DateTime(1994, 4, 4) },
				]
			},
			new Department
			{
				Id = 3,
				Name = "HR",
				Employees = [
					new Employee { Id = 5, Name = "John", DateOfBirth = new DateTime(2002, 2, 2) },
					new Employee { Id = 6, Name = "Alice", DateOfBirth = new DateTime(1996, 6, 6) },
					new Employee { Id = 7, Name = "Tom", DateOfBirth = new DateTime(1997, 7, 7) },
				]
			});

		await dbContext.SaveChangesAsync();

		var rule = new StrictEqualsRule("HR", new VariableRule("Name"));
		var expression = ExpressionTestHelpers.CreateRuleExpression<Department, bool>(rule);

		var departmentCount = await dbContext
			.Departments
			.Where(expression)
			.CountAsync();
		Assert.AreEqual(1, departmentCount);
	}
}