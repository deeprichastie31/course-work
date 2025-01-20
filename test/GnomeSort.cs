public class GnomeSort
{
    public int[] array = Array.Empty<int>();  // нужно для очистки. После этого нужно везде проверку добавить
    private Random random = new Random();
    public int low_b, up_b;

     // длину массива можно записать, чтобы сравнивать с передаваемым индексом 
    public GnomeSort() // регулирование значений элементов массива
    {
    }
    // было решено сделать так, чтобы очистить основной класс
    public bool Create_array(string arrays) // приду сделаю так, чтобы значения вводились на клиенте, а здесь было переприсваивание массива
    {
        var numbers = arrays.Split(',').Select(int.Parse).ToArray(); // преобразование строки в массив

        array = new int[numbers.Length];

        for (int i = 0; i < numbers.Length; i++)
        {
        array[i] = numbers[i];
        }
    return true;
    }
    public bool Edit_array(int index, int value) // Лучше использовать bool, чтобы чаще использовать get_array.
    {
        if (index<0 || value > up_b || value< low_b || index> array.Length)
        {
            return false;
        }
        array[index] = value;
        return true;
    }
    public bool Generate_array(int len, int low_b, int up_b) // если я хочу сделать метод булевым, то мне придется здесь условия проверять, тогда метод будет не тепличным.
    {
        if (len <= 0 || low_b> up_b)
            return false;
        this.low_b = low_b;
        this.up_b = up_b;
        array = new int[len];
        for (int i = 0; i< len; i++)
        {
            array[i] = random.Next(low_b, up_b + 1);
        }
        return true;
    }
    public int[] Get_array()
    {
        return array;
    }
    public int[] Get_part_array(int low_ind, int up_ind)
    {
        int[] slice = array[low_ind..up_ind]; // своего рода срез

        return slice;
    }
    public int Get_element(int index)
    {
        return array[index];
    }
    public int[] GnomeSortic()
    {
        int index = 0;

        while (index < array.Length)
        {
            if (index == 0 || array[index - 1] <= array[index])
            {
                index++; // Двигаемся вперёд, если порядок соблюдён
            }
            else
            {
                // Меняем элементы местами
                (array[index - 1], array[index]) = (array[index], array[index - 1]);
                index--; // Двигаемся назад
            }
        }
        return array;
    }
    public string Delete_array()
    {
        Array.Clear(array);
        return "Массив был успешно удален";
    }
    public void Go_back_array(int[] prev_array) // переустанавливает знаение текущего массива на предыдущий, а это самое главное 
    {
        this.array = prev_array;
    }
}
/// пояснительные моменты: 13.11.2024 - проект не хотел компилироваться из-за базы данных. Я ее добавил в проект, но не реализовал