using Npgsql;
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

        private void StudentsGrid_SelectionChanged(object? sender, EventArgs e) => LoadIssuesForSelectedStudent();
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

            _studentsGrid.SelectionChanged += (_, _) => LoadIssuesForSelectedStudent();

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
            var build = CreateButton("Построить отчет", 610, 44);
            build.Click += (_, _) => BuildReport();
            var export = CreateButton("Экспорт в Excel", 610, 82);
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
                Height = 46,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight
            };

            foreach (var buttonInfo in buttons)
            {
                var button = new Button
                {
                    Text = buttonInfo.Text,
                    Width = 150,
                    Height = 28
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
                Width = 125,
                Height = 30
            };
        }

        private void LoadAll()
        {
            LoadUniversities();
            LoadBooks();
            LoadStudents();
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
            _studentsGrid.DataSource = table;
            RenameColumns(_studentsGrid,
                ("id", "Номер"),
                ("full_name", "ФИО"),
                ("university_name", "ВУЗ"),
                ("student_group", "Группа"),
                ("phone", "Телефон"));
            HideColumns(_studentsGrid, "university_id");
            LoadIssuesForSelectedStudent();
        }

        private void LoadIssuesForSelectedStudent()
        {
            if (!TryGetGridInt(_studentsGrid, "id", out var studentId))
            {
                _issuesGrid.DataSource = null;
                return;
            }

            var table = FillTable("""
                select bi.id,
                       bi.student_id,
                       bi.book_id,
                       b.title as book_title,
                       b.author,
                       bi.issue_date,
                       bi.due_date,
                       bi.return_date,
                       bi.lost,
                       bi.payment_amount,
                       case
                           when bi.lost then 'Утеряна'
                           when bi.return_date is null then 'На руках'
                           when bi.return_date > bi.due_date then 'Возвращена поздно'
                           else 'Возвращена'
                       end as status
                from book_issues bi
                join books b on b.id = bi.book_id
                where bi.student_id = :studentId
                order by bi.issue_date desc, bi.id desc
                """, new NpgsqlParameter("studentId", studentId));

            _issuesGrid.DataSource = table;
            RenameColumns(_issuesGrid,
                ("id", "Номер"),
                ("book_title", "Книга"),
                ("author", "Автор"),
                ("issue_date", "Дата выдачи"),
                ("due_date", "Срок возврата"),
                ("return_date", "Дата возврата"),
                ("status", "Статус"),
                ("payment_amount", "Оплата"));
            HideColumns(_issuesGrid, "student_id", "book_id", "lost");
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
            LoadIssuesForSelectedStudent();
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
            LoadIssuesForSelectedStudent();
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
        }

        private void EditStudent()
        {
            if (!TryGetGridInt(_studentsGrid, "id", out var id))
            {
                MessageBox.Show("Выберите студента для изменения.");
                return;
            }

            var universityId = Convert.ToInt32(_studentsGrid.CurrentRow!.Cells["university_id"].Value, CultureInfo.InvariantCulture);
            using var form = new StudentEditForm(_connection, GridString(_studentsGrid, "full_name"), universityId,
                GridString(_studentsGrid, "student_group"), GridString(_studentsGrid, "phone"));
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
        }

        private void DeleteStudent()
        {
            if (!TryGetGridInt(_studentsGrid, "id", out var id))
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
        }

        private void AddIssue()
        {
            var selectedStudentId = TryGetGridInt(_studentsGrid, "id", out var sid) ? sid : (int?)null;
            using var form = new IssueEditForm(_connection, selectedStudentId);
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("""
                insert into book_issues(student_id, book_id, issue_date, due_date, return_date, lost, payment_amount)
                values(:studentId, :bookId, :issueDate, :dueDate, :returnDate, :lost, :payment)
                """,
                new NpgsqlParameter("studentId", form.StudentId),
                new NpgsqlParameter("bookId", form.BookId),
                new NpgsqlParameter("issueDate", form.IssueDate),
                new NpgsqlParameter("dueDate", form.DueDate),
                new NpgsqlParameter("returnDate", (object?)form.ReturnDate ?? DBNull.Value),
                new NpgsqlParameter("lost", form.Lost),
                new NpgsqlParameter("payment", form.PaymentAmount));
            LoadStudents();
        }

        private void EditIssue()
        {
            if (!TryGetGridInt(_issuesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите запись выдачи для изменения.");
                return;
            }

            var studentId = Convert.ToInt32(_issuesGrid.CurrentRow!.Cells["student_id"].Value, CultureInfo.InvariantCulture);
            var bookId = Convert.ToInt32(_issuesGrid.CurrentRow.Cells["book_id"].Value, CultureInfo.InvariantCulture);
            var returnDate = GridDate(_issuesGrid, "return_date");
            var lost = Convert.ToBoolean(_issuesGrid.CurrentRow.Cells["lost"].Value, CultureInfo.InvariantCulture);

            using var form = new IssueEditForm(_connection, studentId, bookId, GridDate(_issuesGrid, "issue_date") ?? DateTime.Today,
                GridDate(_issuesGrid, "due_date") ?? DateTime.Today, returnDate, lost, GridDecimal(_issuesGrid, "payment_amount"));
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Execute("""
                update book_issues
                set student_id = :studentId,
                    book_id = :bookId,
                    issue_date = :issueDate,
                    due_date = :dueDate,
                    return_date = :returnDate,
                    lost = :lost,
                    payment_amount = :payment
                where id = :id
                """,
                new NpgsqlParameter("studentId", form.StudentId),
                new NpgsqlParameter("bookId", form.BookId),
                new NpgsqlParameter("issueDate", form.IssueDate),
                new NpgsqlParameter("dueDate", form.DueDate),
                new NpgsqlParameter("returnDate", (object?)form.ReturnDate ?? DBNull.Value),
                new NpgsqlParameter("lost", form.Lost),
                new NpgsqlParameter("payment", form.PaymentAmount),
                new NpgsqlParameter("id", id));
            LoadStudents();
        }

        private void DeleteIssue()
        {
            if (!TryGetGridInt(_issuesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите запись выдачи для удаления.");
                return;
            }

            if (!Confirm("Удалить выбранную запись выдачи?"))
            {
                return;
            }

            Execute("delete from book_issues where id = :id", new NpgsqlParameter("id", id));
            LoadIssuesForSelectedStudent();
        }

        private void MarkReturned()
        {
            if (!TryGetGridInt(_issuesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите запись выдачи.");
                return;
            }

            Execute("""
                update book_issues
                set return_date = :returnDate, lost = false, payment_amount = 0
                where id = :id
                """,
                new NpgsqlParameter("returnDate", DateTime.Today),
                new NpgsqlParameter("id", id));
            LoadIssuesForSelectedStudent();
        }

        private void MarkLost()
        {
            if (!TryGetGridInt(_issuesGrid, "id", out var id))
            {
                MessageBox.Show("Выберите запись выдачи.");
                return;
            }

            var cost = Scalar("""
                select b.cost
                from book_issues bi
                join books b on b.id = bi.book_id
                where bi.id = :id
                """, new NpgsqlParameter("id", id));

            Execute("""
                update book_issues
                set return_date = :returnDate, lost = true, payment_amount = :payment
                where id = :id
                """,
                new NpgsqlParameter("returnDate", DateTime.Today),
                new NpgsqlParameter("payment", Convert.ToDecimal(cost, CultureInfo.InvariantCulture)),
                new NpgsqlParameter("id", id));
            LoadIssuesForSelectedStudent();
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
                select s.full_name as "Студент",
                       u.name as "ВУЗ",
                       s.student_group as "Группа",
                       b.title as "Книга",
                       b.author as "Автор",
                       b.cost as "Стоимость",
                       bi.issue_date as "Дата выдачи",
                       bi.due_date as "Срок возврата",
                       bi.return_date as "Дата возврата",
                       case
                           when bi.lost then 'Утеряна'
                           when bi.return_date is null then 'Не возвращена'
                           when bi.return_date > bi.due_date then 'Возвращена поздно'
                           else 'Возвращена'
                       end as "Статус",
                       bi.payment_amount as "Оплата"
                from book_issues bi
                join students s on s.id = bi.student_id
                join universities u on u.id = s.university_id
                join books b on b.id = bi.book_id
                where u.id = any(:universityIds)
                  and bi.issue_date <= :reportDate
                  and bi.due_date < :reportDate
                  and (bi.lost = true or bi.return_date is null or bi.return_date > bi.due_date)
                order by u.name, s.full_name, b.title
                """,
                new NpgsqlParameter("universityIds", ids.ToArray()),
                new NpgsqlParameter("reportDate", _reportDate.Value.Date));

            _reportGrid.DataSource = _reportTable;
            _chartData = _reportTable.AsEnumerable()
                .GroupBy(row => Convert.ToString(row["ВУЗ"], CultureInfo.InvariantCulture) ?? string.Empty)
                .OrderBy(group => group.Key)
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

            graphics.DrawString("Соотношение не сданных книг по ВУЗам", titleFont, textBrush, 16, 12);

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
            summary.Columns.Add("ВУЗ");
            summary.Columns.Add("Количество не сданных книг");
            foreach (var item in _chartData)
            {
                summary.Rows.Add(item.Key, item.Value);
            }

            html.AppendLine("<h3>Итоги по ВУЗам</h3>");
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
        private readonly TextBox _name = new() { Width = 320 };

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
            ClientSize = new Size(470, 115);

            Controls.Add(new Label { Text = label, Location = new Point(12, 18), AutoSize = true });
            editor.Location = new Point(130, 15);
            Controls.Add(editor);
            AddDialogButtons(55);
        }

        private void AddDialogButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(260, y), Width = 95 };
            var cancel = new Button { Text = "Отмена", Location = new Point(365, y), Width = 95 };
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
        private readonly TextBox _title = new() { Width = 300 };
        private readonly TextBox _author = new() { Width = 300 };
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
            ClientSize = new Size(450, 180);

            AddRow("Название", _title, 15);
            AddRow("Автор", _author, 50);
            AddRow("Стоимость", _cost, 85);
            AddButtons(125);
        }

        private void AddRow(string label, Control editor, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(14, y + 4), AutoSize = true });
            editor.Location = new Point(120, y);
            Controls.Add(editor);
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(235, y), Width = 95 };
            var cancel = new Button { Text = "Отмена", Location = new Point(340, y), Width = 95 };
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
        private readonly TextBox _fullName = new() { Width = 310 };
        private readonly ComboBox _university = new() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList };
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
            ClientSize = new Size(480, 220);
            AddRow("ФИО", _fullName, 15);
            AddRow("ВУЗ", _university, 50);
            AddRow("Группа", _group, 85);
            AddRow("Телефон", _phone, 120);
            AddButtons(165);
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
            editor.Location = new Point(130, y);
            Controls.Add(editor);
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(265, y), Width = 95 };
            var cancel = new Button { Text = "Отмена", Location = new Point(370, y), Width = 95 };
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

    internal sealed class IssueEditForm : Form
    {
        private readonly NpgsqlConnection _connection;
        private readonly ComboBox _student = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _book = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _issueDate = new() { Width = 150, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _dueDate = new() { Width = 150, Format = DateTimePickerFormat.Short };
        private readonly CheckBox _hasReturnDate = new() { Text = "Дата возврата указана", Width = 170 };
        private readonly DateTimePicker _returnDate = new() { Width = 150, Format = DateTimePickerFormat.Short };
        private readonly CheckBox _lost = new() { Text = "Книга утеряна", Width = 150 };
        private readonly NumericUpDown _payment = new() { Width = 130, DecimalPlaces = 2, Maximum = 1_000_000, Minimum = 0 };

        public int StudentId => Convert.ToInt32(_student.SelectedValue, CultureInfo.InvariantCulture);
        public int BookId => Convert.ToInt32(_book.SelectedValue, CultureInfo.InvariantCulture);
        public DateTime IssueDate => _issueDate.Value.Date;
        public DateTime DueDate => _dueDate.Value.Date;
        public DateTime? ReturnDate => _hasReturnDate.Checked ? _returnDate.Value.Date : null;
        public bool Lost => _lost.Checked;
        public decimal PaymentAmount => _payment.Value;

        public IssueEditForm(NpgsqlConnection connection, int? selectedStudentId)
        {
            _connection = connection;
            Text = "Добавить выдачу";
            Build();
            LoadLists();
            _issueDate.Value = DateTime.Today;
            _dueDate.Value = DateTime.Today.AddDays(14);
            if (selectedStudentId.HasValue)
            {
                _student.SelectedValue = selectedStudentId.Value;
            }
            UpdateReturnControls();
        }

        public IssueEditForm(NpgsqlConnection connection, int studentId, int bookId, DateTime issueDate, DateTime dueDate,
            DateTime? returnDate, bool lost, decimal payment)
        {
            _connection = connection;
            Text = "Изменить выдачу";
            Build();
            LoadLists();
            _student.SelectedValue = studentId;
            _book.SelectedValue = bookId;
            _issueDate.Value = issueDate;
            _dueDate.Value = dueDate;
            _hasReturnDate.Checked = returnDate.HasValue;
            _returnDate.Value = returnDate ?? DateTime.Today;
            _lost.Checked = lost;
            _payment.Value = Math.Min(_payment.Maximum, Math.Max(_payment.Minimum, payment));
            UpdateReturnControls();
        }

        private void Build()
        {
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(530, 300);

            AddRow("Студент", _student, 15);
            AddRow("Книга", _book, 50);
            AddRow("Дата выдачи", _issueDate, 85);
            AddRow("Срок возврата", _dueDate, 120);
            _hasReturnDate.Location = new Point(150, 155);
            _hasReturnDate.CheckedChanged += (_, _) => UpdateReturnControls();
            Controls.Add(_hasReturnDate);
            _returnDate.Location = new Point(330, 153);
            Controls.Add(_returnDate);
            _lost.Location = new Point(150, 188);
            _lost.CheckedChanged += (_, _) => UpdateLostPayment();
            Controls.Add(_lost);
            AddRow("Оплата", _payment, 220);
            AddButtons(255);
        }

        private void LoadLists()
        {
            _student.DataSource = LoadTable("""
                select s.id, s.full_name || ' (' || u.name || ')' as display_name
                from students s
                join universities u on u.id = s.university_id
                order by s.full_name
                """);
            _student.DisplayMember = "display_name";
            _student.ValueMember = "id";

            _book.DataSource = LoadTable("select id, title || ' - ' || author as display_name from books order by title");
            _book.DisplayMember = "display_name";
            _book.ValueMember = "id";
        }

        private DataTable LoadTable(string sql)
        {
            using var adapter = new NpgsqlDataAdapter(sql, _connection);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        private void AddRow(string label, Control editor, int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(16, y + 4), AutoSize = true });
            editor.Location = new Point(150, y);
            Controls.Add(editor);
        }

        private void UpdateReturnControls()
        {
            _returnDate.Enabled = _hasReturnDate.Checked;
            if (!_hasReturnDate.Checked)
            {
                _lost.Checked = false;
                _payment.Value = 0;
            }
        }

        private void UpdateLostPayment()
        {
            if (!_lost.Checked)
            {
                return;
            }

            _hasReturnDate.Checked = true;
            if (_book.SelectedValue == null)
            {
                return;
            }

            using var command = new NpgsqlCommand("select cost from books where id = :id", _connection);
            command.Parameters.AddWithValue("id", Convert.ToInt32(_book.SelectedValue, CultureInfo.InvariantCulture));
            var cost = command.ExecuteScalar();
            if (cost != null && cost != DBNull.Value)
            {
                _payment.Value = Convert.ToDecimal(cost, CultureInfo.InvariantCulture);
            }
        }

        private void AddButtons(int y)
        {
            var ok = new Button { Text = "Сохранить", Location = new Point(315, y), Width = 95 };
            var cancel = new Button { Text = "Отмена", Location = new Point(420, y), Width = 95 };
            ok.Click += (_, _) =>
            {
                if (_student.SelectedValue == null)
                {
                    MessageBox.Show("Выберите студента.");
                    return;
                }

                if (_book.SelectedValue == null)
                {
                    MessageBox.Show("Выберите книгу.");
                    return;
                }

                if (DueDate < IssueDate)
                {
                    MessageBox.Show("Срок возврата не может быть раньше даты выдачи.");
                    return;
                }

                if (ReturnDate.HasValue && ReturnDate.Value < IssueDate)
                {
                    MessageBox.Show("Дата возврата не может быть раньше даты выдачи.");
                    return;
                }

                DialogResult = DialogResult.OK;
            };
            cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, cancel]);
        }
    }
}
