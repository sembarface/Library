namespace Library;

internal sealed class University
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal sealed class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int UniversityId { get; set; }
    public string StudentGroup { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

internal sealed class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Cost { get; set; }
}

internal sealed class BookLoan
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PaymentAmount { get; set; }
}

internal sealed class BookLoanItem
{
    public int LoanId { get; set; }
    public int BookId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool Lost { get; set; }
}
