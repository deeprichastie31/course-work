using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IO.Compression;
using Azure;
using Microsoft.IdentityModel.Tokens;
public class History
{
    public string? Operation{get; set;} // тип совершенной операции
    public object? Parameters {get; set;} // параметры, использованные
    public string? Result {get; set;} // результат (усешно/ не успешно)

}

public class RGSortAdapter
{

    
    // какую структурку использовать для истории. 
    private GnomeSort gs = new GnomeSort();
    private Stack<History> history = new Stack<History>();
  

    public async Task<IResult> LogIn(string login, string password, HttpContext context)
    {
        if (login == "user" &&  password == "password")
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, login)};
            var claimidentity = new ClaimsIdentity(claims, "Cookies");
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimidentity));

            return Results.Ok();
        }
        return Results.Unauthorized();
    }
    public IResult test()
    {
        var response = new RGValues("Привет, я доступен!");
        return Results.Json(response, statusCode:200);
    }
    //со стороны клиента я не ожидаю ошибок
    public IResult Create_array(string array) // здесь json конвертится в пустую строку даже без ключей
    {
        gs.Create_array(array);
        Add_to_history("Создание массива", new {array}, "Массив вручную успешно создан!", true);
        var response = new RGValues("Массив вручную успешно создан!");
        return Results.Json(response, statusCode: 200);
        
    }
    public IResult Edit_array(int index, int value)
    {
        if(!gs.Edit_array(index, value))
        {
            Add_to_history("Корректировка массива", new {index, value}, "Корректировка массива не прошла успешно!", false);
            var response_1 = new RGValues("Были введены неккоректные значения");
            return Results.Json(response_1, statusCode: 400);
        }
        int[] array = gs.Get_array();
        Add_to_history("Корректировка массива", new {array}, "Массив успешно скорректирован!", true);
        var response_2 = new RGValues("Массив скорректирован!");
        return Results.Json(response_2, statusCode: 200);
    }
    public IResult Add_element(int element, string position, int index)
    {
        if(!gs.Add_element(element, position, index))
        {
            Add_to_history("Добавление элемента в массив", new {element, position, index}, "Добавление элемента в массив не прошло успешно!", false);
            var response_1 = new RGValues("Перепроверьте введенные данные");
            return Results.Json(response_1, statusCode: 400);
        }
            Add_to_history("Добавление элемента в массив", new {element, position, index}, "Добавление элемента в массив прошло успешно!", false);
            var response_2 = new RGValues("Элемент успешно добавлен!");
            return Results.Json(response_2, statusCode: 200);
    }

    public IResult Generate_array(int len, int lb, int ub) // вот тут json нормально себя ведет
    {
        if (!gs.Generate_array(len, lb, ub)) // вот тут вызывается 
        {
            Add_to_history("Генерация массива", new {len, lb, ub}, "генерация массива прошла не успешно!", false);
            var response_1 = new RGValues("Неккоректно введены параметры создаваемого массива");
            return Results.Json(response_1, statusCode: 400);
        }
        int[] array = gs.Get_array();
        Add_to_history("генерация массива", new {array}, "Генерация массива прошла успешно!", true);
        var response_2 = new RGValues("Массив был успешно сгенерирован!");
        return Results.Json(response_2, statusCode: 200);
    }
    public IResult Get_array()
    {
        int[] array = gs.Get_array();
        Add_to_history("Получение массива", new {array}, "Массив был успешно получен!", false);
        var response = new RGValues("Массив:", array);
        return Results.Json(response, statusCode: 200);
    }
    public IResult Get_part_array(int low_ind, int up_ind)
    {
        if (low_ind > up_ind || low_ind > gs.Get_array().Length || up_ind > gs.Get_array().Length)
        {
            Add_to_history("Получение части массива", new {low_ind, up_ind}, "Не удалось получить срез!", false);
            var response_1 = new RGValues("Границы среза были выбраны неккоректно");
            return Results.Json(response_1, statusCode:400);
        }
        if (low_ind == up_ind)
        {
            Add_to_history("Получение части массива", new {low_ind, up_ind}, "Срез получен, однако для этого был вызван другой метод, так как границы среза равны", false);
            var response_2 = new RGValues("Массив после среза:", gs.Get_element(low_ind));
            return Results.Json(response_2, statusCode:200);
        }
        Add_to_history("Получение части массива", new {low_ind, up_ind}, "Срез успешно получен!",false);
        var response_3 = new RGValues("Массив после среза:", gs.Get_part_array(low_ind, up_ind));
        return Results.Json(response_3, statusCode: 200);
    } 
    public IResult Get_element(int index)
    {
        if (index < 0|| index >= gs.Get_array().Length)
        {
            Add_to_history("Получение элемента по индексу", new {index}, "Не удалось получить элемент!", false);
            var response_1 = new RGValues("Неккоретно веден индекс элемента, который вы хотите взять");
            return Results.Json(response_1, statusCode: 400);
        }
        int element = gs.Get_element(index);
        Add_to_history("Получение элемента по индексу", new {element , index}, "Элемент успешно получен!", false);
        var response_2 = new RGValues("Элемент:", element);
        return Results.Json(response_2, statusCode: 200);
    }
    public IResult Delete_array()
    {
        var response = new RGValues(gs.Delete_array());
        Add_to_history("Удаление массива", new{}, response.Message, true);
        
        return Results.Json(response, statusCode: 200);
    }
    public IResult GnomeSortic()
    {
        if (!gs.GnomeSortic())
        {
            Add_to_history("Сортировка массива", 0, "Массив был пуст!", false);
            var response_1 = new RGValues("Массив был пуст перед сортировкой!");
            return Results.Json(response_1, statusCode: 409);
        }
        Add_to_history("Сортировка массива",new {array = gs.Get_array()}, "Массив успешно отсортирован!", true);
        var response_2 = new RGValues("Массив был успешно отсортирован!");
        return Results.Json(response_2, statusCode:200);
    }
    public IResult Get_history()
    {
        return Results.Json(history.ToList());
    }
    private void Add_to_history(string operation, object parameter, string result, bool action)
    {
        if (!action)
        {
            history.Push( new History
            {
            Operation = operation, 
            Parameters = parameter, 
            Result = result
            });
            return;
        }
        history.Push(new History
        {
            Operation = operation, 
            Parameters = parameter, 
            Result = result,
        });
    }
    // В данном случае исключение обрабатывать не нужно, так как от пользователя никаких данных не передается и ему нужно просто выводить сообщение
    public IResult Clear_history()
    {
        history.Clear();
        var response_3 = new RGValues("История успешно очищена!");
        return Results.Json(response_3, statusCode:200);
    }
}




/*
28.11.24 - добавил историю запросов, котора основана на структуре данных - стек.
Методы:
1) добавление в очередь. Каждый элемент очереди - это самописная структура, которая содержит 4 поля:
1.1 Совершенное действие(string)
1.2 Поле параметров (например, индексы, границы и тп)



*/