namespace Library
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControlMain;
        private TabPage tabUniversities;
        private TabPage tabBooks;
        private TabPage tabStudentsIssues;
        private TabPage tabReport;

        private DataGridView _universitiesGrid;
        private DataGridView _booksGrid;
        private DataGridView _studentsGrid;
        private DataGridView _issuesGrid;
        private DataGridView _reportGrid;

        private FlowLayoutPanel panelUniversitiesButtons;
        private FlowLayoutPanel panelBooksButtons;
        private FlowLayoutPanel panelStudentsButtons;
        private FlowLayoutPanel panelIssuesButtons;
        private Panel panelReportFilters;
        private SplitContainer splitStudentsIssues;
        private SplitContainer splitReport;

        private Button buttonAddUniversity;
        private Button buttonEditUniversity;
        private Button buttonDeleteUniversity;
        private Button buttonAddBook;
        private Button buttonEditBook;
        private Button buttonDeleteBook;
        private Button buttonAddStudent;
        private Button buttonEditStudent;
        private Button buttonDeleteStudent;
        private Button buttonAddIssue;
        private Button buttonEditIssue;
        private Button buttonDeleteIssue;
        private Button buttonMarkReturned;
        private Button buttonMarkLost;
        private Button buttonSelectAllUniversities;
        private Button buttonClearUniversities;
        private Button buttonBuildReport;
        private Button buttonExportReport;

        private Label labelReportDate;
        private Label labelReportUniversities;
        private CheckedListBox _reportUniversities;
        private DateTimePicker _reportDate;
        private Panel _chartPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControlMain = new TabControl();
            tabUniversities = new TabPage();
            tabBooks = new TabPage();
            tabStudentsIssues = new TabPage();
            tabReport = new TabPage();

            _universitiesGrid = new DataGridView();
            _booksGrid = new DataGridView();
            _studentsGrid = new DataGridView();
            _issuesGrid = new DataGridView();
            _reportGrid = new DataGridView();

            panelUniversitiesButtons = new FlowLayoutPanel();
            panelBooksButtons = new FlowLayoutPanel();
            panelStudentsButtons = new FlowLayoutPanel();
            panelIssuesButtons = new FlowLayoutPanel();
            panelReportFilters = new Panel();
            splitStudentsIssues = new SplitContainer();
            splitReport = new SplitContainer();

            buttonAddUniversity = new Button();
            buttonEditUniversity = new Button();
            buttonDeleteUniversity = new Button();
            buttonAddBook = new Button();
            buttonEditBook = new Button();
            buttonDeleteBook = new Button();
            buttonAddStudent = new Button();
            buttonEditStudent = new Button();
            buttonDeleteStudent = new Button();
            buttonAddIssue = new Button();
            buttonEditIssue = new Button();
            buttonDeleteIssue = new Button();
            buttonMarkReturned = new Button();
            buttonMarkLost = new Button();
            buttonSelectAllUniversities = new Button();
            buttonClearUniversities = new Button();
            buttonBuildReport = new Button();
            buttonExportReport = new Button();

            labelReportDate = new Label();
            labelReportUniversities = new Label();
            _reportUniversities = new CheckedListBox();
            _reportDate = new DateTimePicker();
            _chartPanel = new Panel();

            ((System.ComponentModel.ISupportInitialize)_universitiesGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_booksGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_studentsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_issuesGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_reportGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitStudentsIssues).BeginInit();
            splitStudentsIssues.Panel1.SuspendLayout();
            splitStudentsIssues.Panel2.SuspendLayout();
            splitStudentsIssues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitReport).BeginInit();
            splitReport.Panel1.SuspendLayout();
            splitReport.Panel2.SuspendLayout();
            splitReport.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabUniversities.SuspendLayout();
            tabBooks.SuspendLayout();
            tabStudentsIssues.SuspendLayout();
            tabReport.SuspendLayout();
            SuspendLayout();

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Библиотека выдач книг студентам ВУЗов";
            Load += Form1_Load;
            FormClosed += Form1_FormClosed;

            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Controls.Add(tabUniversities);
            tabControlMain.Controls.Add(tabBooks);
            tabControlMain.Controls.Add(tabStudentsIssues);
            tabControlMain.Controls.Add(tabReport);

            tabUniversities.Text = "ВУЗы";
            tabUniversities.Controls.Add(_universitiesGrid);
            tabUniversities.Controls.Add(panelUniversitiesButtons);

            tabBooks.Text = "Книги";
            tabBooks.Controls.Add(_booksGrid);
            tabBooks.Controls.Add(panelBooksButtons);

            tabStudentsIssues.Text = "Студенты и выдачи";
            tabStudentsIssues.Controls.Add(splitStudentsIssues);

            tabReport.Text = "Отчет";
            tabReport.Controls.Add(splitReport);
            tabReport.Controls.Add(panelReportFilters);

            ConfigureGrid(_universitiesGrid);
            ConfigureGrid(_booksGrid);
            ConfigureGrid(_studentsGrid);
            ConfigureGrid(_issuesGrid);
            ConfigureGrid(_reportGrid);

            ConfigureButtonPanel(panelUniversitiesButtons);
            panelUniversitiesButtons.Controls.Add(buttonAddUniversity);
            panelUniversitiesButtons.Controls.Add(buttonEditUniversity);
            panelUniversitiesButtons.Controls.Add(buttonDeleteUniversity);

            ConfigureButton(buttonAddUniversity, "Добавить", 120);
            ConfigureButton(buttonEditUniversity, "Изменить", 120);
            ConfigureButton(buttonDeleteUniversity, "Удалить", 120);
            buttonAddUniversity.Click += AddUniversityButton_Click;
            buttonEditUniversity.Click += EditUniversityButton_Click;
            buttonDeleteUniversity.Click += DeleteUniversityButton_Click;

            ConfigureButtonPanel(panelBooksButtons);
            panelBooksButtons.Controls.Add(buttonAddBook);
            panelBooksButtons.Controls.Add(buttonEditBook);
            panelBooksButtons.Controls.Add(buttonDeleteBook);

            ConfigureButton(buttonAddBook, "Добавить", 120);
            ConfigureButton(buttonEditBook, "Изменить", 120);
            ConfigureButton(buttonDeleteBook, "Удалить", 120);
            buttonAddBook.Click += AddBookButton_Click;
            buttonEditBook.Click += EditBookButton_Click;
            buttonDeleteBook.Click += DeleteBookButton_Click;

            splitStudentsIssues.Dock = DockStyle.Fill;
            splitStudentsIssues.Orientation = Orientation.Horizontal;
            splitStudentsIssues.SplitterDistance = 300;
            splitStudentsIssues.Panel1.Controls.Add(_studentsGrid);
            splitStudentsIssues.Panel1.Controls.Add(panelStudentsButtons);
            splitStudentsIssues.Panel2.Controls.Add(_issuesGrid);
            splitStudentsIssues.Panel2.Controls.Add(panelIssuesButtons);

            _studentsGrid.SelectionChanged += StudentsGrid_SelectionChanged;

            ConfigureButtonPanel(panelStudentsButtons);
            panelStudentsButtons.Controls.Add(buttonAddStudent);
            panelStudentsButtons.Controls.Add(buttonEditStudent);
            panelStudentsButtons.Controls.Add(buttonDeleteStudent);

            ConfigureButton(buttonAddStudent, "Добавить студента", 150);
            ConfigureButton(buttonEditStudent, "Изменить студента", 155);
            ConfigureButton(buttonDeleteStudent, "Удалить студента", 145);
            buttonAddStudent.Click += AddStudentButton_Click;
            buttonEditStudent.Click += EditStudentButton_Click;
            buttonDeleteStudent.Click += DeleteStudentButton_Click;

            ConfigureButtonPanel(panelIssuesButtons);
            panelIssuesButtons.Controls.Add(buttonAddIssue);
            panelIssuesButtons.Controls.Add(buttonEditIssue);
            panelIssuesButtons.Controls.Add(buttonDeleteIssue);
            panelIssuesButtons.Controls.Add(buttonMarkReturned);
            panelIssuesButtons.Controls.Add(buttonMarkLost);

            ConfigureButton(buttonAddIssue, "Добавить выдачу", 145);
            ConfigureButton(buttonEditIssue, "Изменить выдачу", 150);
            ConfigureButton(buttonDeleteIssue, "Удалить выдачу", 140);
            ConfigureButton(buttonMarkReturned, "Вернуть книгу", 130);
            ConfigureButton(buttonMarkLost, "Книга утеряна", 130);
            buttonAddIssue.Click += AddIssueButton_Click;
            buttonEditIssue.Click += EditIssueButton_Click;
            buttonDeleteIssue.Click += DeleteIssueButton_Click;
            buttonMarkReturned.Click += MarkReturnedButton_Click;
            buttonMarkLost.Click += MarkLostButton_Click;

            panelReportFilters.Dock = DockStyle.Top;
            panelReportFilters.Height = 150;
            panelReportFilters.Padding = new Padding(10);
            panelReportFilters.Controls.Add(labelReportDate);
            panelReportFilters.Controls.Add(_reportDate);
            panelReportFilters.Controls.Add(labelReportUniversities);
            panelReportFilters.Controls.Add(_reportUniversities);
            panelReportFilters.Controls.Add(buttonSelectAllUniversities);
            panelReportFilters.Controls.Add(buttonClearUniversities);
            panelReportFilters.Controls.Add(buttonBuildReport);
            panelReportFilters.Controls.Add(buttonExportReport);

            labelReportDate.AutoSize = true;
            labelReportDate.Location = new Point(10, 14);
            labelReportDate.Text = "Дата отчета";

            _reportDate.Format = DateTimePickerFormat.Short;
            _reportDate.Location = new Point(105, 10);
            _reportDate.Width = 120;

            labelReportUniversities.AutoSize = true;
            labelReportUniversities.Location = new Point(10, 47);
            labelReportUniversities.Text = "ВУЗы";

            _reportUniversities.CheckOnClick = true;
            _reportUniversities.Location = new Point(105, 44);
            _reportUniversities.Size = new Size(350, 90);

            ConfigureReportButton(buttonSelectAllUniversities, "Выбрать все", 470, 44);
            ConfigureReportButton(buttonClearUniversities, "Снять выбор", 470, 82);
            ConfigureReportButton(buttonBuildReport, "Построить отчет", 635, 44);
            ConfigureReportButton(buttonExportReport, "Экспорт в Excel", 635, 82);
            buttonSelectAllUniversities.Click += SelectAllReportUniversitiesButton_Click;
            buttonClearUniversities.Click += ClearReportUniversitiesButton_Click;
            buttonBuildReport.Click += BuildReportButton_Click;
            buttonExportReport.Click += ExportReportButton_Click;

            splitReport.Dock = DockStyle.Fill;
            splitReport.Orientation = Orientation.Horizontal;
            splitReport.SplitterDistance = 350;
            splitReport.Panel1.Controls.Add(_reportGrid);
            splitReport.Panel2.Controls.Add(_chartPanel);

            _chartPanel.Dock = DockStyle.Fill;
            _chartPanel.BackColor = Color.White;
            _chartPanel.Paint += DrawChart;

            Controls.Add(tabControlMain);

            splitReport.Panel2.ResumeLayout(false);
            splitReport.Panel1.ResumeLayout(false);
            splitReport.ResumeLayout(false);
            splitStudentsIssues.Panel2.ResumeLayout(false);
            splitStudentsIssues.Panel1.ResumeLayout(false);
            splitStudentsIssues.ResumeLayout(false);
            tabReport.ResumeLayout(false);
            tabStudentsIssues.ResumeLayout(false);
            tabBooks.ResumeLayout(false);
            tabUniversities.ResumeLayout(false);
            tabControlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_reportGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_issuesGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_studentsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_booksGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_universitiesGrid).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigureGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
        }

        private static void ConfigureButtonPanel(FlowLayoutPanel panel)
        {
            panel.Dock = DockStyle.Top;
            panel.Height = 56;
            panel.Padding = new Padding(8);
            panel.FlowDirection = FlowDirection.LeftToRight;
        }

        private static void ConfigureButton(Button button, string text, int width)
        {
            button.Text = text;
            button.Width = width;
            button.Height = 32;
            button.Margin = new Padding(0, 0, 8, 0);
        }

        private static void ConfigureReportButton(Button button, string text, int x, int y)
        {
            button.Text = text;
            button.Location = new Point(x, y);
            button.Width = 145;
            button.Height = 30;
        }
    }
}
