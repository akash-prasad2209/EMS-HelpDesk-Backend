using HelpDeskSystem.Api.Models.Entities;

namespace HelpDeskSystem.Api.Data;

// Runs at startup in Development only (see Program.cs). Seeds the lookup
// tables and one demo login per role so the RBAC matrix can be tried immediately.
public static class DbSeeder
{
    public static async Task SeedAsync(HelpDeskDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { RoleName = RoleNames.Admin },
                new Role { RoleName = RoleNames.Employee },
                new Role { RoleName = RoleNames.Hr },
                new Role { RoleName = RoleNames.It });
            await db.SaveChangesAsync();
        }

        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
                new Department { DepartmentName = DepartmentNames.It, Description = "Information Technology" },
                new Department { DepartmentName = DepartmentNames.Hr, Description = "Human Resources" },
                new Department { DepartmentName = DepartmentNames.Admin, Description = "General Administration" });
            await db.SaveChangesAsync();
        }

        if (!db.Categories.Any())
        {
            var it = db.Departments.Single(d => d.DepartmentName == DepartmentNames.It);
            var hr = db.Departments.Single(d => d.DepartmentName == DepartmentNames.Hr);
            var admin = db.Departments.Single(d => d.DepartmentName == DepartmentNames.Admin);

            db.Categories.AddRange(
                new Category { CategoryName = "Laptop / Hardware Issue", DepartmentId = it.DepartmentId },
                new Category { CategoryName = "Network / VPN Access", DepartmentId = it.DepartmentId },
                new Category { CategoryName = "Payslip Query", DepartmentId = hr.DepartmentId },
                new Category { CategoryName = "Leave Query", DepartmentId = hr.DepartmentId },
                new Category { CategoryName = "Facility / Seating Request", DepartmentId = admin.DepartmentId });
            await db.SaveChangesAsync();
        }

        if (!db.RequestTypes.Any())
        {
            db.RequestTypes.AddRange(
                new RequestType { RequestTypeName = "Incident" },
                new RequestType { RequestTypeName = "Service Request" });
            await db.SaveChangesAsync();
        }

        if (!db.Employees.Any())
        {
            var adminRole = db.Roles.Single(r => r.RoleName == RoleNames.Admin);
            var employeeRole = db.Roles.Single(r => r.RoleName == RoleNames.Employee);
            var hrRole = db.Roles.Single(r => r.RoleName == RoleNames.Hr);
            var itRole = db.Roles.Single(r => r.RoleName == RoleNames.It);

            var itDept = db.Departments.Single(d => d.DepartmentName == DepartmentNames.It);
            var hrDept = db.Departments.Single(d => d.DepartmentName == DepartmentNames.Hr);
            var adminDept = db.Departments.Single(d => d.DepartmentName == DepartmentNames.Admin);

            string hash(string p) => BCrypt.Net.BCrypt.HashPassword(p);

            db.Employees.AddRange(
                new Employee { EmployeeCode = "EMP001", FullName = "System Admin", Email = "admin@helpdesk.local", PasswordHash = hash("Passw0rd!"), RoleId = adminRole.RoleId, DepartmentId = adminDept.DepartmentId },
                new Employee { EmployeeCode = "EMP002", FullName = "Rohit Sharma", Email = "employee@helpdesk.local", PasswordHash = hash("Passw0rd!"), RoleId = employeeRole.RoleId },
                new Employee { EmployeeCode = "EMP003", FullName = "Aarti Kulkarni", Email = "hr@helpdesk.local", PasswordHash = hash("Passw0rd!"), RoleId = hrRole.RoleId, DepartmentId = hrDept.DepartmentId },
                new Employee { EmployeeCode = "EMP004", FullName = "Vikram Shah", Email = "it@helpdesk.local", PasswordHash = hash("Passw0rd!"), RoleId = itRole.RoleId, DepartmentId = itDept.DepartmentId });
            await db.SaveChangesAsync();
        }
    }
}
