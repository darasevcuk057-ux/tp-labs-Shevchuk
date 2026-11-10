using Lab4.Core;

namespace Lab4.Tests;

public class UniversityTests
{
    // --- Student ---

    [Fact]
    public void Student_Constructor_SetsProperties()
    {
        var s = new Student("Анна", "Компьютерные науки", 4.5);
        Assert.Equal("Анна", s.Name);
        Assert.Equal("Компьютерные науки", s.Department);
        Assert.Equal(4.5, s.AverageScore);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.1)]
    [InlineData(-1)]
    [InlineData(10)]
    public void Student_InvalidScore_Throws(double score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Student("Иван", "Физика", score));
    }

    [Fact]
    public void Student_BoundaryScores_AreAccepted()
    {
        var s0 = new Student("А", "Б", 0);
        Assert.Equal(0, s0.AverageScore);
        var s5 = new Student("А", "Б", 5);
        Assert.Equal(5, s5.AverageScore);
    }

    // --- Teacher ---

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Teacher_NegativeWorkload_Throws(int workload)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Teacher("Иван", "Физика", workload));
    }

    [Fact]
    public void Teacher_ZeroWorkload_Accepted()
    {
        var t = new Teacher("Иван", "Физика", 0);
        Assert.Equal(0, t.Workload);
    }

    // --- Postgrad ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Postgrad_EmptyTopic_Throws(string? topic)
    {
        Assert.Throws<ArgumentNullException>(() => new Postgrad("Игорь", "Химия", topic!));
    }

    [Fact]
    public void Postgrad_Constructor_SetsTopic()
    {
        var p = new Postgrad("Игорь", "Химия", "Квантовые вычисления");
        Assert.Equal("Квантовые вычисления", p.ResearchTopic);
    }

    // --- Person ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Person_EmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentNullException>(() => new Student(name!, "Кафедра", 4));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Person_EmptyDepartment_Throws(string? dept)
    {
        Assert.Throws<ArgumentNullException>(() => new Student("Имя", dept!, 4));
    }

    // --- SearchByDepartment ---

    [Fact]
    public void SearchByDepartment_FindsMatching()
    {
        var people = new List<Person>
        {
            new Student("Анна", "Компьютерные науки", 4.8),
            new Teacher("Елена", "Компьютерные науки", 15),
            new Student("Борис", "Математика", 4.3),
        };

        var found = Person.SearchByDepartment(people, "Компьютерные науки");
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void SearchByDepartment_NoMatch_ReturnsEmpty()
    {
        var people = new List<Person>
        {
            new Student("Анна", "Компьютерные науки", 4.8),
        };

        var found = Person.SearchByDepartment(people, "Физика");
        Assert.Empty(found);
    }

    [Fact]
    public void SearchByDepartment_EmptyList_ReturnsEmpty()
    {
        var found = Person.SearchByDepartment(new List<Person>(), "Физика");
        Assert.Empty(found);
    }

    // Бонус 1: поиск по кафедре должен находить записи независимо от регистра.
    // В лаб 2 было p.Department == dept — точное сравнение с учётом регистра.
    // Тест выявил: поиск "компьютерные науки" не находил "Компьютерные науки".
    [Fact]
    public void SearchByDepartment_DifferentCase_FindsMatch()
    {
        var people = new List<Person>
        {
            new Student("Анна", "Компьютерные науки", 4.8),
            new Student("Борис", "Математика", 4.3),
        };

        var found = Person.SearchByDepartment(people, "компьютерные науки");
        Assert.Single(found);
        Assert.Equal("Анна", found[0].Name);
    }

    // --- ToString ---

    [Theory]
    [InlineData(typeof(Student), "Анна")]
    [InlineData(typeof(Teacher), "Дмитрий")]
    [InlineData(typeof(Postgrad), "Игорь")]
    public void ToString_ContainsName(Type type, string name)
    {
        Person p = type.Name switch
        {
            "Student" => new Student(name, "Каф", 4),
            "Teacher" => new Teacher(name, "Каф", 10),
            "Postgrad" => new Postgrad(name, "Каф", "Тема"),
            _ => throw new ArgumentException()
        };
        Assert.Contains(name, p.ToString());
    }
}
