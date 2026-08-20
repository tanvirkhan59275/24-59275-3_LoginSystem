-- Lab 1: Login, Registration & Logout
-- Student ID: 24-59275-3
-- Run this whole script in SSMS (or Visual Studio > SQL Server Object Explorer)
-- against your local SQL Server instance.

-- Database name has hyphens in it, so it needs to be in brackets.
CREATE DATABASE [24-59275-3_LoginDB];
GO

USE [24-59275-3_LoginDB];
GO

CREATE TABLE dbo.Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,   -- SHA-256 hex string, never the real password
    Email         NVARCHAR(100),
    FullName      NVARCHAR(100),
    CreatedAt     DATETIME DEFAULT GETDATE()
);
GO

-- Bonus (Task 8, option 1): keep a history of logins/logouts per user.
CREATE TABLE dbo.LoginHistory (
    HistoryID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID      INT NOT NULL,
    LoginTime   DATETIME DEFAULT GETDATE(),
    LogoutTime  DATETIME NULL,
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserID)
        REFERENCES dbo.Users (UserID)
);
GO

-- Sanity check - should return the two tables with no rows yet.
SELECT * FROM dbo.Users;
SELECT * FROM dbo.LoginHistory;
