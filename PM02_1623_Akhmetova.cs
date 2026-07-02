using System;
using System.IO;
using System.Text;

namespace NorthWestTransportApp;

internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("РЕШЕНИЕ ТРАНСПОРТНОЙ ЗАДАЧИ");
            Console.WriteLine("Метод северо-западного угла");
            Console.WriteLine("1 - Ввести данные вручную");
            Console.WriteLine("2 - Загрузить данные из файла");
            Console.WriteLine("0 - Выход");
            Console.Write("Выберите действие: ");

            try
            {
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    ReadManual(
                        out int[] supply,
                        out int[] demand,
                        out int[,] cost);

                    SolveAndPrint(supply, demand, cost);
                }
                else if (choice == "2")
                {
                    Console.Write("Введите путь к файлу: ");

                    string path = (Console.ReadLine() ?? "").Trim().Trim('"');


                    Console.WriteLine($"Путь: [{path}]");
                    Console.WriteLine($"Файл существует: {File.Exists(path)}");


                    ReadFile(
                        path,
                        out int[] supply,
                        out int[] demand,
                        out int[,] cost);

                    SolveAndPrint(supply, demand, cost);
                }
                else if (choice == "0")
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Неверный пункт меню.");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Ошибка: " + exception.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey(true);
            Console.Clear();
        }
    }

    // Ввод транспортной задачи вручную.
    static void ReadManual(
        out int[] supply,
        out int[] demand,
        out int[,] cost)
    {
        int suppliers = ReadPositiveInt("Количество поставщиков: ");
        int consumers = ReadPositiveInt("Количество потребителей: ");

        supply = ReadArray("Запасы через пробел: ", suppliers);
        demand = ReadArray("Потребности через пробел: ", consumers);

        cost = new int[suppliers, consumers];

        Console.WriteLine("Введите строки матрицы стоимостей:");

        for (int i = 0; i < suppliers; i++)
        {
            int[] row = ReadArray(
                $"Строка {i + 1}: ",
                consumers);

            for (int j = 0; j < consumers; j++)
            {
                cost[i, j] = row[j];
            }
        }
    }

    // Чтение транспортной задачи из текстового файла.
    static void ReadFile(
        string path,
        out int[] supply,
        out int[] demand,
        out int[,] cost)
    {
        if (!File.Exists(path))
        {
            throw new Exception("Файл не найден.");
        }

        string[] lines = File.ReadAllLines(path);

        if (lines.Length < 5)
        {
            throw new Exception("В файле недостаточно данных.");
        }

        int suppliers = int.Parse(lines[0].Trim());
        int consumers = int.Parse(lines[1].Trim());

        if (suppliers <= 0 || consumers <= 0)
        {
            throw new Exception(
                "Количество поставщиков и потребителей должно быть больше нуля.");
        }

        if (lines.Length != suppliers + 4)
        {
            throw new Exception("Неверное количество строк в файле.");
        }

        supply = ParseArray(lines[2], suppliers);
        demand = ParseArray(lines[3], consumers);

        cost = new int[suppliers, consumers];

        for (int i = 0; i < suppliers; i++)
        {
            int[] row = ParseArray(lines[i + 4], consumers);

            for (int j = 0; j < consumers; j++)
            {
                cost[i, j] = row[j];
            }
        }
    }

    // Балансировка задачи с помощью фиктивного поставщика или потребителя.
    static string BalanceTask(
        ref int[] supply,
        ref int[] demand,
        ref int[,] cost)
    {
        int supplySum = GetSum(supply);
        int demandSum = GetSum(demand);

        if (supplySum == demandSum)
        {
            return "Задача сбалансирована.";
        }

        // Потребностей больше: добавляем фиктивного поставщика.
        if (supplySum < demandSum)
        {
            int difference = demandSum - supplySum;

            int[] newSupply = new int[supply.Length + 1];

            for (int i = 0; i < supply.Length; i++)
            {
                newSupply[i] = supply[i];
            }

            newSupply[newSupply.Length - 1] = difference;

            int[,] newCost = new int[
                supply.Length + 1,
                demand.Length];

            for (int i = 0; i < supply.Length; i++)
            {
                for (int j = 0; j < demand.Length; j++)
                {
                    newCost[i, j] = cost[i, j];
                }
            }

            supply = newSupply;
            cost = newCost;

            return
                $"Добавлен фиктивный поставщик с запасом {difference}.";
        }

        // Запасов больше: добавляем фиктивного потребителя.
        int differenceForConsumer = supplySum - demandSum;

        int[] newDemand = new int[demand.Length + 1];

        for (int j = 0; j < demand.Length; j++)
        {
            newDemand[j] = demand[j];
        }

        newDemand[newDemand.Length - 1] = differenceForConsumer;

        int[,] newCostWithConsumer = new int[
            supply.Length,
            demand.Length + 1];

        for (int i = 0; i < supply.Length; i++)
        {
            for (int j = 0; j < demand.Length; j++)
            {
                newCostWithConsumer[i, j] = cost[i, j];
            }
        }

        demand = newDemand;
        cost = newCostWithConsumer;

        return
            $"Добавлен фиктивный потребитель с потребностью {differenceForConsumer}.";
    }

    // Решение задачи и вывод результата.
    static void SolveAndPrint(
        int[] supply,
        int[] demand,
        int[,] cost)
    {
        string balanceMessage = BalanceTask(
            ref supply,
            ref demand,
            ref cost);

        int[,] plan = SolveNorthWestCorner(supply, demand);
        int totalCost = CalculateCost(plan, cost);

        string result =
            balanceMessage +
            Environment.NewLine +
            Environment.NewLine +
            CreateResultText(
                supply,
                demand,
                cost,
                plan,
                totalCost);

        Console.WriteLine();
        Console.WriteLine(result);

        File.WriteAllText(
            "result.txt",
            result,
            Encoding.UTF8);

        Console.WriteLine("Результат сохранён в файл result.txt");
    }

    // Метод северо-западного угла.
    static int[,] SolveNorthWestCorner(
        int[] supplyInput,
        int[] demandInput)
    {
        int[] supply = (int[])supplyInput.Clone();
        int[] demand = (int[])demandInput.Clone();

        int[,] plan = new int[supply.Length, demand.Length];

        int i = 0;
        int j = 0;

        while (i < supply.Length && j < demand.Length)
        {
            int value = Math.Min(supply[i], demand[j]);

            plan[i, j] = value;

            supply[i] -= value;
            demand[j] -= value;

            if (supply[i] == 0)
            {
                i++;
            }

            if (demand[j] == 0)
            {
                j++;
            }
        }

        return plan;
    }

    // Расчёт общей стоимости.
    static int CalculateCost(
        int[,] plan,
        int[,] cost)
    {
        int total = 0;

        for (int i = 0; i < plan.GetLength(0); i++)
        {
            for (int j = 0; j < plan.GetLength(1); j++)
            {
                total += plan[i, j] * cost[i, j];
            }
        }

        return total;
    }

    // Формирование текста результата.
    static string CreateResultText(
        int[] supply,
        int[] demand,
        int[,] cost,
        int[,] plan,
        int totalCost)
    {
        StringBuilder text = new StringBuilder();

        text.AppendLine("ИСХОДНЫЕ ДАННЫЕ");
        text.AppendLine("Запасы: " + string.Join(" ", supply));
        text.AppendLine("Потребности: " + string.Join(" ", demand));

        text.AppendLine();
        text.AppendLine("Матрица стоимостей:");
        AddMatrix(text, cost);

        text.AppendLine();
        text.AppendLine("ПЛАН ПЕРЕВОЗОК:");
        AddMatrix(text, plan);

        text.AppendLine();
        text.AppendLine("Общая стоимость: " + totalCost);

        return text.ToString();
    }

    // Добавление матрицы в текст результата.
    static void AddMatrix(
        StringBuilder text,
        int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                text.Append(matrix[i, j] + "\t");
            }

            text.AppendLine();
        }
    }

    // Подсчёт суммы элементов массива.
    static int GetSum(int[] values)
    {
        int sum = 0;

        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    // Ввод положительного целого числа.
    static int ReadPositiveInt(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(Console.ReadLine(), out int number) &&
                number > 0)
            {
                return number;
            }

            Console.WriteLine("Введите целое число больше нуля.");
        }
    }

    // Ввод строки целых неотрицательных чисел.
    static int[] ReadArray(
        string message,
        int expectedCount)
    {
        while (true)
        {
            Console.Write(message);

            try
            {
                return ParseArray(
                    Console.ReadLine(),
                    expectedCount);
            }
            catch
            {
                Console.WriteLine(
                    $"Введите {expectedCount} целых неотрицательных чисел через пробел.");
            }
        }
    }

    // Преобразование строки в массив целых чисел.
    static int[] ParseArray(
        string line,
        int expectedCount)
    {
        string[] parts = line.Split(
            new[] { ' ', '\t', ';', ',' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != expectedCount)
        {
            throw new Exception();
        }

        int[] values = new int[expectedCount];

        for (int i = 0; i < expectedCount; i++)
        {
            if (!int.TryParse(parts[i], out values[i]) ||
                values[i] < 0)
            {
                throw new Exception();
            }
        }

        return values;
    }
}
