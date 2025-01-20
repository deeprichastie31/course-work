using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http.HttpResults; // Подключаем пространство имен для JsonHttpResult
using System.Text.Json;
using Newtonsoft.Json.Bson;

namespace UnitTests;

[TestClass]
public class UnitTest1
{
    public RGSortAdapter gs = new RGSortAdapter();

    [TestMethod]

    // Схема такова: ответ приходит в формате jsonHttpResult с полями, в которых есть необходимый мне json RGValues. Его не нужно в строку обрабатывать
    //, его нужно обработать в JsonHttpResult, чтобы я смог обратиться к необходимому мне полю и провести сравнение ключей-значений в этом json.
    public void TestGet_array_CreateArray()
{
    // Arrange: создаём объект класса и строку для инициализации
    string array = "5,4,3,2,1";
    int[] expectedArray = { 5, 4, 3, 2, 1 };

    // Act: инициализируем массив методом Create_array и вызываем Get_array
    IResult isCreated = gs.Create_array(array);
    IResult actualArray = gs.Get_array(); // Получаем массив

    Assert.IsNotNull(isCreated, "Создание массива должно быть успешным.");
    //Assert.IsInstanceOfType(actualArray, typeof(JsonHttpResult<RGValues>), "Ожидался JsonHttpResult."); // из-за полиморфизма, он проходит проверку

    // Приводим actualArray к типу JsonHttpResult<RGValues>
    var jsonResult = actualArray as JsonHttpResult<RGValues>;
    var jsonResult_2 = isCreated as JsonHttpResult<RGValues>;
    Assert.IsNotNull(jsonResult, "Не удалось преобразовать результат к JsonHttpResult.");

    // Извлекаем данные из поля Value
    RGValues responseObject = jsonResult.Value;
    RGValues responseObject_2 = jsonResult_2.Value;
    Assert.IsNotNull(responseObject, "Ответ должен содержать данные.");

    // Assert: проверяем данные
    Assert.AreEqual("Массив вручную успешно создан!", responseObject_2.Message); // Проверка сообщения
    Assert.AreEqual("Массив:", responseObject.Message); // Проверка сообщения
    CollectionAssert.AreEqual(expectedArray, responseObject.Values); // Проверка массива
    
}
    [TestMethod]
    public void TestGet_array_GenerateArray_Success()
    {
        // Arrange
        int len = 10; // Длина массива
        int lb = 1;  // Нижняя граница
        int ub = 10; // Верхняя граница

        IResult isGenerated = gs.Generate_array(len, lb, ub);
        IResult generatedArray = gs.Get_array();
        
        var jsonResult_1 = isGenerated as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        var jsonResult_2 = generatedArray as JsonHttpResult<RGValues>;
        var responseObject_2 = jsonResult_2.Value;
    
        // Проверка содержимого массива
        Assert.AreEqual("Массив был успешно сгенерирован!", responseObject_1.Message);
        Assert.AreEqual(len, responseObject_2.Values.Length, "Длина массива должна совпадать с заданной.");
    }

    [TestMethod]
    // тут неважно какие данные именно неправильные, потому что сообщение всегда будет одно 
    public void TestGenerateArray_Failure()
    {
        // Arrange
        int len = -5; // Некорректная длина массива
        int lb = 1;
        int ub = 10;

        IResult isGenerated = gs.Generate_array(len, lb, ub);

        var jsonResult_1 = isGenerated as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
    
        // Проверка содержимого массива
        Assert.AreEqual("Неккоректно введены параметры создаваемого массива", responseObject_1.Message);
    }
    [TestMethod]
    public void TestEditArray_Success()
    {
        int index = 1;
        int value = 2;
        string array = "1,3,3,4,5";
        int[] expected_array = {1, 2, 3, 4, 5};
        gs.Create_array(array);
        IResult isEdited = gs.Edit_array(index, value);
        IResult EditedArray = gs.Get_array();
        var jsonResult_2 = isEdited as JsonHttpResult<RGValues>;
        var jsonResult_3 = EditedArray as JsonHttpResult<RGValues>;
        var responseObject_2 = jsonResult_2.Value;
        var responseObject_3 = jsonResult_3.Value;

        Assert.AreEqual("Массив скорректирован!", responseObject_2.Message);
        CollectionAssert.AreEqual(responseObject_3.Values, expected_array);
    }

    [TestMethod]
    public void TestEditArray_Failure()
    {
        int index = 5;
        int value = 2;
        string array = "1,3,3,4,5";
        int[] expected_array = {1, 3, 3, 4, 5};
        gs.Create_array(array);
        IResult isEdited = gs.Edit_array(index, value);
        IResult EditedArray = gs.Get_array();
        var jsonResult_2 = isEdited as JsonHttpResult<RGValues>;
        var jsonResult_3 = EditedArray as JsonHttpResult<RGValues>;
        var responseObject_2 = jsonResult_2.Value;
        var responseObject_3 = jsonResult_3.Value;

        Assert.AreEqual("Были введены неккоректные значения", responseObject_2.Message);
        CollectionAssert.AreEqual(responseObject_3.Values, expected_array);
    }
    [TestMethod]
    // Для проверки достаточно просто создать массив без проверки, потому что данный метод уже был проверен 
    // и с присваиванием в переменную получение части массива
    public void TestGetPartArray_Success()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {4, 3};
        int low_ind = 1;
        int up_ind = 3;
        gs.Create_array(array);
        IResult isTakenPart = gs.Get_part_array(low_ind, up_ind);
        var jsonResult = isTakenPart as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        Assert.AreEqual("Массив после среза:", responseObject.Message);
        CollectionAssert.AreEqual(responseObject.Values, expectedArray);
    }
    [TestMethod]
    public void TestGetPartArray_Failure()
    {
        string array = "5,4,3,2,1";
        int low_ind = 3;
        int up_ind = 2;
        gs.Create_array(array);
        IResult isTakenPart = gs.Get_part_array(low_ind, up_ind);
        var jsonResult = isTakenPart as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        Assert.AreEqual("Границы среза были выбраны неккоректно", responseObject.Message);
    }
    [TestMethod]
    public void TestGetElement_Success()
    {
        string array = "5,4,3,2,1";
        int expectedElement = 2;
        int index = 3;
        gs.Create_array(array);
        IResult isTakenElement = gs.Get_element(index);
        var jsonResult = isTakenElement as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        Assert.AreEqual("Элемент:", responseObject.Message);
        Assert.AreEqual(responseObject.SingleValue, expectedElement);
    }
    [TestMethod]
    public void TestGetElement_Failure()
    {
        string array = "5,4,3,2,1";
        int index = 6;
        gs.Create_array(array);
        IResult isTakenElement = gs.Get_element(index);
        var jsonResult = isTakenElement as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        Assert.AreEqual("Неккоретно веден индекс элемента, который вы хотите взять", responseObject.Message);
    }
    [TestMethod]
    public void TestDeleteArray()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {};
        gs.Create_array(array);
        IResult isDeleted = gs.Delete_array();
        IResult isGet = gs.Get_array();
        var jsonResult_1 = isDeleted as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        var jsonResult_2 = isGet as JsonHttpResult<RGValues>;
        var responseObject_2 = jsonResult_2.Value;
        Assert.AreEqual("Массив был успешно удален", responseObject_1.Message);
        CollectionAssert.AreEqual(responseObject_2.Values, expectedArray);
    }
    [TestMethod]
    public void TestAdd_element_begin()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {10,5,4,3,2,1};
        string position = "начало";
        int element = 10;
        gs.Create_array(array);
        IResult isAdded = gs.Add_element(element, position, 0);
        var jsonResult = isAdded as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        IResult isGet = gs.Get_array();
        var jsonResult_1 = isGet as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        Assert.AreEqual(responseObject.Message, "Элемент успешно добавлен!");
        CollectionAssert.AreEqual(responseObject_1.Values, expectedArray);
    }
    [TestMethod]
    public void TestAdd_element_end()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {5,4,3,2,1,10};
        string position = "конец";
        int element = 10;
        gs.Create_array(array);
        IResult isAdded = gs.Add_element(element, position, 0);
        var jsonResult = isAdded as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        IResult isGet = gs.Get_array();
        var jsonResult_1 = isGet as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        Assert.AreEqual(responseObject.Message, "Элемент успешно добавлен!");
        CollectionAssert.AreEqual(responseObject_1.Values, expectedArray);
    }
    [TestMethod]
    public void TestAdd_element_between()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {5,4,10,3,2,1};
        string position = "после";
        int element = 10;
        gs.Create_array(array);
        IResult isAdded = gs.Add_element(element, position, 1);
        var jsonResult = isAdded as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        IResult isGet = gs.Get_array();
        var jsonResult_1 = isGet as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        Assert.AreEqual(responseObject.Message, "Элемент успешно добавлен!");
        CollectionAssert.AreEqual(responseObject_1.Values, expectedArray);
    }
    [TestMethod]
    public void TestSortArray_Success()
    {
        string array = "5,4,3,2,1";
        int[] expectedArray = {1,2,3,4,5};
        gs.Create_array(array);
        IResult isSorted = gs.GnomeSortic();
        var jsonResult_1 = isSorted as JsonHttpResult<RGValues>;
        var responseObject_1 = jsonResult_1.Value;
        IResult isGet = gs.Get_array();
        var jsonResult_2 = isGet as JsonHttpResult<RGValues>;
        var responseObject_2 = jsonResult_2.Value;
        Assert.AreEqual(responseObject_1.Message, "Массив был успешно отсортирован!");
        CollectionAssert.AreEqual(responseObject_2.Values, expectedArray);

    }
    [TestMethod]
    public void TestSortArray_Failure()
    {
        string array = "5,4,3,2,1";
        gs.Create_array(array);
        gs.Delete_array();
        IResult isSorted = gs.GnomeSortic();
        var jsonResult = isSorted as JsonHttpResult<RGValues>;
        var responseObject = jsonResult.Value;
        Assert.AreEqual(responseObject.Message, "Массив был пуст перед сортировкой!");
    }


}
