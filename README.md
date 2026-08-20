# 24-59275-3_LoginSystem

Lab 1 - Login, Registration & Logout with C# and SQL Server.

A WinForms app with three forms (Login, Register, Home) backed by a SQL Server
database, built from scratch (not a copy of the sample `Login_System` project).

> Fill in the bracketed bits below with your actual setup before you submit -
> I left them as placeholders since I don't know your exact machine.

## Environment

- SQL Server: **[edit: e.g. SQL Server 2022 Express]**
- Visual Studio: **[edit: e.g. Visual Studio 2022 17.9]**
- .NET: .NET Framework 4.7.2 (WinForms, old-style `.csproj`)
- Connection string format used in `App.config`:
  ```
  Data Source=localhost\SQLEXPRESS;Initial Catalog=24-59275-3_LoginDB;Integrated Security=True;Connect Timeout=30
  ```
  No password is committed anywhere - `Integrated Security=True` logs in with
  the current Windows account, so there's no credential to leak. If your setup
  needs a SQL login instead, swap in `User Id=...;Password=...` locally and
  don't commit that value.

## How the database was created

Ran `Schema.sql` (included in this repo) in SSMS against my local instance.
It:
1. Creates `[24-59275-3_LoginDB]` (bracketed because the name has hyphens).
2. Creates `dbo.Users` with the exact columns the assignment asked for
   (`UserID`, `Username`, `PasswordHash`, `Email`, `FullName`, `CreatedAt`).
3. Creates `dbo.LoginHistory` for the bonus task, with a foreign key back to
   `Users.UserID`.

Screenshot of the table design goes here: **[insert screenshot]**

## Project layout

```
24-59275-3_LoginSystem/
  24-59275-3_LoginSystem.sln
  24-59275-3_LoginSystem/
    App.config              connection string only, no code touches SQL directly
    Program.cs               entry point, tests the connection before showing Login
    DatabaseHelper.cs         all ADO.NET code lives here (bonus - see below)
    LoginForm.cs / .Designer.cs
    RegisterForm.cs / .Designer.cs
    HomeForm.cs / .Designer.cs
Schema.sql
InjectionDemo/VulnerableLoginExample.cs   report-only, not part of the build
```

## How registration, login and logout work

**Registration** (`RegisterForm.cs`) validates on the client side first
(nothing empty, password 6+ characters, passwords match, email has an `@`),
then calls `DatabaseHelper.UsernameExists()` (a parameterized
`SELECT COUNT(*) ... WHERE Username = @username`, run with `ExecuteScalar()`)
before inserting. If the name's free, it hashes the password and calls
`DatabaseHelper.RegisterUser()`, which does a parameterized
`INSERT ... ExecuteNonQuery()`. On success it shows a message box, clears the
form, and closes back to Login.

**Login** (`LoginForm.cs`) hashes whatever was typed and calls
`DatabaseHelper.TryLogin()`, which runs a parameterized `SELECT` with a
`SqlDataReader` comparing `Username` **and** `PasswordHash` in the same query.
If it finds a row, `LoginForm` hides itself and opens `HomeForm`, passing the
`FullName` so `HomeForm` can show "Welcome, {FullName}". If it doesn't find a
row, it shows a message box, decrements a `failedAttempts` counter, and after
3 misses disables the Login button entirely.

**Logout** (`HomeForm.cs`) calls `this.Close()` on the Home form - it does
**not** call `Application.Exit()`. `LoginForm` subscribes to `HomeForm`'s
`FormClosed` event and, when it fires, clears its own textboxes, re-enables
the Login button, resets the failed-attempt counter, and shows itself again
with focus on the username box. The app's actual "main form" (the one
`Application.Run()` was given) is always `LoginForm`, so hiding/showing it
never ends the process, and there's only ever one `HomeForm` alive at a time.

## Password hashing

`DatabaseHelper.HashPassword()` runs the password through `SHA256` and stores
the hex digest in `PasswordHash`. Registration hashes once and stores the
hash; login hashes the typed password and compares hash-to-hash in the SQL
`WHERE` clause. The real password is never written to the database, never
logged, and never held anywhere longer than it takes to hash it - plain text
storage means a database leak (or even just someone with read access to the
table) instantly exposes every user's real password, and because people
reuse passwords across sites, that's not just a problem for this app.

## SQL injection demo (Task 6)

- **Vulnerable version:** `InjectionDemo/VulnerableLoginExample.cs`. It builds
  the query with string concatenation, exactly like bug #1 in the sample
  project.
- **Exploit input:** typing `' OR '1'='1` into the password field turns the
  query into `...WHERE Username='x' AND PasswordHash='' OR '1'='1'`, which is
  always true, so the login "succeeds" with no real password.
- **Fixed version:** `DatabaseHelper.TryLogin()` - same query, but
  `@username`/`@hash` are added as `SqlParameter`s instead of being pasted
  into the string.
- **Why it works:** parameters send the value to SQL Server separately from
  the query text, so the server never re-parses it as SQL. `' OR '1'='1`
  is just compared as a literal (wrong) string, so the login correctly fails.

Screenshots for the report: **[insert: vulnerable version bypassed]**,
**[insert: fixed code]**, **[insert: same input failing on the fixed version]**.

## Bonus tasks attempted (2 of 5, +15)

1. **`DatabaseHelper` class** - every `SqlConnection`/`SqlCommand` in the
   whole project lives in `DatabaseHelper.cs`. `LoginForm`, `RegisterForm`,
   and `HomeForm` never reference `SqlConnection` at all - they just call
   methods like `TryLogin()`, `RegisterUser()`, `GetAllUsers()`.
2. **`LoginHistory` table** - `DatabaseHelper.LogLoginStart()` inserts a row
   (with `LoginTime`) the moment a login succeeds and returns the new
   `HistoryID`. `HomeForm` holds onto that ID and calls
   `DatabaseHelper.LogLogoutTime()` on logout, which stamps `LogoutTime` on
   that same row.

## Problems I hit and how I solved them

**[fill this in with what actually happened on your machine]** - e.g. if you
hit "Cannot open database" because you ran the app before running
`Schema.sql`, or `ConfigurationManager does not exist` because
`System.Configuration` wasn't referenced yet, write it here. This section is
part of the grade, so it needs to be your own account, not a generic line.

## Every query in this project (for the "-15 if any concatenated SQL" check)

All in `DatabaseHelper.cs`, all parameterized:
`UsernameExists`, `RegisterUser`, `TryLogin`, `GetAllUsers`,
`LogLoginStart`, `LogLogoutTime`. The only concatenated SQL anywhere in the
repo is the intentionally-bad demo in `InjectionDemo/VulnerableLoginExample.cs`,
which is not referenced by the `.csproj` and does not build into the app.

## Running it

1. Open `24-59275-3_LoginSystem.sln` in Visual Studio.
2. Run `Schema.sql` against your SQL Server instance (SSMS, or
   View > SQL Server Object Explorer).
3. Edit the `Data Source` in `App.config` to match your instance name if it's
   not `localhost\SQLEXPRESS`.
4. Press F5. Register a user, log in, watch the grid populate on `HomeForm`,
   log out, confirm the login form is clean and the app is still running.
