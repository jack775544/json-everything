using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Json.Logic.Expressions.Tests.EfCore;

public class Department
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required ICollection<Employee> Employees { get; set; }
}

public enum EmploymentStatus
{
	Current,
	Former,
}

public enum EmploymentType
{
	FullTime,
	PartTime,
	Casual,
}

public class Employee
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required DateTime DateOfBirth { get; set; }
	public required decimal Salary { get; set; }
	public required double Height { get; set; }
	public required int NumberOfChildren { get; set; }
	public EmploymentStatus Status { get; set; } = EmploymentStatus.Current;
	public EmploymentType? EmploymentType { get; set; }

	public int DepartmentId { get; set; }
	public Department? Department { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
	public DbSet<Employee> Employees => Set<Employee>();
	public DbSet<Department> Departments => Set<Department>();
}