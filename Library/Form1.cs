using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;

namespace Library
{
    public partial class Form1 : Form
    {
        private NpgsqlConnection _connection = null!;

        private DataTable _reportTable = new();
        private Dictionary<string, int> _chartData = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                    ?? "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=library";

                _connection = new NpgsqlConnection(connectionString);
                _connection.Open();

                EnsureDatabaseCompatibility();
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к PostgreSQL: " + ex.Message,
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _connection?.Dispose();
        }

        private void EnsureDatabaseCompatibility()
        {
            Execute("""
                drop index if exists uq_active_book_loan
                """);

            Execute("""
                create unique index if not exists uq_active_book_loan
                on book_loan_items(book_id)
                where return_date is null
                """);
        }

        private void StudentsGrid_SelectionChanged(object? sender, EventArgs e) => LoadLoanItemsForSelectedLoan();
        private void AddUniversityButton_Click(object? sender, EventArgs e) => AddUniversity();
        private void EditUniversityButton_Click(object? sender, EventArgs e) => EditUniversity();
        private void DeleteUniversityButton_Click(object? sender, EventArgs e) => DeleteUniversity();
        private void AddBookButton_Click(object? sender, EventArgs e) => AddBook();
        private void EditBookButton_Click(object? sender, EventArgs e) => EditBook();
        private void DeleteBookButton_Click(object? sender, EventArgs e) => DeleteBook();
        private void AddStudentButton_Click(object? sender, EventArgs e) => AddStudent();
        private void EditStudentButton_Click(object? sender, EventArgs e) => EditStudent();
        private void DeleteStudentButton_Click(object? sender, EventArgs e) => DeleteStudent();
        private void AddIssueButton_Click(object? sender, EventArgs e) => AddIssue();
        private void EditIssueButton_Click(object? sender, EventArgs e) => EditIssue();
        private void DeleteIssueButton_Click(object? sender, EventArgs e) => DeleteIssue();
        private void MarkReturnedButton_Click(object? sender, EventArgs e) => MarkReturned();
        private void MarkLostButton_Click(object? sender, EventArgs e) => MarkLost();
        private void SelectAllReportUniversitiesButton_Click(object? sender, EventArgs e) => SetAllReportUniversities(true);
        private void ClearReportUniversitiesButton_Click(object? sender, EventArgs e) => SetAllReportUniversities(false);
        private void BuildReportButton_Click(object? sender, EventArgs e) => BuildReport();
        private void ExportReportButton_Click(object? sender, EventArgs e) => ExportReport();

        private void BuildInterface()
        {
            Text = "Библиотека выдач книг студентам ВУЗов";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 720);
            Load += Form1_Load;
            FormClosed += Form1_FormClosed;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildUniversitiesTab());
            tabs.TabPages.Add(BuildBooksTab());
            tabs.TabPages.Add(BuildStudentsIssuesTab());
            tabs.TabPages.Add(BuildReportTab());
            Controls.Add(tabs);
        }

        private TabPage BuildUniversitiesTab()
        {
            var tab = new TabPage("ВУЗы");
            tab.Controls.Add(_universitiesGrid);
            tab.Controls.Add(BuildButtonsPanel(
                ("Добавить", (_, _) => AddUniversity()),
                ("Изменить", (_, _) => EditUniversity()),
                ("Удалить", (_, _) => DeleteUniversity())));
            return tab;
        }

        private TabPage BuildBooksTab()
        {
            var tab = new TabPage("Книги");
            tab.Controls.Add(_booksGrid);
            tab.Controls.Add(BuildButtonsPanel(
                ("Добавить", (_, _) => AddBook()),
                ("Изменить", (_, _) => EditBook()),
                ("Удалить", (_, _) => DeleteBook())));
            return tab;
        }

        private TabPage BuildStudentsIssuesTab()
        {
            var tab = new TabPage("Студенты и выдачи");
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 260
            };

            _studentsGrid.SelectionChanged += (_, _) => LoadLoanItemsForSelectedLoan();

            split.Panel1.Controls.Add(_studentsGrid);
            split.Panel1.Controls.Add(BuildButtonsPanel(
                ("Добавить студента", (_, _) => AddStudent()),
                ("Изменить студента", (_, _) => EditStudent()),
                ("Удалить студента", (_, _) => DeleteStudent())));

            split.Panel2.Controls.Add(_issuesGrid);
            split.Panel2.Controls.Add(BuildButtonsPanel(
                ("Добавить выдачу", (_, _) => AddIssue()),
                ("Изменить выдачу", (_, _) => EditIssue()),
                ("Удалить выдачу", (_, _) => DeleteIssue()),
                ("Вернуть книгу", (_, _) => MarkReturned()),
                ("Книга утеряна", (_, _) => MarkLost())));

            tab.Controls.Add(split);
            return tab;
        }

        private TabPage BuildReportTab()
        {
            var tab = new TabPage("Отчет");

            var top = new Panel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(10) };
            var dateLabel = new Label { Text = "Дата отчета", AutoSize = true, Location = new Point(10, 14) };
            _reportDate.Format = DateTimePickerFormat.Short;
            _reportDate.Location = new Point(100, 10);

            var universitiesLabel = new Label { Text = "ВУЗы", AutoSize = true, Location = new Point(10, 47) };
            _reportUniversities.Location = new Point(100, 44);
            _reportUniversities.Size = new Size(350, 90);
            _reportUniversities.CheckOnClick = true;

            var selectAll = CreateButton("Выбрать все", 470, 44);
            selectAll.Click += (_, _) => SetAllReportUniversities(true);
            var clearAll = CreateButton("Снять выбор", 470, 82);
            clearAll.Click += (_, _) => SetAllReportUniversities(false);
            var build = CreateButton("Построить отчет", 635, 44);
            build.Click += (_, _) => BuildReport();
            var export = CreateButton("Экспорт в Excel", 635, 82);
            export.Click += (_, _) => ExportReport();

            top.Controls.AddRange([dateLabel, _reportDate, universitiesLabel, _reportUniversities, selectAll, clearAll, build, export]);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 320
            };
            split.Panel1.Controls.Add(_reportGrid);
            _chartPanel.Dock = DockStyle.Fill;
            _chartPanel.BackColor = Color.White;
            _chartPanel.Paint += DrawChart;
            split.Panel2.Controls.Add(_chartPanel);

            tab.Controls.Add(split);
            tab.Controls.Add(top);
            return tab;
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
        }

        private static Panel BuildButtonsPanel(params (string Text, EventHandler Handler)[] buttons)
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight
            };

            foreach (var buttonInfo in buttons)
            {
                var button = new Button
                {
                    Text = buttonInfo.Text,
                    Width = 150,
                    Height = 32,
                    Margin = new Padding(0, 0, 8, 0)
                };
                button.Click += buttonInfo.Handler;
                panel.Controls.Add(button);
            }

            return panel;
        }

        private static Button CreateButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 145,
                Height = 30
            };
        }

        private void LoadAll()
        {
            LoadUniversities();
            LoadBooks();
            LoadStudents();
            LoadLoans();
            LoadReportUniversities();
        }

        private DataTable FillTable(string sql, params NpgsqlParameter[] parameters)
        {
            using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddRange(parameters);
            using var adapter = new NpgsqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        private int Execute(string sql, params NpgsqlParameter[] parameters)
        {
            using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }

        private object? Scalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var command = new NpgsqlCommand(sql, _connection);
            command.Parameters.AddRange(parameters);
            return command.ExecuteScalar();
        }

        private int GetOrCreateLoan(int studentId, DateTime loanDate)
        {
            var existing = Scalar("""
                select id
                from book_loans
                where student_id = :studentId and loan_date = :loanDate
                order by id
                limit 1
                """,
                new NpgsqlParameter("studentId", studentId),
                new NpgsqlParameter("loanDate", loanDate.Date));

            if (existing != null && existing != DBNull.Value)
            {
                return Convert.ToInt32(existing, CultureInfo.InvariantCulture);
            }

            var created = Scalar("""
                insert into book_loans(student_id, loan_date)
                values(:studentId, :loanDate)
                returning id
                """,
                new NpgsqlParameter("studentId", studentId),
                new NpgsqlParameter("loanDate", loanDate.Date));

            return Convert.ToInt32(created, CultureInfo.InvariantCulture);
        }

        private void DeleteEmptyLoan(int loanId)
        {
            Execute("""
                delete from book_loans bl
                where bl.id = :loanId
                  and not exists (
                      select 1
                      from book_loan_items bli
                      where bli.loan_id = bl.id
                  )
                """, new NpgsqlParameter("loanId", loanId));
        }

        private List<int> LoadLoanBookIds(int loanId)
        {
            var table = FillTable("""
                select book_id
                from book_loan_items
                where loan_id = :loanId
                order by book_id
                """, new NpgsqlParameter("loanId", loanId));

            return table.AsEnumerable()
                .Select(row => Convert.ToInt32(row["book_id"], CultureInfo.InvariantCulture))
                .ToList();
        }

        private bool TryGetSelectedLoanItem(out int loanId, out int bookId)
        {
            loanId = 0;
            bookId = 0;
            return TryGetGridInt(_issuesGrid, "loan_id", out loanId)
                && TryGetGridInt(_issuesGrid, "book_id", out bookId);
        }

        private void RecalculateLoanPayment(int loanId)
        {
            Execute("""
                update book_loans bl
                set payment_amount = coalesce((
                    select sum(b.cost)
                    from book_loan_items bli
                    join books b on b.id = bli.book_id
                    where bli.loan_id = bl.id
                      and bli.lost = true
                ), 0)
                where bl.id = :loanId
                """, new NpgsqlParameter("loanId", loanId));
        }

        private void LoadUniversities()
        {
            var table = FillTable("select id, name from universities order by name");
            _universitiesGrid.DataSource = table;
            RenameColumns(_universitiesGrid, ("id", "Номер"), ("name", "Наименование ВУЗа"));
        }

        private void LoadBooks()
        {
            var table = FillTable("select id, title, author, cost from books order by title");
            _booksGrid.DataSource = table;
            RenameColumns(_booksGrid,
                ("id", "Номер"),
                ("title", "Название"),
                ("author", "Автор"),
                ("cost", "Стоимость"));
        }

        private void LoadStudents()
        {
            var table = FillTable("""
                select s.id, s.full_name, s.university_id, u.name as university_name, s.student_group, s.phone
                from students s
                join universities u on u.id = s.university_id
                order by u.name, s.full_name
                """);
            _studentsCatalogGrid.DataSource = table;
            RenameColumns(_studentsCatalogGrid,
                ("id", "Номер"),
                ("full_name", "ФИО"),
                ("university_name", "ВУЗ"),
                ("student_group", "Группа"),
                ("phone", "Телефон"));
            HideColumns(_studentsCatalogGrid, "university_id");
        }

        private void LoadLoans()
        {
            var table = FillTable("""
                select bl.id as loan_id,
                       bl.student_id,
                       s.full_name as student_name,
                       u.name as university_name,
                       bl.loan_date,
                       bl.due_date,
                       bl.payment_amount,
                       count(bli.book_id) as books_count
                from book_loans bl
                join students s on s.id = bl.student_id
                join universities u on u.id = s.university_id
                join book_loan_items bli on bli.loan_id = bl.id
                group by bl.id, bl.student_id, s.full_name, u.name, bl.loan_date, bl.due_date, bl.payment_amount
                order by bl.loan_date desc, bl.id desc
                """);

            _studentsGrid.DataSource = table;
            RenameColumns(_studentsGrid,
                ("loan_id", "Номер выдачи"),
                ("student_name", "Студент"),
                ("university_name", "ВУЗ"),
                ("loan_date", "Дата выдачи"),
                ("due_date", "Срок возврата"),
                ("payment_amount", "Оплата"),
                ("books_count", "Книг"));
            HideColumns(_studentsGrid, "student_id");
            LoadLoanItemsForSelectedLoan();
        }

        private void LoadLoanItemsForSelectedLoan()
        {
            if (!TryGetGridInt(_studentsGrid, "loan_id", out var loanId))
            {
                _issuesGrid.DataSource = null;
                return;
            }

            var table = FillTable("""
                select bli.loan_id,
                       bli.book_id,
                       b.title,
                       b.author,
                       b.cost,
                       bli.return_date,
                       bli.lost,
                       case
                           when bli.lost then 'Утеряна'
                           when bli.return_date is null then 'На руках'
                           when bli.return_date > bl.due_date then 'Возвращена поздно'
                           else 'Возвращена'
                       end as status
                from book_loan_items bli
                join book_loans bl on bl.id = bli.loan_id
                join books b on b.id = bli.book_id
                where bli.loan_id = :loanId
                order by b.title, b.author, b.id
                """, new NpgsqlParameter("loanId", loanId));

            _issuesGrid.DataSource = table;
            RenameColumns(_issuesGrid,
                ("book_id", "Номер книги"),
                ("title", "Книга"),
                ("author", "Автор"),
                ("cost", "Стоимость"),
                ("return_date", "Дата возврата"),
                ("status", "Статус"));
            HideColumns(_issuesGrid, "loan_id", "lost");
        }

        private void LoadReportUniversities()
        {
            var table = FillTable("select id, name from universities order by name");
            _reportUniversities.DataSource = table;
            _reportUniversities.DisplayMember = "name";
            _reportUniversities.ValueMember = "id";
            SetAllReportUniversities(true);
        }

        private static void RenameColumns(DataGridView grid, params (string Name, string Header)[] columns)
        {
            foreach (var column in columns)
            {
                if (grid.Columns.Contains(column.Name))
                {
                    grid.Columns[column.Name].HeaderText = column.Header;
                }
            }
        }

        private static void HideColumns(DataGridView grid, params string[] columns)
        {
            foreach (var column in columns)
            {
                if (grid.Columns.Contains(column))
                {
                    grid.Columns[column].Visible = false;
                }
            }
        }

        private static bool TryGetGridInt(DataGridView grid, string column, out int value)
        {
            value = 0;
            if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow || !grid.Columns.Contains(column))
            {
                return false;
            }

            var raw = grid.CurrentRow.Cells[column].Value;
            if (raw == null || raw == DBNull.Value)
            {
                return false;
            }

            value = Convert.ToInt32(raw, CultureInfo.InvariantCulture);
            return true;
        }

        private static string GridString(DataGridView grid, string column)
        {
            var raw = grid.CurrentRow?.Cells[column].Value;
            return raw == null || raw == DBNull.Value ? string.Empty : Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static decimal GridDecimal(DataGridView grid, string column)
        {
            var raw = grid.CurrentRow?.Cells[column].Value;
            return raw == null || raw == DBNull.Value ? 0m : Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
        }

        private static DateTime? GridDate(DataGridView grid, string column)
        {
            var raw = grid.CurrentRow?.Cells[column].Value;
            if (raw == null || raw == DBNull.Value)
            {
                return null;
            }

            if (raw is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }

            if (raw is DateTime dateTime)
            {
                return dateTime;
            }

            return Convert.ToDateTime(raw, CultureInfo.InvariantCulture);
        }

        private static DateTime DbDate(object? raw)
        {
            if (raw == null || raw == DBNull.Value)
            {
                return DateTime.Today;
            }

            if (raw is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }

            if (raw is DateTime dateTime)
            {
                return dateTime;
            }

            return Convert.ToDateTime(raw, CultureInfo.InvariantCulture);
        }

        private static bool Confirm(string message)
        {
            return MessageBox.Show(message, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void AddUniversity()
        {
            using var form = new UniversityEditForm();
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("insert into universities(name) values(:name)", new NpgsqlParameter("name", form.UniversityName));
            LoadUniversities();
            LoadStudents();
            LoadLoans();
            LoadReportUniversities();
        }

        private void EditUniversity()
        {
            if (!TryGetGridInt(_universitiesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите ВУЗ для изменения.");
                return;
            }

            using var form = new UniversityEditForm(GridString(_universitiesGrid, "name"));
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("update universities set name = :name where id = :id",
                new NpgsqlParameter("name", form.UniversityName),
                new NpgsqlParameter("id", id));
            LoadAll();
        }

        private void DeleteUniversity()
        {
            if (!TryGetGridInt(_universitiesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите ВУЗ для удаления.");
                return;
            }

            if (!Confirm("Удалить выбранный ВУЗ вместе со студентами и их выдачами?"))
            {
                return;
            }

            Execute("delete from universities where id = :id", new NpgsqlParameter("id", id));
            LoadAll();
        }

        private void AddBook()
        {
            using var form = new BookEditForm();
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("insert into books(title, author, cost) values(:title, :author, :cost)",
                new NpgsqlParameter("title", form.BookTitle),
                new NpgsqlParameter("author", form.Author),
                new NpgsqlParameter("cost", form.Cost));
            LoadBooks();
        }

        private void EditBook()
        {
            if (!TryGetGridInt(_booksGrid, "id", out var id))
            {
                MessageBox.Show("Выберите книгу для изменения.");
                return;
            }

            using var form = new BookEditForm(GridString(_booksGrid, "title"), GridString(_booksGrid, "author"), GridDecimal(_booksGrid, "cost"));
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("update books set title = :title, author = :author, cost = :cost where id = :id",
                new NpgsqlParameter("title", form.BookTitle),
                new NpgsqlParameter("author", form.Author),
                new NpgsqlParameter("cost", form.Cost),
                new NpgsqlParameter("id", id));
            LoadBooks();
            LoadLoans();
        }

        private void DeleteBook()
        {
            if (!TryGetGridInt(_booksGrid, "id", out var id))
            {
                MessageBox.Show("Выберите книгу для удаления.");
                return;
            }

            if (!Confirm("Удалить выбранную книгу вместе с записями выдачи?"))
            {
                return;
            }

            Execute("delete from books where id = :id", new NpgsqlParameter("id", id));
            LoadBooks();
            LoadLoans();
        }

        private void AddStudent()
        {
            using var form = new StudentEditForm(_connection);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("""
                insert into students(full_name, university_id, student_group, phone)
                values(:fullName, :universityId, :studentGroup, :phone)
                """,
                new NpgsqlParameter("fullName", form.FullName),
                new NpgsqlParameter("universityId", form.UniversityId),
                new NpgsqlParameter("studentGroup", form.StudentGroup),
                new NpgsqlParameter("phone", form.Phone));
            LoadStudents();
            LoadLoans();
        }

        private void EditStudent()
        {
            if (!TryGetGridInt(_studentsCatalogGrid, "id", out var id))
            {
                MessageBox.Show("Выберите студента для изменения.");
                return;
            }

            var universityId = Convert.ToInt32(_studentsCatalogGrid.CurrentRow!.Cells["university_id"].Value, CultureInfo.InvariantCulture);
            using var form = new StudentEditForm(_connection, GridString(_studentsCatalogGrid, "full_name"), universityId,
                GridString(_studentsCatalogGrid, "student_group"), GridString(_studentsCatalogGrid, "phone"));
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("""
                update students
                set full_name = :fullName, university_id = :universityId, student_group = :studentGroup, phone = :phone
                where id = :id
                """,
                new NpgsqlParameter("fullName", form.FullName),
                new NpgsqlParameter("universityId", form.UniversityId),
                new NpgsqlParameter("studentGroup", form.StudentGroup),
                new NpgsqlParameter("phone", form.Phone),
                new NpgsqlParameter("id", id));
            LoadStudents();
            LoadLoans();
        }

        private void DeleteStudent()
        {
            if (!TryGetGridInt(_studentsCatalogGrid, "id", out var id))
            {
                MessageBox.Show("Выберите студента для удаления.");
                return;
            }

            if (!Confirm("Удалить выбранного студента вместе с его выдачами?"))
            {
                return;
            }

            Execute("delete from students where id = :id", new NpgsqlParameter("id", id));
            LoadStudents();
            LoadLoans();
        }

        private void AddIssue()
        {
            using var form = new IssueEditForm(_connection);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var transaction = _connection.BeginTransaction();
            try
            {
                using var loanCommand = new NpgsqlCommand("""
                    insert into book_loans(student_id, loan_date, due_date, payment_amount)
                    values(:studentId, :loanDate, :dueDate, 0)
                    returning id
                    """, _connection, transaction);
                loanCommand.Parameters.AddWithValue("studentId", form.StudentId);
                loanCommand.Parameters.AddWithValue("loanDate", form.LoanDate);
                loanCommand.Parameters.AddWithValue("dueDate", form.DueDate);
                var loanId = Convert.ToInt32(loanCommand.ExecuteScalar(), CultureInfo.InvariantCulture);

                foreach (var bookId in form.SelectedBookIds)
                {
                    using var itemCommand = new NpgsqlCommand("""
                        insert into book_loan_items(loan_id, book_id, return_date, lost)
                        values(:loanId, :bookId, null, false)
                        """, _connection, transaction);
                    itemCommand.Parameters.AddWithValue("loanId", loanId);
                    itemCommand.Parameters.AddWithValue("bookId", bookId);
                    itemCommand.ExecuteNonQuery();
                }

                transaction.Commit();
                LoadLoans();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Ошибка при добавлении выдачи: " + ex.Message);
            }
        }

        private void EditIssue()
        {
            if (TryGetSelectedLoanItem(out var selectedLoanId, out var selectedBookId))
            {
                EditLoanItem(selectedLoanId, selectedBookId);
                return;
            }

            if (!TryGetGridInt(_studentsGrid, "loan_id", out var loanId))
            {
                MessageBox.Show("Выберите выдачу для изменения.");
                return;
            }

            var studentId = Convert.ToInt32(_studentsGrid.CurrentRow!.Cells["student_id"].Value, CultureInfo.InvariantCulture);
            var loanDate = GridDate(_studentsGrid, "loan_date") ?? DateTime.Today;
            var dueDate = GridDate(_studentsGrid, "due_date") ?? DateTime.Today;
            var selectedBooks = LoadLoanBookIds(loanId);

            using var form = new IssueEditForm(_connection, loanId, studentId, loanDate, dueDate, selectedBooks);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var transaction = _connection.BeginTransaction();
            try
            {
                using var updateLoan = new NpgsqlCommand("""
                    update book_loans
                    set student_id = :studentId,
                        loan_date = :loanDate,
                        due_date = :dueDate
                    where id = :loanId
                    """, _connection, transaction);
                updateLoan.Parameters.AddWithValue("studentId", form.StudentId);
                updateLoan.Parameters.AddWithValue("loanDate", form.LoanDate);
                updateLoan.Parameters.AddWithValue("dueDate", form.DueDate);
                updateLoan.Parameters.AddWithValue("loanId", loanId);
                updateLoan.ExecuteNonQuery();

                using var deleteRemoved = new NpgsqlCommand("""
                    delete from book_loan_items
                    where loan_id = :loanId
                      and lost = false
                      and return_date is null
                    """, _connection, transaction);
                deleteRemoved.Parameters.AddWithValue("loanId", loanId);
                deleteRemoved.ExecuteNonQuery();

                foreach (var bookId in form.SelectedBookIds)
                {
                    using var insertItem = new NpgsqlCommand("""
                        insert into book_loan_items(loan_id, book_id, return_date, lost)
                        values(:loanId, :bookId, null, false)
                        on conflict (loan_id, book_id) do nothing
                        """, _connection, transaction);
                    insertItem.Parameters.AddWithValue("loanId", loanId);
                    insertItem.Parameters.AddWithValue("bookId", bookId);
                    insertItem.ExecuteNonQuery();
                }

                transaction.Commit();
                RecalculateLoanPayment(loanId);
                LoadLoans();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Ошибка при изменении выдачи: " + ex.Message);
            }
        }

        private void EditLoanItem(int loanId, int bookId)
        {
            var table = FillTable("""
                select bl.loan_date, b.title, b.author, bli.return_date, bli.lost
                from book_loan_items bli
                join book_loans bl on bl.id = bli.loan_id
                join books b on b.id = bli.book_id
                where bli.loan_id = :loanId and bli.book_id = :bookId
                """,
                new NpgsqlParameter("loanId", loanId),
                new NpgsqlParameter("bookId", bookId));

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Выбранная книга в выдаче не найдена.");
                return;
            }

            var row = table.Rows[0];
            var loanDate = DbDate(row["loan_date"]);
            var title = Convert.ToString(row["title"], CultureInfo.InvariantCulture) ?? string.Empty;
            var author = Convert.ToString(row["author"], CultureInfo.InvariantCulture) ?? string.Empty;
            DateTime? returnDate = row["return_date"] == DBNull.Value ? null : DbDate(row["return_date"]);
            var lost = Convert.ToBoolean(row["lost"], CultureInfo.InvariantCulture);

            using var form = new LoanItemStateForm($"{title} - {author}", loanDate, returnDate, lost);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Execute("""
                    update book_loan_items
                    set return_date = :returnDate,
                        lost = :lost
                    where loan_id = :loanId and book_id = :bookId
                    """,
                    new NpgsqlParameter("returnDate", (object?)form.ReturnDate ?? DBNull.Value),
                    new NpgsqlParameter("lost", form.Lost),
                    new NpgsqlParameter("loanId", loanId),
                    new NpgsqlParameter("bookId", bookId));
                RecalculateLoanPayment(loanId);
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при изменении состояния книги: " + ex.Message);
            }
        }

        private void DeleteIssue()
        {
            if (!TryGetGridInt(_studentsGrid, "loan_id", out var loanId))
            {
                MessageBox.Show("Выберите выдачу для удаления.");
                return;
            }

            if (!Confirm("Удалить выбранную выдачу вместе со всеми книгами?"))
            {
                return;
            }

            Execute("delete from book_loans where id = :loanId", new NpgsqlParameter("loanId", loanId));
            LoadLoans();
        }

        private void MarkReturned()
        {
            if (!TryGetSelectedLoanItem(out var loanId, out var bookId))
            {
                MessageBox.Show("Выберите книгу в составе выдачи.");
                return;
            }

            try
            {
                var loanDate = DbDate(Scalar(
                    "select loan_date from book_loans where id = :loanId",
                    new NpgsqlParameter("loanId", loanId)));
                if (DateTime.Today < loanDate)
                {
                    MessageBox.Show("Дата возврата не может быть раньше даты выдачи.");
                    return;
                }

                Execute("""
                    update book_loan_items
                    set return_date = :returnDate, lost = false
                    where loan_id = :loanId and book_id = :bookId
                    """,
                    new NpgsqlParameter("returnDate", DateTime.Today),
                    new NpgsqlParameter("loanId", loanId),
                    new NpgsqlParameter("bookId", bookId));
                RecalculateLoanPayment(loanId);
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при возврате книги: " + ex.Message);
            }
        }

        private void MarkLost()
        {
            if (!TryGetSelectedLoanItem(out var loanId, out var bookId))
            {
                MessageBox.Show("Выберите книгу в составе выдачи.");
                return;
            }

            try
            {
                Execute("""
                    update book_loan_items
                    set return_date = null, lost = true
                    where loan_id = :loanId and book_id = :bookId
                    """,
                    new NpgsqlParameter("loanId", loanId),
                    new NpgsqlParameter("bookId", bookId));
                RecalculateLoanPayment(loanId);
                LoadLoans();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отметке утери книги: " + ex.Message);
            }
        }

        private void SetAllReportUniversities(bool selected)
        {
            for (var i = 0; i < _reportUniversities.Items.Count; i++)
            {
                _reportUniversities.SetItemChecked(i, selected);
            }
        }

        private List<int> SelectedUniversityIds()
        {
            var result = new List<int>();
            foreach (var item in _reportUniversities.CheckedItems)
            {
                if (item is DataRowView row)
                {
                    result.Add(Convert.ToInt32(row["id"], CultureInfo.InvariantCulture));
                }
            }

            return result;
        }

        private void BuildReport()
        {
            var ids = SelectedUniversityIds();
            if (ids.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один ВУЗ.");
                return;
            }

            _reportTable = FillTable("""
                select u.name as "ВУЗ",
                       s.full_name as "Студент",
                       s.student_group as "Группа",
                       s.phone as "Телефон",
                       bl.id as "Номер выдачи",
                       bl.loan_date as "Дата выдачи",
                       bl.due_date as "Срок возврата",
                       b.id as "Номер книги",
                       b.title as "Книга",
                       b.author as "Автор",
                       b.cost as "Стоимость",
                       'Не возвращена' as "Статус"
                from book_loans bl
                join students s on s.id = bl.student_id
                join universities u on u.id = s.university_id
                join book_loan_items bli on bli.loan_id = bl.id
                join books b on b.id = bli.book_id
                where u.id = any(:universityIds)
                  and bl.due_date < :reportDate
                  and bli.return_date is null
                  and bli.lost = false
                order by u.name, s.full_name, bl.due_date, b.title
                """,
                new NpgsqlParameter("universityIds", ids.ToArray()),
                new NpgsqlParameter("reportDate", _reportDate.Value.Date));

            _reportGrid.DataSource = _reportTable;
            _chartData = _reportTable.AsEnumerable()
                .GroupBy(row => Convert.ToString(row["Студент"], CultureInfo.InvariantCulture) ?? string.Empty)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .ToDictionary(group => group.Key, group => group.Count());
            _chartPanel.Invalidate();
        }

        private void DrawChart(object? sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.Clear(Color.White);
            using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using var textFont = new Font("Segoe UI", 9);
            using var brush = new SolidBrush(Color.FromArgb(47, 111, 179));
            using var textBrush = new SolidBrush(Color.Black);
            using var axisPen = new Pen(Color.DimGray);

            graphics.DrawString("Количество не сданных книг по студентам", titleFont, textBrush, 16, 12);

            if (_chartData.Count == 0)
            {
                graphics.DrawString("Нет данных для диаграммы.", textFont, textBrush, 16, 45);
                return;
            }

            var left = 60;
            var top = 50;
            var bottom = _chartPanel.Height - 55;
            var right = _chartPanel.Width - 25;
            var height = Math.Max(1, bottom - top);
            var max = Math.Max(1, _chartData.Values.Max());
            var barWidth = Math.Max(30, (right - left) / _chartData.Count - 20);
            var x = left + 10;

            graphics.DrawLine(axisPen, left, top, left, bottom);
            graphics.DrawLine(axisPen, left, bottom, right, bottom);

            foreach (var item in _chartData)
            {
                var barHeight = (int)Math.Round(item.Value / (double)max * (height - 25));
                var y = bottom - barHeight;
                graphics.FillRectangle(brush, x, y, barWidth, barHeight);
                graphics.DrawString(item.Value.ToString(CultureInfo.InvariantCulture), textFont, textBrush, x + barWidth / 2 - 6, y - 20);

                var label = item.Key.Length > 24 ? item.Key[..24] + "..." : item.Key;
                graphics.TranslateTransform(x, bottom + 5);
                graphics.RotateTransform(-20);
                graphics.DrawString(label, textFont, textBrush, 0, 0);
                graphics.ResetTransform();
                x += barWidth + 20;
            }
        }

        private void ExportReport()
        {
            if (_reportTable.Rows.Count == 0)
            {
                MessageBox.Show("Сначала постройте отчет.");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Excel (*.xls)|*.xls",
                FileName = "Отчет_по_несданным_книгам.xls"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var html = new StringBuilder();
            html.AppendLine("<html><head><meta charset=\"utf-8\"></head><body>");
            html.AppendLine("<h2>Список студентов, не вернувших книги в срок</h2>");
            html.AppendLine("<p>Дата отчета: " + WebUtility.HtmlEncode(_reportDate.Value.ToShortDateString()) + "</p>");
            AppendHtmlTable(html, _reportTable);

            var summary = new DataTable();
            summary.Columns.Add("Студент");
            summary.Columns.Add("Количество не сданных книг");
            foreach (var item in _chartData)
            {
                summary.Rows.Add(item.Key, item.Value);
            }

            html.AppendLine("<h3>Итоги по студентам</h3>");
            AppendHtmlTable(html, summary);
            html.AppendLine("</body></html>");

            File.WriteAllText(dialog.FileName, html.ToString(), Encoding.UTF8);
            MessageBox.Show("Отчет экспортирован в файл Excel.");
        }

        private static void AppendHtmlTable(StringBuilder html, DataTable table)
        {
            html.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"4\">");
            html.AppendLine("<tr>");
            foreach (DataColumn column in table.Columns)
            {
                html.Append("<th>").Append(WebUtility.HtmlEncode(column.ColumnName)).AppendLine("</th>");
            }
            html.AppendLine("</tr>");

            foreach (DataRow row in table.Rows)
            {
                html.AppendLine("<tr>");
                foreach (var value in row.ItemArray)
                {
                    html.Append("<td>").Append(WebUtility.HtmlEncode(FormatCell(value))).AppendLine("</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
        }

        private static string FormatCell(object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value is DateTime date
                ? date.ToShortDateString()
                : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    internal sealed class UniversityEditForm : Form
    {
        private readonly TextBox _name = new() { Width = 520 };

        public string UniversityName => _name.Text.Trim();

        public UniversityEditForm(string name = "")
        {
            Text = string.IsNullOrWhiteSpace(name) ? "Добавить ВУЗ" : "Изменить ВУЗ";
            _name.Text = name;
            Build("Наименование ВУЗа", _name);
        }

        private void Build(string label, Control editor)
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(560, 155);

            Controls.Add(new Label { Text = label, Location = new Point(16, 15), AutoSize = true });
            editor.Location = new Point(16, 42);
            Controls.Add(editor);
            AddDialogButtons(105);
        }

        private void AddDialogButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(335, y), Width = 105, Height = 30 };
            var cancel = new Button { Text = "Отмена", Location = new Point(445, y), Width = 105, Height = 30 };
            ok.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(UniversityName))
                {
                    MessageBox.Show("Введите наименование ВУЗа.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);
        }
    }

    internal sealed class BookEditForm : Form
    {
        private readonly TextBox _title = new() { Width = 380 };
        private readonly TextBox _author = new() { Width = 380 };
        private readonly NumericUpDown _cost = new() { Width = 120, DecimalPlaces = 2, Maximum = 1_000_000, Minimum = 0 };

        public string BookTitle => _title.Text.Trim();
        public string Author => _author.Text.Trim();
        public decimal Cost => _cost.Value;

        public BookEditForm(string title = "", string author = "", decimal cost = 0)
        {
            Text = string.IsNullOrWhiteSpace(title) ? "Добавить книгу" : "Изменить книгу";
            _title.Text = title;
            _author.Text = author;
            _cost.Value = Math.Min(_cost.Maximum, Math.Max(_cost.Minimum, cost));

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(540, 200);

            AddRow("Название", _title, 15);
            AddRow("Автор", _author, 50);
            AddRow("Стоимость", _cost, 85);
            AddButtons(145);
        }

        private void AddRow(string label, Control editor, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(14, y + 4), AutoSize = true });
            editor.Location = new Point(120, y);
            Controls.Add(editor);
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(315, y), Width = 105, Height = 30 };
            var cancel = new Button { Text = "Отмена", Location = new Point(425, y), Width = 105, Height = 30 };
            ok.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(BookTitle))
                {
                    MessageBox.Show("Введите название книги.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);
        }
    }

    internal sealed class StudentEditForm : Form
    {
        private readonly TextBox _fullName = new() { Width = 400 };
        private readonly ComboBox _university = new() { Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _group = new() { Width = 160 };
        private readonly TextBox _phone = new() { Width = 160 };

        public string FullName => _fullName.Text.Trim();
        public int UniversityId => Convert.ToInt32(_university.SelectedValue, CultureInfo.InvariantCulture);
        public string StudentGroup => _group.Text.Trim();
        public string Phone => _phone.Text.Trim();

        public StudentEditForm(NpgsqlConnection connection, string fullName = "", int? universityId = null, string group = "", string phone = "")
        {
            Text = string.IsNullOrWhiteSpace(fullName) ? "Добавить студента" : "Изменить студента";
            Build();
            LoadUniversities(connection);
            _fullName.Text = fullName;
            _group.Text = group;
            _phone.Text = phone;
            if (universityId.HasValue)
            {
                _university.SelectedValue = universityId.Value;
            }
        }

        private void Build()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(580, 240);
            AddRow("ФИО", _fullName, 15);
            AddRow("ВУЗ", _university, 50);
            AddRow("Группа", _group, 85);
            AddRow("Телефон", _phone, 120);
            AddButtons(185);
        }

        private void LoadUniversities(NpgsqlConnection connection)
        {
            using var adapter = new NpgsqlDataAdapter("select id, name from universities order by name", connection);
            var table = new DataTable();
            adapter.Fill(table);
            _university.DataSource = table;
            _university.DisplayMember = "name";
            _university.ValueMember = "id";
        }

        private void AddRow(string label, Control editor, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(14, y + 4), AutoSize = true });
            editor.Location = new Point(150, y);
            Controls.Add(editor);
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(355, y), Width = 105, Height = 30 };
            var cancel = new Button { Text = "Отмена", Location = new Point(465, y), Width = 105, Height = 30 };
            ok.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    MessageBox.Show("Введите ФИО студента.");
                    return;
                }

                if (_university.SelectedValue == null)
                {
                    MessageBox.Show("Выберите ВУЗ.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);
        }
    }

    internal sealed class LoanItemStateForm : Form
    {
        private readonly DateTime _loanDate;
        private readonly Label _bookLabel = new() { AutoSize = true };
        private readonly CheckBox _hasReturnDate = new() { Text = "Дата возврата", Width = 160 };
        private readonly DateTimePicker _returnDate = new() { Width = 160, Format = DateTimePickerFormat.Short };
        private readonly CheckBox _lost = new() { Text = "Книга утеряна", Width = 160 };
        private bool _updatingState;

        public DateTime? ReturnDate => _hasReturnDate.Checked ? _returnDate.Value.Date : null;
        public bool Lost => _lost.Checked;

        public LoanItemStateForm(string bookName, DateTime loanDate, DateTime? returnDate, bool lost)
        {
            _loanDate = loanDate.Date;
            Text = "Изменить состояние книги";

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(520, 190);

            _bookLabel.Text = bookName;
            _bookLabel.Location = new Point(16, 16);
            _bookLabel.MaximumSize = new Size(480, 0);
            Controls.Add(_bookLabel);

            _hasReturnDate.Location = new Point(16, 65);
            _hasReturnDate.Checked = returnDate.HasValue;
            _hasReturnDate.CheckedChanged += (_, _) => UpdateReturnState();
            Controls.Add(_hasReturnDate);

            _returnDate.Location = new Point(190, 62);
            _returnDate.Value = returnDate ?? DateTime.Today;
            Controls.Add(_returnDate);

            _lost.Location = new Point(16, 100);
            _lost.Checked = lost;
            _lost.CheckedChanged += (_, _) => UpdateLostState();
            Controls.Add(_lost);

            var ok = new Button { Text = "Сохранить", Location = new Point(300, 145), Width = 105, Height = 30 };
            var cancel = new Button { Text = "Отмена", Location = new Point(410, 145), Width = 95, Height = 30 };

            ok.Click += (_, _) =>
            {
                if (ReturnDate.HasValue && ReturnDate.Value < _loanDate)
                {
                    MessageBox.Show("Дата возврата не может быть раньше даты выдачи.");
                    return;
                }

                if (Lost && ReturnDate.HasValue)
                {
                    MessageBox.Show("Книга не может быть одновременно возвращена и утеряна.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);

            ApplyState();
        }

        private void UpdateReturnState()
        {
            if (_updatingState)
            {
                return;
            }

            _updatingState = true;
            if (_hasReturnDate.Checked)
            {
                _lost.Checked = false;
            }
            _returnDate.Enabled = _hasReturnDate.Checked;
            _updatingState = false;
        }

        private void UpdateLostState()
        {
            if (_updatingState)
            {
                return;
            }

            _updatingState = true;
            if (_lost.Checked)
            {
                _hasReturnDate.Checked = false;
            }
            _returnDate.Enabled = _hasReturnDate.Checked;
            _updatingState = false;
        }

        private void ApplyState()
        {
            _updatingState = true;
            if (_lost.Checked)
            {
                _hasReturnDate.Checked = false;
            }
            _returnDate.Enabled = _hasReturnDate.Checked;
            _updatingState = false;
        }
    }

    internal sealed class IssueEditForm : Form
    {
        private readonly NpgsqlConnection _connection;
        private readonly int? _currentLoanId;
        private readonly ComboBox _student = new() { Width = 470, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _loanDate = new() { Width = 160, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dueDate = new() { Width = 160, Format = DateTimePickerFormat.Short };
        private readonly CheckedListBox _books = new() { Width = 470, Height = 220, CheckOnClick = true };
        private readonly HashSet<int> _initialBookIds;

        public int StudentId => Convert.ToInt32(_student.SelectedValue, CultureInfo.InvariantCulture);
        public DateTime LoanDate => _loanDate.Value.Date;
        public DateTime DueDate => _dueDate.Value.Date;
        public List<int> SelectedBookIds
        {
            get
            {
                var result = new List<int>();
                foreach (var item in _books.CheckedItems)
                {
                    if (item is DataRowView row)
                    {
                        result.Add(Convert.ToInt32(row["id"], CultureInfo.InvariantCulture));
                    }
                }

                return result;
            }
        }

        public IssueEditForm(NpgsqlConnection connection)
            : this(connection, null, null, DateTime.Today, DateTime.Today.AddDays(14), [])
        {
            Text = "Добавить выдачу";
        }

        public IssueEditForm(NpgsqlConnection connection, int currentLoanId, int studentId, DateTime loanDate, DateTime dueDate, List<int> selectedBookIds)
            : this(connection, (int?)currentLoanId, (int?)studentId, loanDate, dueDate, selectedBookIds)
        {
            Text = "Изменить выдачу";
        }

        private IssueEditForm(NpgsqlConnection connection, int? currentLoanId, int? studentId, DateTime loanDate, DateTime dueDate, List<int> selectedBookIds)
        {
            _connection = connection;
            _currentLoanId = currentLoanId;
            _initialBookIds = selectedBookIds.ToHashSet();
            Build();
            LoadStudents();
            LoadBooks();
            _loanDate.Value = loanDate;
            _dueDate.Value = dueDate;
            if (studentId.HasValue)
            {
                _student.SelectedValue = studentId.Value;
            }
        }

        private void Build()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(660, 430);

            AddRow("Студент", _student, 15);
            AddRow("Дата выдачи", _loanDate, 50);
            AddRow("Срок возврата", _dueDate, 85);
            Controls.Add(new Label { Text = "Книги", Location = new Point(16, 124), AutoSize = true });
            _books.Location = new Point(165, 120);
            Controls.Add(_books);
            AddButtons(375);
        }

        private void LoadStudents()
        {
            using var adapter = new NpgsqlDataAdapter("""
                select s.id, s.full_name || ' (' || u.name || ')' as display_name
                from students s
                join universities u on u.id = s.university_id
                order by s.full_name
                """, _connection);
            var table = new DataTable();
            adapter.Fill(table);
            _student.DataSource = table;
            _student.DisplayMember = "display_name";
            _student.ValueMember = "id";
        }

        private void LoadBooks()
        {
            using var command = new NpgsqlCommand("""
                select b.id, b.title || ' - ' || b.author || ' (экз. ' || b.id || ')' as display_name
                from books b
                where not exists (
                    select 1
                    from book_loan_items bli
                    where bli.book_id = b.id
                      and bli.return_date is null
                      and (:currentLoanId is null or bli.loan_id <> :currentLoanId)
                )
                order by b.title, b.author, b.id
                """, _connection);
            command.Parameters.Add("currentLoanId", NpgsqlDbType.Integer).Value = (object?)_currentLoanId ?? DBNull.Value;
            using var adapter = new NpgsqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);

            _books.DataSource = table;
            _books.DisplayMember = "display_name";
            _books.ValueMember = "id";

            for (var i = 0; i < _books.Items.Count; i++)
            {
                if (_books.Items[i] is DataRowView row)
                {
                    var bookId = Convert.ToInt32(row["id"], CultureInfo.InvariantCulture);
                    _books.SetItemChecked(i, _initialBookIds.Contains(bookId));
                }
            }
        }

        private void AddRow(string label, Control editor, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(16, y + 4), AutoSize = true });
            editor.Location = new Point(165, y);
            Controls.Add(editor);
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(435, y), Width = 105, Height = 30 };
            var cancel = new Button { Text = "Отмена", Location = new Point(545, y), Width = 105, Height = 30 };
            ok.Click += (_, _) =>
            {
                if (_student.SelectedValue == null)
                {
                    MessageBox.Show("Выберите студента.");
                    return;
                }

                if (DueDate < LoanDate)
                {
                    MessageBox.Show("Срок возврата не может быть раньше даты выдачи.");
                    return;
                }

                if (SelectedBookIds.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы одну книгу.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);
        }
    }
}
