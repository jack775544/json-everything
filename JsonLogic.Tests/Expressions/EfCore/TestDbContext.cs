using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Json.Logic.Tests.Expressions.EfCore;

public class Department
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public ICollection<Employee> Employees { get; set; }
}

public class Employee
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public required DateTime DateOfBirth { get; set; }

	public int DepartmentId { get; set; }
	public Department? Department { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
	public DbSet<Employee> Employees => Set<Employee>();
	public DbSet<Department> Departments => Set<Department>();
}