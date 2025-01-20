public struct ServerResponse // структура, которая имеет возможность преобразовывать в json не только массивы
{
    public string message { get; set;} 
    public int[]? values { get; set;} 
    public int? singleValue { get; set;} 
}
public class ResponseForHistory
{
    public string? operation{get; set;} // тип совершенной операцииЫ
    public object? parameters {get; set;} // параметры, использованные
    public string? result {get; set;} // результат (усешно/ не успешно)
}