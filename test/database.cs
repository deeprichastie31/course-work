using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
// всегда проверяем подключение к базе данных
public class DBManager
{
    private SqliteConnection? connect = null;

    private string Hash_password(string password)
    {
        using(var alg = SHA256.Create()) 
        {
            var bytes_hash = alg.ComputeHash(Encoding.Unicode.GetBytes(password)); // преобразование в байты
            return Encoding.Unicode.GetString(bytes_hash);
        }
        
    }
    public bool ConnectToDB(string path)
    {
        try
        {
            connect = new SqliteConnection("Data source=" + path);
            connect.Open();
            if (connect.State != System.Data.ConnectionState.Open) // проверка состояния подключения
            {
                Console.WriteLine("O-ops!");
                return false;
            }
        }
        catch (Exception exp) // выкидывет не объекты ошибок, а исключения. 
        {
            Console.WriteLine(exp.Message);
            return false;
        }
        Console.WriteLine("Connected!");
        return true;
    }
    public void Disconnect()
    {
        if (connect == null)
        {
            return; // вырубает базу данных.
        }
        if (connect.State != System.Data.ConnectionState.Open) // было ли вообще подключение
        {
            return;
        }
        connect.Close();
        Console.WriteLine("Disconnected!");
    }
    public bool AddUser(string login, string password)
    {
        if (connect == null)
        {
            return false; // невозможно добавить
        }
        if (connect.State != System.Data.ConnectionState.Open)
        {
            return false;
        }
        string REQUEST = "INSERT INTO users (login, password) VALUES ('" + login + "', '"+ Hash_password(password) + "')";
        var command = new SqliteCommand(REQUEST, connect);
        
        int result = 0;
        try
        {
            result = command.ExecuteNonQuery();
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
        if (result == 1)
            return true;
        else
            return false;
        
    }
    public bool CheckByLogin(string login)
{
    if (connect == null || connect.State != System.Data.ConnectionState.Open)
    {
        Console.WriteLine("База данных недоступна.");
        return false; // База данных недоступна
    }

    string REQUEST = "SELECT 1 FROM users WHERE login = @login";
    var command = new SqliteCommand(REQUEST, connect);

    // Используем параметризованный запрос
    command.Parameters.AddWithValue("@login", login);

    try
    {
        using (var reader = command.ExecuteReader())
        {
            return reader.HasRows; // Возвращает true, если найдены строки
        }
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // Пример: ошибка UNIQUE или другая, связанная с ограничениями
    {
        Console.WriteLine("Ошибка базы данных: конфликт при проверке логина.");
        return false; // Возвращаем false при ошибке, связанной с базой данных
    }
    catch (SqliteException ex) // Общая обработка SQLite ошибок
    {
        Console.WriteLine($"Ошибка SQLite: {ex.Message}");
        return false; // Возвращаем false при любой другой SQLite ошибке
    }
    catch (Exception ex) // Общая обработка всех остальных ошибок
    {
        Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
        return false; // Возвращаем false при любой другой ошибке
    }
}
    
    public bool CheckUser(string login, string password)
{
    if (connect == null || connect.State != System.Data.ConnectionState.Open)
    {
        Console.WriteLine("База данных недоступна.");
        return false; // База данных недоступна
    }

    // Параметризованный запрос
    string REQUEST = "SELECT 1 FROM users WHERE login = @login AND password = @password";
    var command = new SqliteCommand(REQUEST, connect);

    // Добавляем параметры для безопасности
    command.Parameters.AddWithValue("@login", login);
    command.Parameters.AddWithValue("@password", Hash_password(password));

    try
    {
        var reader = command.ExecuteReader();
        return reader.HasRows; // Возвращает true, если найдены строки
    }
    catch (Exception exp)
    {
        Console.WriteLine($"Ошибка выполнения запроса: {exp.Message}");
        return false;
    }
}

}