using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IO.Compression;


public struct History
{
    public string Operation{get; set;} // тип совершенной операции
    public object Parameters {get; set;} // параметры, использованные
    public object Result {get; set;} // результат (усешно/ не успешно)
    public int[] Prev {get; set;} // сохраняет состояние массива 
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
        return Results.Ok(new RGValues("Привет, я доступен!"));
    }
    // со стороны клиента я не ожидаю ошибок
    public IResult Create_array(string array) // здесь json конвертится в пустую строку даже без ключей
    {
        gs.Create_array(array);
        Add_to_history("Создание массива", new {array}, "Массив вручную успешно создан!", true);
        return Results.Ok(new RGValues("Массив вручную успешно создан!"));
        
    }
    public IResult Edit_array(int index, int value)
    {
        if(!gs.Edit_array(index, value))
        {
            Add_to_history("Корректировка массива", new {index, value}, "Корректировка массива не прошла успешно!", false);
            return Results.BadRequest(new RGValues("Были введены неккоректные значения"));
        }
        int[] array = gs.Get_array();
        Add_to_history("Корректировка массива", new {array}, "Массив успешно скорректирован!", true);
        return Results.Ok(new RGValues("Массив скорректирован!"));
    }

    public IResult Generate_array(int len, int lb, int ub) // вот тут json нормально себя ведет
    {
        if (!gs.Generate_array(len, lb, ub)) // вот тут вызывается 
        {
            Add_to_history("Генерация массива", new {len, lb, ub}, "генерация массива прошла не успешно!", false);
            return Results.BadRequest(new RGValues("Неккоректно введены параметры создаваемого массива"));
        }
        int[] array = gs.Get_array();
        Add_to_history("генерация массива", new {array}, "Генерация массива прошла успешно!", true);
        return Results.Ok(new RGValues("Массив был успешно сгенерирован!"));
    }
    public IResult Get_array()
    {
        int[] array = gs.Get_array();
        Add_to_history("Получение массива", new {array}, "Массив был успешно получен!", false);
        return Results.Ok(new RGValues("Массив:", array));
    }
    public IResult Get_part_array(int low_ind, int up_ind)
    {
        if (low_ind > up_ind || low_ind > gs.Get_array().Length || up_ind > gs.Get_array().Length)
        {
            Add_to_history("Получение части массива", new {low_ind, up_ind}, "Не удалось получить срез!", false);
            return Results.BadRequest(new RGValues("Границы среза были выбраны неккоректно", 0));
        }
        if (low_ind == up_ind)
        {
            Add_to_history("Получение части массива", new {low_ind, up_ind}, "Срез получен, однако для этого был вызван другой метод, так как границы среза равны", false);
            return Results.Ok(new RGValues("Массив после среза:", gs.Get_element(low_ind)));
        }
        Add_to_history("Получение части массива", new {low_ind, up_ind}, "Срез успешно получен!",false);
        return Results.Ok(new RGValues("Массив после среза:", gs.Get_part_array(low_ind, up_ind)));
    } 
    public IResult Get_element(int index)
    {
        if (index < 0|| index >= gs.Get_array().Length)
        {
            Add_to_history("Получение элемента по индексу", new {index}, "Не удалось получить элемент!", false);
            return Results.BadRequest(new RGValues("Неккоретно веден индекс элемента, который вы хотите взять", 0));
        }
        int element = gs.Get_element(index);
        Add_to_history("Получение элемента по индексу", new {element , index}, "Элемент успешно получен!", false);
        return Results.Ok(new RGValues("Элемент:",element));
    }
    public IResult Delete_array()
    {
        string message = gs.Delete_array();
        Add_to_history("Удаление массива", new{}, message, true);
        return Results.Ok(new RGValues(message));
    }
    public IResult GnomeSortic()
    {
        int[] array = gs.GnomeSortic();
        if (array.Length == 0)
        {
            Add_to_history("Сортировка массива",new {array.Length}, "Массив был пуст!", false);
            return Results.Conflict(new RGValues("Массив был пуст перед сортировкой!", 0));
        }
        Add_to_history("Сортировка массива",new {array}, "Массив успешно отсортирован!", true);
        return Results.Ok(new RGValues("Массив был успешно отсортирован!", array));
    }
    public IResult Get_history()
    {
        return Results.Ok(history);
    }
    private void Add_to_history(string operation, object parameter, object result, bool action)
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
            Prev = (int[])gs.Get_array().Clone() // как раз поэтому ничего и не передается в метод, потому что он сам сохраняет состояние массива. 
            //лучше клонировать массив, чтобы избегать переназначения по ссылке 
        });
    }
    // В данном случае исключение обрабатывать не нужно, так как от пользователя никаких данных не передается и ему нужно просто выводить сообщение
    public IResult Go_back() // метод, который отвечает за обработку запроса вернуться на одну позицию назад
    {
        if (history.Count == 0)
        {
            return Results.Conflict(new RGValues("История запросов пуста!", 0)); 
        }
        var Last_object = history.Pop();

        if (Last_object.Prev == null)
        {
            return Results.Conflict(new RGValues("Откат невозможен, нет предыдущего значения!", 0));
        }
        gs.Go_back_array(Last_object.Prev); // возвращаем состояние массива
        
        return Results.Ok(new RGValues("Откат выполнен успешно!",Last_object.Prev));
    }
    public IResult Clear_history()
    {
        history.Clear();
        return Results.Ok("Историю успешно очищена!");
    }
}




/*
28.11.24 - добавил историю запросов, котора основана на структуре данных - стек.
Методы:
1) добавление в очередь. Каждый элемент очереди - это самописная структура, которая содержит 4 поля:
1.1 Совершенное действие(string)
1.2 Поле параметров (например, индексы, границы и тп)



*/