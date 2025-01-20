﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.VisualBasic;

CookieContainer cookies = new CookieContainer();
HttpClientHandler handler = new HttpClientHandler();

HttpClient client = new HttpClient(handler); // кука сразу прикладывается

/*
Вместо swich/case для реализации главной меню было решено использовать словарь и соответственно будет допилен try catch. Данное решение мне показалось более интересным

Также было сделано по два bool метода для регистрации и авторизации, чтобы в одном методе происходила обработка введенных пользователем данных, а в другом компановка запроса
Это позволяет мне лучше ориентироваться и расширить пространство для обработки различных исключений. Так же это нужно для словаря, я не могу для функции указать тудымс параметры

*/

handler.CookieContainer = cookies;

void Register()
{
    Console.WriteLine("РЕГИСТРАЦИЯ");
    bool flag = false;
    try
    {
        while (!flag)
        {
        Console.WriteLine("Введите логин: ");
        string? username = Console.ReadLine();
        Console.WriteLine("Введите пароль: ");
        string? password = Console.ReadLine();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Неккоректный набор логина или пароля, перепроверьте данные");
        }
        if (!AttemtSignUp(username, password))
        {
            throw new UnauthorizedAccessException("Неккоректно введен логин или пароль, перепроверьте данные");
        }
        else
        {
            flag = true;
            Console.WriteLine("Регистрация прошла успешно");
        }
        }
    }
    catch(ArgumentException exp)
    {
        Console.WriteLine(exp.Message);
    }
    catch (UnauthorizedAccessException exp)
    {
        Console.WriteLine(exp.Message);
    }
}
bool AttemtSignUp(string username, string password)
{
    string request = "/SignUp?login=" + username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode) // в диапозоне от 200 до 300 лежат хорошие запросы, от 400 и выше - плохие
    {
        return true;
    }
    else{
        return false;
    }
}

void LogInUser()
{
    Console.WriteLine("АВТОРИЗАЦИЯ");
    bool flag = false;
    while(!flag)
    {
        try
        {
        Console.WriteLine("Введите логин: ");
        string? username = Console.ReadLine();
        Console.WriteLine("Введите пароль: ");
        string? password = Console.ReadLine();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Введите логин или пароль");
        }
        if (!LoginOnServer(username, password))
        {
            throw new UnauthorizedAccessException("Неверно введен логин или пароль, перепроверьте данные");
        }
        else
        {
            flag = true;
        }
        }
        catch (ArgumentException exp)
        {
            Console.WriteLine(exp.Message);
        }
        catch (UnauthorizedAccessException exp)
        {
            Console.WriteLine(exp.Message);
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
        }
    }
}
bool LoginOnServer(string username, string password) // тут будет проходить сборка запроса
{
    string request = "/login?login=" + username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Авторизация прошла успешно");
        
        IEnumerable<Cookie> response_cookies = cookies.GetAllCookies(); // проходимся по всем кукам
        foreach (Cookie cookie in response_cookies)
        {
            Console.WriteLine(cookie.Name + ":" + cookie.Value); 
        }
        return true;
    }
    else{
        return false;
    }

}

// Отправщик запросов. Нужен для того, чтобы инкапсулировать код отправки запросов и получения ответа
async Task<(string, bool)> requestSender(string request, string action, string? responseWeNeed)
{
    HttpResponseMessage response;
    switch(action.ToLower())
    {
        case "post":
            response = await client.PostAsync(request, null);
            break;
        case "get":
            response = await client.GetAsync(request);
            break;
        case "patch":
            var patchMethod = new HttpMethod("PATCH");
            var requestMessage = new HttpRequestMessage(patchMethod, request) { Content = null};
            response = await client.SendAsync(requestMessage);
            break;
        case "delete":
            response = await client.DeleteAsync(request);
            break;
        default:
            throw new ArgumentException($"Неккоректное действие {action}");
    }
   
    string responseText = await response.Content.ReadAsStringAsync(); // должен получить все, что находится в структуре rgvalues по данному запросу
    if (responseWeNeed == "history")
    {
        ResponseForHistory forHistory = JsonSerializer.Deserialize<ResponseForHistory>(responseText);
        return (forHistory.operation,response.IsSuccessStatusCode);
    }
    ServerResponse responseJson = JsonSerializer.Deserialize<ServerResponse>(responseText);
    switch (responseWeNeed)
    {
        case "messege":
            return (responseJson.message,response.IsSuccessStatusCode);
        case "messege&array":
            return ($"{responseJson.message} {string.Join(", ", responseJson.values)}", response.IsSuccessStatusCode);
        case "messege&singlevalue":
            return (responseJson.message + responseJson.singleValue, response.IsSuccessStatusCode);
        default:
            return (responseText,response.IsSuccessStatusCode);
    }
    
}
// 


int GetValidIntInput(string prompt, int minValue = int.MinValue, int maxValue = int.MaxValue)
{
    int result = 0;
    bool isValid = false;

    while (!isValid)
    {
        Console.WriteLine(prompt);
        string input = Console.ReadLine();
        isValid = int.TryParse(input, out result) && result >= minValue && result <= maxValue;

        if (!isValid)
        {
            Console.WriteLine($"Некорректное значение. Пожалуйста, введите число между {minValue} и {maxValue}.");
        }
    }

 return result;
}
// после того, как пользователь авторизуется или пройдет регистрацию, его перенесет в новое меню, где он сможет выбрать что делать с массивом
// переделать структуру запросов.
bool flag = false;
while (!flag)
{

    const string DEFAULT_WAY = "http://localhost:5000";
    Console.WriteLine("Введите url сервера, к которому вы хотите подключиться (по умолчанию стоит http://localhost:5000 - ENTER)");

    string? server_url = Console.ReadLine(); 

    if (server_url == null || server_url.Length == 0) // нажатие enter - подключение к тестовому
    {
        server_url = DEFAULT_WAY;
    }   
    // основные проверки: корректность адреса и отправка тестового запроса. Если ответа не будет, то сервера или не существует, или он выключен.
    try // если адрес дефолтный, то все проверки он пройдет
    {
        client.BaseAddress = new Uri(server_url, UriKind.Absolute); // преобразование в uri, проверка формата
        flag = true;
        // string request = "test";
        // var (response, IsSuccessStatusCode) = await requestSender(request, "get", "messege"); 
        // if (IsSuccessStatusCode)
        // {
        //     Console.WriteLine(response);
        //     flag = true;
        // }
        // else
        // {
        //     throw new HttpRequestException();
        // }
    }
    catch (HttpRequestException exp)
    {
        Console.WriteLine("Сервер недоступен" + exp.Message);
    }
    catch (UriFormatException)
    {
        Console.WriteLine("Неккоректно введен адрес сервера");
    }
}

    Console.WriteLine("Главное меню\n1)Авторизоваться\n2)Зарегистрироваться\n");
    try
    {
        int choose = GetValidIntInput("Выберите действие по номеру:",1, 2);
        switch(choose)
        {
            case 1:
                LogInUser();
                flag = true;
                break;
            case 2:
                Register();
                Console.WriteLine("После регистрации необходимо авторизоваться с введенными ранее данными");
                LogInUser();
                flag = true;
                break;
        }
    }
    
    catch (Exception exp)
    {
        Console.WriteLine(exp.Message);
    }


flag = false;
int k = 0;
while(!flag)
{
    Console.WriteLine("Вы успешно авторизовались, теперь вы можете вершить судьбу человечества в этом мире!");
    Console.WriteLine("Выберите что вы хотите сделать с массивом:\n1) Сгенерировать массив автоматически\n2) Создать массив вручную\n3) Отсортировать массив\n4)Вывести массив\n5) Получить часть массива\n6) Получить элемент массива\n7)Скорректировать массив\n8) Удалить массив\n9) Перейти в раздел истории запросов\n10) Выйти из программы");
     // нужно для того, чтобы отслеживать выполнил ли пользователь действия, которые записываются в историю
        int chose = GetValidIntInput("Выбор действия",0, 10);
        switch (chose)
        {
            case 1:
            // Здесь могут быть только неккоректно введены значения, поэтому стоит обрабатывать исключения.
            k++;
            try
            {
                Console.WriteLine("Вы выбрали автоматическую генерацию массива");
                int len = GetValidIntInput("Введите длину массива", 1);
                int lb = GetValidIntInput("Введите нижнюю границу");
                int ub = GetValidIntInput("Введите верхнюю границу", lb + 1); // от нижней границы
                string request_1 = "/Generate_array?len=" + len + "&lb=" + lb + "&ub=" + ub; 
                var(response_1, IsSuccessStatusCode_1) = await requestSender(request_1, "post", "messege"); // надо было сразу понять, как правильно писать запрос в строку.
                if (IsSuccessStatusCode_1)
                {
                    Console.WriteLine(response_1); // обработку json надо замутить
                }
                else
                {
                    throw new ArgumentException(response_1); // могут быть только неккоректно введены значения.
                }
            }
            catch (ArgumentException exp)
            {
                Console.WriteLine(exp.Message);
            }
            break;
            case 2:
                // здесь ошибки с данными быть не может, потому что на этапе проверки корректности введенных данных уйдут все неккоректные варианты
                k++;
                Console.WriteLine("Вы выбрали создать массив вручную");
                int lenchik = GetValidIntInput("Введите длину массива", 1);
                int[] array = new int[lenchik];
                for (int i = 0; i < lenchik; i++)
                {
                    array[i] = GetValidIntInput($"Введите значение элемента {i}");
                }
                string arrayString = string.Join(",", array);
                string request = "/Create_array?array=" + arrayString; // массив отправляется строкой
                var (response_2, IsSuccessStatusCode_2) = await requestSender(request, "post", "messege");
                Console.WriteLine(response_2);
            break;
            case 3:
            // В данном случае неудача может произойти только если массив окажется пустым. Однако пользователь никак не может повлиять на это, поэтому здесь просто сообшение выведется
            k++;
                Console.WriteLine("Вы выбрали отсортировать массив");
                string request_3 = "/Sort_array";
                var (response_3, IsSuccessStatusCode_3) = await requestSender(request_3, "post", "messege");
                Console.WriteLine(response_3);
                break;
            case 4:
                // здесь также неудачи быть не может - обрабатывать исключения не получится, только 500, что сервак не работает
                k++;
                Console.WriteLine("Вы выбрали получить массив");
                string request_4 = "/Get_array";
                var (response_4, IsSuccessStatusCode_4) = await requestSender(request_4, "get", "messege&array");
                Console.WriteLine(response_4);
                
                break;
            case 5:
            // здесь стоило бы обрабатывать исключения, потому что пользователь может задать значения, выходящие за рамки массива, что я не могу четко контролировать
            k++;
            try
            {
                Console.WriteLine("Вы выбрали получить часть массива");
                int low_ind = GetValidIntInput("Введите нижнюю границу среза");
                int up_ind = GetValidIntInput("Введите верхнюю границу среза", low_ind);
                string request_5 = "/Get_part_array?low_ind" + low_ind + "&up_ind" + up_ind;
                var (response_5, IsSuccessStatusCode_5) = await requestSender(request_5, "get", "messege&singlevalue");
                if(IsSuccessStatusCode_5)
                {
                    Console.WriteLine(response_5);
                }
                else
                {
                    throw new ArgumentException(response_5);
                }
            }
            catch (ArgumentException exp)
            {
                Console.WriteLine(exp.Message);
            }
                
            break;
            case 6:
                try
                {
                    Console.WriteLine("Вы выбрали получить элемент массива");
                    int element = GetValidIntInput("Введите индекс массива");
                    string request_6 = "/Get_element?element=" + element;
                    var (response_6, IsSuccessStatusCode_6) = await requestSender(request_6, "get", "messege&singlevalue");
                    if(IsSuccessStatusCode_6)
                    {
                        Console.WriteLine(response_6);
                    }
                    else
                    {
                        throw new ArgumentException(response_6);
                    }
                }
                catch (ArgumentException exp)
                {
                    Console.WriteLine(exp.Message);
                }
                break;
            case 7:
            // Здесь нужно следить за тем, чтобы пользователь не вышел за рамки массива по индексу. Сразу проверить в клиенте по длине массива я не могу
            // поэтому следует ориентироваться по отправленному назад ответу.
            k++;
                try
                {
                    Console.WriteLine("Вы выбрали скорректировать массив");
                    int index = GetValidIntInput("Введите индекс элемента");
                    int value = GetValidIntInput("Введите значение элемента");
                    string request_7 = "/Edit_array?index=" + index + "&value=" + value;
                    var (response_7, IsSuccessStatusCode_7) = await requestSender(request_7, "patch", "messege");
                    if(IsSuccessStatusCode_7)
                    {
                        Console.WriteLine(response_7);
                    }
                    else
                    {
                        throw new ArgumentException(response_7);
                    }
                }
                catch (ArgumentException exp)
                {
                    Console.WriteLine(exp.Message);
                }
                break;
            case 8:
                // здесь по аналогии обрабатывать ничего не нужно.
                k++;
                Console.WriteLine("Вы выбрали удалить массив");
                string request_8 = "/Delete_array";
                var (response_8, IsSuccessStatusCode_8) = await requestSender(request_8, "delete", "messege");
                Console.WriteLine(response_8); 
                break;
            case 9:
                    Console.WriteLine("Вы выбрали работу с историей запросов");
                    if (k == 0)
                    {
                        Console.WriteLine("Не было совершено действий");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("1. Получить историю\n2. Удалить историю\n3. Вернуться к предыдущему состоянию\n4. Выйти");
                        int choose = GetValidIntInput("Выберите что вы хотите сделать", 1, 4);
                        switch(choose)
                        {
                            case 1:
                                Console.WriteLine("Вы выбрали получить историю");
                                string request_91 = "/Get_history";
                                var (response_91, IsSuccessStatusCode_91) = await requestSender(request_91, "get", "history");
                                Console.WriteLine(response_91);
                                break;
                            case 2:
                                Console.WriteLine("Вы выбрали удалить историю");
                                string request_92 = "/Delete_history";
                                var (response_92, IsSuccessStatusCode_92) = await requestSender(request_92, "delete", "messege");
                                Console.WriteLine(response_92);
                                break;
                            case 3:
                                // нет обработки исключения, потому что пользователь не родит здесь числа, чтобы заполнить историю запросов.
                                Console.WriteLine("Вы выбрали вернуться на шаг назад");
                                    var request_93 = "/Go_back";
                                    var (response_93, IsSuccessStatusCode_93) = await requestSender(request_93, "delete", "messege&array");
                                    Console.WriteLine(response_93);           
                                break;
                            default: // default отвечает за обработку числа 4.
                                break;
                        }
                    }
                    break;
            default:
                Console.WriteLine("Выполняется выход из программы.....");
                flag = true;
                break;
        }
}
    
