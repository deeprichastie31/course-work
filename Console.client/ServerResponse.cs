public struct ServerResponse // структура, которая имеет возможность преобразовывать в json не только массивы
{
    public int[]? values { get; set;} 
    public string message { get; set;} 
    public int? singleValue { get; set;} 
}
public struct ResponseForHistory
{
    public string operation{get; set;} // тип совершенной операции
    public object parameters {get; set;} // параметры, использованные
    public int[] prev {get; set;} // сохраняет состояние массива 
    public object result {get; set;} // результат (усешно/ не успешно)
}