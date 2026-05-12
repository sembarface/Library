CREATE TABLE IF NOT EXISTS universities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS students (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    university_id INTEGER NOT NULL REFERENCES universities(id) ON DELETE CASCADE,
    student_group VARCHAR(30) NOT NULL,
    phone VARCHAR(30)
);

CREATE TABLE IF NOT EXISTS books (
    id SERIAL PRIMARY KEY,
    title VARCHAR(150) NOT NULL,
    author VARCHAR(150) NOT NULL,
    cost NUMERIC(10, 2) NOT NULL CHECK (cost >= 0)
);

CREATE TABLE IF NOT EXISTS book_issues (
    id SERIAL PRIMARY KEY,
    student_id INTEGER NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    book_id INTEGER NOT NULL REFERENCES books(id) ON DELETE CASCADE,
    issue_date DATE NOT NULL,
    due_date DATE NOT NULL,
    return_date DATE,
    lost BOOLEAN NOT NULL DEFAULT FALSE,
    payment_amount NUMERIC(10, 2) NOT NULL DEFAULT 0 CHECK (payment_amount >= 0),
    CONSTRAINT chk_issue_dates CHECK (due_date >= issue_date),
    CONSTRAINT chk_return_date CHECK (return_date IS NULL OR return_date >= issue_date)
);

CREATE INDEX IF NOT EXISTS idx_students_university_id ON students(university_id);
CREATE INDEX IF NOT EXISTS idx_book_issues_student_id ON book_issues(student_id);
CREATE INDEX IF NOT EXISTS idx_book_issues_book_id ON book_issues(book_id);
CREATE INDEX IF NOT EXISTS idx_book_issues_due_date ON book_issues(due_date);
