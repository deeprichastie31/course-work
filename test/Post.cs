using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json; // Парсинг джсона вручную.
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

DBManager dB = new DBManager();RGSortAdapter gs = new RGSortAdapter();
const string Path_to_db = "/home/kishlak/WebApp/users.db";
if (!dB.ConnectToDB(Path_to_db))
{
    Console.WriteLine("Ну приехали, все!");
    return;
}
app.MapGet("/test", () => gs.test());

app.MapPost("/login", async (string login, string password, HttpContext context) =>
{
    if (!dB.CheckUser(login, password))
    {
        var response_1 = new RGValues("Авторизация не удалась");
        return Results.Json(response_1, statusCode: 401);
    }
    var sessionId = Guid.NewGuid().ToString();
    var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, login),
        new Claim("SessionId", sessionId)
    };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    var response = new RGValues($"Авторизация прошла успешно. id cессии: {sessionId}");
    return Results.Json(response, statusCode: 200);
});

app.MapPost("/SignUp", (string login, string password) =>
{
    if (dB.CheckByLogin(login))
    {
        var response = new RGValues("Данный пользователь уже суествует!");
        return Results.Json(response, statusCode: 409);
    }
    if (dB.AddUser(login, password))
    {
        var response = new RGValues("Регистрация прошла успешно");
        return Results.Json(response, statusCode: 200);
    }
    else
    {
        var response = new RGValues("Проблема на сервере, невозможно зарегестрировать пользоватетя");
        return Results.Json(response, statusCode: 500);
    }
});

app.MapPost("/logout", async (HttpContext context) =>
{
    var sessionId = context.User.FindFirst("SessionId")?.Value;

    if (sessionId != null)
        SessionManager.RemoveSession(sessionId);

    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var response = new RGValues("Выход из сессии прошел успешно");
    return Results.Json(response, statusCode: 200);
});

app.MapGet("/current_user", [Authorize] (HttpContext context) =>
{
    if (context.User.Identity == null)
    {
        var response_1 = new RGValues("Пользователь не найден!");
        return Results.Json(response_1, statusCode: 400);
    }
    var response = new RGValues(context.User.Identity.Name);
    return Results.Json(response, statusCode: 200);
});
// тут контекст не нужен, потому что нет отсылки к разганичителю сессий и нужно было сам метод немного поменять 
app.MapMethods("/Change_password", new[] { "PATCH" }, [Authorize] (string login, string oldPassword, string newPassword) =>
{
    if(!dB.Change_Password(login, oldPassword, newPassword))
    {
        var response = new RGValues("Старый пароль оказался неверен");
        return Results.Json(response, statusCode: 400);
    }
    var response_2 = new RGValues("Смена пароля прошла успешно");
    return Results.Json(response_2, statusCode: 200);
});

// Использование сессий для маршрутов
app.MapPost("/Generate_array", [Authorize] (int len, int lb, int ub, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Generate_array(len, lb, ub);
});

app.MapPost("/Create_array", [Authorize] (string array, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Create_array(array);
});

app.MapPost("/Sort_array", [Authorize] (HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.GnomeSortic();
});

app.MapGet("/Get_array", [Authorize] (HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Get_array();
});

app.MapGet("/Get_element", [Authorize] (int element, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Get_element(element);
});

app.MapGet("/Get_part_array", [Authorize] (int low_ind, int up_ind, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Get_part_array(low_ind, up_ind);
});

app.MapGet("/Get_history", [Authorize] (HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Get_history();
});

app.MapPatch("/Edit_array", [Authorize] (int index, int value, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Edit_array(index, value);
});
app.MapPatch("/Add_element", [Authorize] (int element, string position, int index, HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Add_element(element, position, index);
});

app.MapDelete("/Delete_array", [Authorize] (HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Delete_array();
});

app.MapDelete("/Clear_history", [Authorize] (HttpContext context) =>
{
    var session = SessionManager.GetOrCreateSession(context);
    return session.Clear_history();
});

app.Run();
// Класс для управления сессиями пользователей
public static class SessionManager
{
    private static readonly Dictionary<string, RGSortAdapter> UserSessions = new();

    public static RGSortAdapter GetOrCreateSession(HttpContext context)
    {
        var sessionId = context.User.FindFirst("SessionId")?.Value;

        if (sessionId == null)
            throw new UnauthorizedAccessException("Session ID not found.");

        if (!UserSessions.ContainsKey(sessionId))
            UserSessions[sessionId] = new RGSortAdapter(); // Создание новой сессии

        return UserSessions[sessionId];
    }

    public static void RemoveSession(string sessionId)
    {
        if (UserSessions.ContainsKey(sessionId))
            UserSessions.Remove(sessionId);
    }
}



public readonly struct RGValues // структура, которая имеет возможность преобразовывать в json не только массивы
{
    public int[]? Values { get; } 
    
    public string Message { get; } 
    
    public int? SingleValue { get; } 

    // Конструктор для массива
    public RGValues(string message, int[] values)
    {
        Message = message;
        Values = values;
        SingleValue = null;
    }

    // Конструктор для одиночного значения
    public RGValues(string message, int singleValue)
    {
        Message = message;
        Values = null;
        SingleValue = singleValue;
    }
    // конструктор для одиночного сообщения 
    public RGValues(string message)
    {
        Message = message;
        Values = null;
        SingleValue = null;
    }

}

