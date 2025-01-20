public class GenerateArrayRequest
{
    public int Len { get; set; }
    public int Lb { get; set; }
    public int Ub { get; set; }
}
public class CreateArrayRequest
{
    public int[] array {get; set;}
}
public class GetPartArrayRequest
{
    public int low_ind {get;set;}
    public int up_ind{get;set;}
}

public class EditArrayRequest
{
    public int index {get;set;}
    public int value{get;set;}
}
public class GetElementRequest
{
    public int element {get;set;}
}