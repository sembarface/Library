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

CREATE TABLE IF NOT EXISTS book_loans (
    id SERIAL PRIMARY KEY,
    student_id INTEGER NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    loan_date DATE NOT NULL,
    due_date DATE NOT NULL,
    payment_amount NUMERIC(10, 2) NOT NULL DEFAULT 0 CHECK (payment_amount >= 0),
    CONSTRAINT chk_loan_dates CHECK (due_date >= loan_date)
);

CREATE TABLE IF NOT EXISTS book_loan_items (
    loan_id INTEGER NOT NULL REFERENCES book_loans(id) ON DELETE CASCADE,
    book_id INTEGER NOT NULL REFERENCES books(id) ON DELETE CASCADE,
    return_date DATE,
    lost BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (loan_id, book_id),
    CONSTRAINT chk_lost_return CHECK (lost = FALSE OR return_date IS NULL)
);

CREATE INDEX IF NOT EXISTS idx_students_university_id
ON students(university_id);

CREATE INDEX IF NOT EXISTS idx_book_loans_student_id
ON book_loans(student_id);

CREATE INDEX IF NOT EXISTS idx_book_loans_loan_date
ON book_loans(loan_date);

CREATE INDEX IF NOT EXISTS idx_book_loans_due_date
ON book_loans(due_date);

CREATE INDEX IF NOT EXISTS idx_book_loans_payment_amount
ON book_loans(payment_amount);

CREATE INDEX IF NOT EXISTS idx_book_loan_items_loan_id
ON book_loan_items(loan_id);

CREATE INDEX IF NOT EXISTS idx_book_loan_items_book_id
ON book_loan_items(book_id);

CREATE INDEX IF NOT EXISTS idx_book_loan_items_return_date
ON book_loan_items(return_date);

CREATE INDEX IF NOT EXISTS idx_book_loan_items_lost
ON book_loan_items(lost);

CREATE UNIQUE INDEX IF NOT EXISTS uq_active_book_loan
ON book_loan_items(book_id)
WHERE return_date IS NULL;
