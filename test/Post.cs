using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json; // Парсинг джсона вручную.
using System.Text.Json.Serialization;
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


DBManager dB = new DBManager();
RGSortAdapter gs = new RGSortAdapter();

const string Path_to_db = "/home/kishlak/WebApp/users.db";
if(!dB.ConnectToDB(Path_to_db))
{
    Console.WriteLine("Ну приехали, все!");
    return;
}

// Дело все было в том, что при передаче json, компилятор не знал откуда брать данные(тело, строка или форма). Теперь нужно переписать клиента и нормальные запросы сделать
app.MapGet("/test", () => gs.test());
app.MapPost("/Generate_array", [Authorize] (int len, int lb, int ub) => gs.Generate_array(len, lb, ub));
app.MapPost("/Create_array", [Authorize] (string array) => gs.Create_array(array));
app.MapPost("/Sort_array", [Authorize] () => gs.GnomeSortic());
app.MapPost("/Go_back", [Authorize] () => gs.Go_back());
app.MapGet("/Get_array", [Authorize] ()=> gs.Get_array());
app.MapGet("/Get_element", [Authorize] (int element)=> gs.Get_element(element));
app.MapGet("/Get_part_array", [Authorize] (int low_ind, int up_ind) => gs.Get_part_array(low_ind, up_ind)); 
/*
1) при неверном выборе границ массива выводи 400
2) при неавторизованном пользователе выдает 500
*/
app.MapGet("/Get_history", [Authorize] () => gs.Get_history());
app.MapPatch("/Edit_array", [Authorize] (int index, int value)=> gs.Edit_array(index, value));
app.MapDelete("/Delete_array", [Authorize] () => gs.Delete_array());
app.MapDelete("/Clear_history", [Authorize] () => gs.Clear_history());
app.MapPost("/login", async (string login, string password, HttpContext context) => 
{
    if (!dB.CheckUser(login, password))
    {
        return Results.Unauthorized();
    }
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, login)};
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    return Results.Ok();
});
app.MapPost("/SignUp", (string login, string password) =>
{
    if(dB.CheckByLogin(login))
    {
        return Results.Conflict("Такой пользователь уже существует в системе");
    }
    if (dB.AddUser(login, password))
    {
        return Results.Ok("user" + login + "has been registered successfully");
    }
    else
    {
        return Results.Problem("oh sh.....");
    }
}
);
app.MapGet("/current_user", [Authorize] (HttpContext context) => 
{
    if (context.User.Identity == null)
        return Results.BadRequest("Нет имени пользователя");
    return Results.Ok(context.User.Identity.Name);
});
app.Run();

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

