namespace NailBot
{
    public static class Extension
    {
        public static string Slicer(this string str)
        {
            return str.Length >= 5 ? str.Substring(0, 5) : "default";
        }
    }
}



//namespace NailBot

//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {

//            int res = 2;
//            int count = 0;

//            Console.WriteLine("Введи число");

//            int enter = int.Parse(Console.ReadLine());

//            Sum(res);
//            ShowSum(res);
            //Console.WriteLine($"res res {res}");


            //Console.WriteLine($"count res {count + res}");

            //int[] array = new int[count + 1];
            //array[count] = enter;
            //count++;

            //while (Adder(ref count, ref array));

            //foreach (var item in array)
            //{
            //    res += item;
            //}

            //Console.WriteLine("Сумма введённых элементов " + res);


            //Greet();
            //Greet(Console.ReadLine());

            //string evenOdd = 124.OddEven();
            //Console.WriteLine(evenOdd);


        //}

        //static bool Adder(ref int count, ref int[] array)
        //{
        //    count++;
        //    int enter;

        //    bool result = int.TryParse(Console.ReadLine(), out enter);

        //    int[] newArray = new int[count];

        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        newArray[i] = array[i];
        //    }
        //    newArray[newArray.Length - 1] = enter;

        //    array = newArray;

        //    return result;
        //}

        //static void Greet() { Console.WriteLine("Поздравляю!"); }
        //static void Greet(string name) { Console.WriteLine($"Поздравляю, {name}!"); }

        //static void Sum(int num)
        //{
        //    num = num + 10;
        //}
        //static void ShowSum(int num)
        //{
        //    Console.WriteLine(num);
        //}

        //public static bool group1(int random,ref int tryes, ref int totalTryes)
        //{
        //    Console.ResetColor();
        //    int num = int.Parse(Console.ReadLine());
        //    tryes++;
        //    totalTryes--;
        //    if (num > random)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Blue;
        //        Console.WriteLine("Загаданное число больше");
        //        if (totalTryes > 0)
        //        {
        //            Console.WriteLine("Осталось попыток " + totalTryes);
        //        }
        //        else if (totalTryes == 0)
        //        {
        //            Console.WriteLine("Попытки закончились");
        //            return false;
        //        }
        //    }
        //    else if (num < random)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine("Загаданное число меньше");

        //        if (totalTryes > 0)
        //        {
        //            Console.WriteLine("Осталось попыток " + totalTryes);
        //        }
        //        else if (totalTryes == 0)
        //        { 
        //            Console.WriteLine("Попытки закончились");
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Поздравляю вы угадали");
        //        Console.WriteLine("Количество попыток " + tryes);
        //        return false;
        //    }
        //    return true;
        //}
//    }
//}


//Задание: Игра “Быки и коровы”

//Описание: Напишите консольное приложение на C#, которое реализует игру “Быки и коровы”. 
//В этой игре компьютер загадывает четырехзначное число с уникальными цифрами, а игрок пытается его угадать. 
//После каждой попытки программа сообщает количество “быков” (цифры на правильных позициях) и “коров” 
//(цифры, которые есть в числе, но на неправильных позициях).

//Требования:

//Программа должна генерировать случайное четырехзначное число с уникальными цифрами.
//Пользователь должен вводить свои догадки через консоль.
//После каждой попытки программа должна сообщать количество “быков” и “коров”.
//Программа должна продолжать работу до тех пор, пока пользователь не угадает число.
//После угадывания числа программа должна поздравить пользователя и вывести количество попыток.

//Подсказка по реализации:

//Используйте класс Random для генерации случайного числа.
//Random random = new Random();
//int number = random.Next(0, 10);



//Random random = new Random();
//string number = String.Empty;
//string input = String.Empty;
//bool win = false;

//while (number.Length < 4)
//{
//    int randN = random.Next(0, 10);
//    if (!number.Contains(randN.ToString()))
//        number += randN;
//}

//while (!win)
//{
//    Console.Write("Введите значение: ");
//    input = Console.ReadLine();

//    if (input.Length != 4)
//    {

//    }

//    if (int.TryParse(input, out int n))
//    {

//    }

//    int buls = 0;
//    int cows = 0;

//    if (input == number)
//        break;

//    for (int i = 0; i < number.Length; i++)
//    {
//        if (number[i] == input[i])
//            buls++;
//        else if (number.Contains(input[i]))
//            cows++;
//    }

//    Console.WriteLine($"Bulls {buls}, cows {cows}");
//}
//Console.WriteLine("You win!!!");



//Задание: Угадай число

//Описание: Напишите консольное приложение, которое позволяет пользователю угадывать случайное число от 0 до 100, сгенерированное программой.
//После каждой попытки программа должна выводить сообщение “загаданное число больше” синим цветом или “загаданное число меньше” зеленым цветом,
//в зависимости от того, больше или меньше введенное пользователем число по сравнению с загаданным.

//Требования:

//Программа должна генерировать случайное число от 0 до 100.
//Пользователь должен вводить свои догадки через консоль.
//После каждой попытки программа должна сообщать пользователю, больше или меньше загаданное число, используя соответствующие цвета:
//“Загаданное число больше” - синий цвет.
//“Загаданное число меньше” - зеленый цвет.
//Программа должна продолжать работу до тех пор, пока пользователь не угадает число.
//После угадывания числа программа должна поздравить пользователя и вывести количество попыток.

//Подсказка по реализации:

//Используйте класс Random для генерации случайного числа.
//Random random = new Random();
//int secretNumber = random.Next(0, 101);
//Для изменения цвета текста в консоли используйте свойство Console.ForegroundColor и Console.ResetColor().


//Console.WriteLine("Введите число, и мы проверим оно больше или меньше рандомного");
//int TryCounter = 0;
//Random random = new Random();
//int NewRandom = (int)random.Next(0, 101);
////int NewRandom = 52;

//while (!GroupOne(NewRandom, ref TryCounter)) ;

//public static bool GroupOne(int random, ref int TryCounter)
//{
//    Console.ResetColor();
//    if (int.TryParse(Console.ReadLine(), out int Num))
//    {
//        TryCounter++;

//        if (Num > random)
//        {
//            Console.ForegroundColor = ConsoleColor.Blue;
//            Console.WriteLine("Введенное число больше рандомного");
//        }
//        else if (Num < random)
//        {
//            Console.ForegroundColor = ConsoleColor.Green;
//            Console.WriteLine("Введенное число меньше рандомного");
//        }
//        else
//        {
//            Console.WriteLine("Введенное число равно рандомному");
//            Console.WriteLine($"Количество попыток {TryCounter}");
//            return true;
//        }
//    }
//    else
//    {
//        cw("Неправильный ввод, попробуйте снова")
//    }


//    return false;
//}