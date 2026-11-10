namespace Lab4.Core;

// Классы предметной области из лабораторной работы 2 (вариант 7 — Университет).
public abstract class Person
{
    public string Name { get; set; }
    public string Department { get; set; }

    protected Person(string name, string department)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "Имя не может быть пустым.");
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentNullException(nameof(department), "Кафедра не может быть пустой.");

        Name = name;
        Department = department;
    }

    public override string ToString()
    {
        return $"{GetType().Name}: Name={Name}, Department={Department}";
    }

    // Поиск людей по кафедре.
    public static List<Person> SearchByDepartment(List<Person> people, string dept)
    {
        return people.Where(p => p.Department == dept).ToList();
    }
}

public class Student : Person
{
    public double AverageScore { get; set; }

    public Student(string name, string department, double averageScore)
        : base(name, department)
    {
        if (averageScore < 0 || averageScore > 5)
            throw new ArgumentOutOfRangeException(nameof(averageScore), "Средний балл должен быть от 0 до 5.");

        AverageScore = averageScore;
    }

    public override string ToString()
    {
        return base.ToString() + $", AverageScore={AverageScore:F2}";
    }
}

public class Teacher : Person
{
    public int Workload { get; set; }

    public Teacher(string name, string department, int workload)
        : base(name, department)
    {
        if (workload < 0)
            throw new ArgumentOutOfRangeException(nameof(workload), "Нагрузка не может быть отрицательной.");

        Workload = workload;
    }

    public override string ToString()
    {
        return base.ToString() + $", Workload={Workload} ч/week";
    }
}

public class Postgrad : Person
{
    public string ResearchTopic { get; set; }

    public Postgrad(string name, string department, string researchTopic)
        : base(name, department)
    {
        if (string.IsNullOrWhiteSpace(researchTopic))
            throw new ArgumentNullException(nameof(researchTopic), "Тема исследования не может быть пустой.");

        ResearchTopic = researchTopic;
    }

    public override string ToString()
    {
        return base.ToString() + $", ResearchTopic={ResearchTopic}";
    }
}
