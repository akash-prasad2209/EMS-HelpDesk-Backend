using HelpDeskSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskSystem.Api.Data;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(x => x.DepartmentId);
            e.Property(x => x.DepartmentName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.DepartmentName).IsUnique();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.EmployeeId);
            e.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();

            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.DepartmentId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.DepartmentId);
        });

        modelBuilder.Entity<RequestType>(e =>
        {
            e.HasKey(x => x.RequestTypeId);
            e.Property(x => x.RequestTypeName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasKey(x => x.AssetId);
            e.Property(x => x.AssetNo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.AssetNo).IsUnique();
            e.Property(x => x.AssetName).HasMaxLength(150).IsRequired();
            e.Property(x => x.AssetType).HasMaxLength(100);
            e.Property(x => x.IPAddress).HasMaxLength(45);
            e.Property(x => x.Location).HasMaxLength(150);
            e.Property(x => x.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(x => x.TicketId);
            e.Property(x => x.TicketNumber).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.TicketNumber).IsUnique();
            e.Property(x => x.ShortDescription).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Impact).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Urgency).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.AssignedToEmployeeId);

            e.HasOne(x => x.CreatedByEmployee).WithMany().HasForeignKey(x => x.CreatedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedToEmployee).WithMany().HasForeignKey(x => x.AssignedToEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RequestType).WithMany().HasForeignKey(x => x.RequestTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketComment>(e =>
        {
            e.HasKey(x => x.CommentId);
            e.Property(x => x.CommentText).IsRequired();
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketHistory>(e =>
        {
            e.HasKey(x => x.TicketHistoryId);
            e.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
            e.Property(x => x.FieldName).HasMaxLength(100);
            e.Property(x => x.OldValue).HasMaxLength(500);
            e.Property(x => x.NewValue).HasMaxLength(500);
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ChangedByEmployee).WithMany().HasForeignKey(x => x.ChangedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketAttachment>(e =>
        {
            e.HasKey(x => x.AttachmentId);
            e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.FileType).HasMaxLength(50);
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UploadedByEmployee).WithMany().HasForeignKey(x => x.UploadedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Message).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
