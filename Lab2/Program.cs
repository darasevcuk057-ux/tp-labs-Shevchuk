using System;
using System.Collections.Generic;
using System.Linq;

// Базовый класс Person
public abstract class Person
{
    public string Name { get; set; }
    public string Department { get; set; }

    protected Person(string name, string department)
    {
        Name = name;
        Department = department;
    }

    public override string ToString()
    {
        return $"{GetType().Name}: ФИО={Name}, Дисциплина={Department}";
    }

    // Статический метод поиска по кафедре
    public static List<Person> SearchByDepartment(List<Person> people, string dept)
    {
        return people.Where(p => p.Department == dept).ToList();
    }
}

// Студент
public class Student : Person
{
    public double AverageScore { get; set; } // средний балл

    public Student(string name, string department, double averageScore)
        : base(name, department)
    {
        AverageScore = averageScore;
    }

    public override string ToString()
    {
        return base.ToString() + $", Средний балл={AverageScore:F2}";
    }
}

// Преподаватель (профессор)
public class Teacher : Person
{
    public int Workload { get; set; } // нагрузка (часы в неделю)

    public Teacher(string name, string department, int workload)
        : base(name, department)
    {
        Workload = workload;
    }

    public override string ToString()
    {
        return base.ToString() + $", Кол-во часов ={Workload} кол-во/ч";
    }
}

// Аспирант
public class Postgrad : Person
{
    public string ResearchTopic { get; set; } // тема исследования

    public Postgrad(string name, string department, string researchTopic)
        : base(name, department)
    {
        ResearchTopic = researchTopic;
    }

    public override string ToString()
    {
        return base.ToString() + $", Тема исследования={ResearchTopic}";
    }
}

// Точка входа
internal class Program
{
    private static void Main()
    {
        // Создание списка сотрудников и студентов
        var people = new List<Person>
        {
            new Student("Анна Смирнова", "Компьютерные науки", 4.8),
            new Student("Борис Петросян", "Математика", 4.3),
            new Teacher("Дмитрий Иванов", "Физика", 12),
            new Teacher("Елена Соколова", "Компьютерные науки", 15),
            new Postgrad("Игорь Николаев", "Химия", "Квантовые вычисления"),
            new Postgrad("Мария Ковалева", "Химия", "Синтетические методы")
        };

        Console.WriteLine("=== Список всех людей ===");
        foreach (var p in people)
            Console.WriteLine(p);

        // Поиск по кафедре "Компьютерные науки"
        Console.WriteLine("\n=== Поиск по кафедре 'Компьютерные науки' ===");
        var csPeople = Person.SearchByDepartment(people, "Компьютерные науки");
        foreach (var p in csPeople)
            Console.WriteLine(p);

        // Вычисление среднего балла среди студентов
        Console.WriteLine("\n=== Средний балл среди студентов ===");
        var students = people.OfType<Student>();
        double avg = students.Average(s => s.AverageScore);
        Console.WriteLine($"Средний балл: {avg:F2}");

        // Вывод информации о нагрузке преподавателей
        Console.WriteLine("\n=== Нагрузка преподавателей ===");
        var teachers = people.OfType<Teacher>();
        foreach (var t in teachers)
            Console.WriteLine($"{t.Name} — {t.Workload} ч");

        Console.WriteLine("\nНажмите Enter для выхода.");
        Console.ReadLine();
    }
}
