-- Helpdesk Management System - schema script
-- Mirrors HelpDeskDbContext.OnModelCreating exactly (table names, types, keys, FKs, indexes).
-- Use this if you want to create the database directly (SSMS / sqlcmd) instead of running
-- Add-Migration / Update-Database in Visual Studio. Either path produces the same schema.

IF DB_ID('HelpDeskDb') IS NULL
BEGIN
    CREATE DATABASE HelpDeskDb;
END
GO

USE HelpDeskDb;
GO

CREATE TABLE Roles (
    RoleId      INT IDENTITY PRIMARY KEY,
    RoleName    NVARCHAR(50) NOT NULL,
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);

CREATE TABLE Departments (
    DepartmentId    INT IDENTITY PRIMARY KEY,
    DepartmentName  NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT UQ_Departments_DepartmentName UNIQUE (DepartmentName)
);

CREATE TABLE Employees (
    EmployeeId      INT IDENTITY PRIMARY KEY,
    EmployeeCode    NVARCHAR(20) NOT NULL,
    FullName        NVARCHAR(150) NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    PasswordHash    NVARCHAR(300) NOT NULL,
    RoleId          INT NOT NULL,
    DepartmentId    INT NULL,
    IsActive        BIT NOT NULL DEFAULT (1),
    CreatedAt       DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Employees_EmployeeCode UNIQUE (EmployeeCode),
    CONSTRAINT UQ_Employees_Email UNIQUE (Email),
    CONSTRAINT FK_Employees_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE NO ACTION,
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE NO ACTION
);
CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);

CREATE TABLE Categories (
    CategoryId      INT IDENTITY PRIMARY KEY,
    CategoryName    NVARCHAR(150) NOT NULL,
    DepartmentId    INT NOT NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT FK_Categories_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE NO ACTION
);
CREATE INDEX IX_Categories_DepartmentId ON Categories(DepartmentId);

CREATE TABLE RequestTypes (
    RequestTypeId       INT IDENTITY PRIMARY KEY,
    RequestTypeName     NVARCHAR(100) NOT NULL,
    Description         NVARCHAR(500) NULL
);

CREATE TABLE Assets (
    AssetId     INT IDENTITY PRIMARY KEY,
    AssetNo     NVARCHAR(50) NOT NULL,
    AssetName   NVARCHAR(150) NOT NULL,
    AssetType   NVARCHAR(100) NULL,
    IPAddress   NVARCHAR(45) NULL,
    Location    NVARCHAR(150) NULL,
    Status      NVARCHAR(50) NULL,
    CONSTRAINT UQ_Assets_AssetNo UNIQUE (AssetNo)
);

CREATE TABLE Tickets (
    TicketId                INT IDENTITY PRIMARY KEY,
    TicketNumber            NVARCHAR(20) NOT NULL,
    ShortDescription        NVARCHAR(200) NOT NULL,
    Description             NVARCHAR(MAX) NULL,
    Impact                  NVARCHAR(20) NOT NULL,
    Urgency                 NVARCHAR(20) NOT NULL,
    Priority                NVARCHAR(20) NOT NULL,
    Status                  NVARCHAR(20) NOT NULL,
    CreatedByEmployeeId     INT NOT NULL,
    AssignedToEmployeeId    INT NULL,
    CategoryId              INT NOT NULL,
    RequestTypeId           INT NOT NULL,
    AssetId                 INT NULL,
    CreatedAt               DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    UpdatedAt               DATETIME2 NULL,
    ResolvedAt              DATETIME2 NULL,
    ClosedAt                DATETIME2 NULL,
    CONSTRAINT UQ_Tickets_TicketNumber UNIQUE (TicketNumber),
    CONSTRAINT FK_Tickets_CreatedBy FOREIGN KEY (CreatedByEmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION,
    CONSTRAINT FK_Tickets_AssignedTo FOREIGN KEY (AssignedToEmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION,
    CONSTRAINT FK_Tickets_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE NO ACTION,
    CONSTRAINT FK_Tickets_RequestTypes FOREIGN KEY (RequestTypeId) REFERENCES RequestTypes(RequestTypeId) ON DELETE NO ACTION,
    CONSTRAINT FK_Tickets_Assets FOREIGN KEY (AssetId) REFERENCES Assets(AssetId) ON DELETE NO ACTION
);
CREATE INDEX IX_Tickets_Status ON Tickets(Status);
CREATE INDEX IX_Tickets_CategoryId ON Tickets(CategoryId);
CREATE INDEX IX_Tickets_AssignedToEmployeeId ON Tickets(AssignedToEmployeeId);

CREATE TABLE TicketComments (
    CommentId       INT IDENTITY PRIMARY KEY,
    TicketId        INT NOT NULL,
    EmployeeId      INT NOT NULL,
    CommentText     NVARCHAR(MAX) NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TicketComments_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE NO ACTION,
    CONSTRAINT FK_TicketComments_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION
);

CREATE TABLE TicketHistories (
    TicketHistoryId         INT IDENTITY PRIMARY KEY,
    TicketId                INT NOT NULL,
    ChangedByEmployeeId     INT NOT NULL,
    ActionType              NVARCHAR(50) NOT NULL,
    FieldName               NVARCHAR(100) NULL,
    OldValue                NVARCHAR(500) NULL,
    NewValue                NVARCHAR(500) NULL,
    ChangedAt               DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TicketHistories_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE NO ACTION,
    CONSTRAINT FK_TicketHistories_Employees FOREIGN KEY (ChangedByEmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION
);

CREATE TABLE TicketAttachments (
    AttachmentId            INT IDENTITY PRIMARY KEY,
    TicketId                INT NOT NULL,
    FileName                NVARCHAR(255) NOT NULL,
    FilePath                NVARCHAR(500) NOT NULL,
    FileType                NVARCHAR(50) NULL,
    FileSize                BIGINT NULL,
    UploadedByEmployeeId    INT NOT NULL,
    UploadedAt              DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_TicketAttachments_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE NO ACTION,
    CONSTRAINT FK_TicketAttachments_Employees FOREIGN KEY (UploadedByEmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION
);

CREATE TABLE Notifications (
    NotificationId  INT IDENTITY PRIMARY KEY,
    TicketId        INT NOT NULL,
    EmployeeId      INT NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Message         NVARCHAR(500) NOT NULL,
    IsRead          BIT NOT NULL DEFAULT (0),
    CreatedAt       DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Notifications_Tickets FOREIGN KEY (TicketId) REFERENCES Tickets(TicketId) ON DELETE NO ACTION,
    CONSTRAINT FK_Notifications_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE NO ACTION
);
GO

-- Optional seed data (the app also seeds this automatically at startup in Development -
-- run this block only if you're using the script instead of letting the app seed).
/*
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Employee'), ('HR'), ('IT');
INSERT INTO Departments (DepartmentName, Description) VALUES
    ('IT', 'Information Technology'),
    ('HR', 'Human Resources'),
    ('Admin', 'General Administration');
*/
