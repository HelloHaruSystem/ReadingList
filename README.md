# Reading List ADO.NET Project

### Project Description
This is a console application for a school project built with ADO.NET and C# that functions as a personal reading list management system. The application allows the user to manage books, update reading statuses, rate books, and track reading goals. The user interface is a text-based TUI (Terminal User Interface) powered by `Terminal.Gui`.

---

### Assignment Overview
This project was an ADO.NET assignment that required building on an existing database. The key tasks were to:

* Implement **Create, Read, Update, and Delete (CRUD)** operations for various database tables.
* Create a **menu-driven system** for navigating the application's features.
* Use **ADO.NET parameters** to prevent SQL injection vulnerabilities.
* Develop a user-friendly TUI.a

---

### How to Set Up the Project

1.  **Database Setup**
    * Ensure you have access to a MS SQL Server.
    * Execute the `full-sql.sql` script to create the `reading_list` database and populate it with initial data.

2.  **Configuration**
    * In the project's root directory, create a new file named `appsettings.json`.
    * Add your database connection string to this file using the format below. Replace the placeholder values with your actual server, user, and password information.


    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=reading_list;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;Trusted_Connection=False;TrustServerCertificate=True;"
      }
    }
    ```

3.  **Run the Application**
    * Open your terminal or command prompt.
    * Navigate to the project's root directory (the same folder as `ReadingList.csproj`).
    * Run the following command:


    ```bash
    dotnet run
    ```
---

### Dependencies

This project relies on the following NuGet packages, which are automatically restored when you run `dotnet run`:

* `Microsoft.Data.SqlClient`
* `Microsoft.Extensions.Configuration`
* `Microsoft.Extensions.Configuration.Json`
* `Microsoft.Extensions.DependencyInjection`
* `Terminal.Gui` - [GitHub Repository](https://github.com/gui-cs/Terminal.Gui)