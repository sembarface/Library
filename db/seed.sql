INSERT INTO universities (id, name) VALUES
    (1, 'Московский государственный университет'),
    (2, 'Санкт-Петербургский политехнический университет'),
    (3, 'Казанский федеральный университет')
ON CONFLICT (id) DO NOTHING;

INSERT INTO students (id, full_name, university_id, student_group, phone) VALUES
    (1, 'Иванов Иван Сергеевич', 1, 'ПИ-22', '+7 900 100-10-10'),
    (2, 'Петрова Анна Викторовна', 1, 'ИВТ-21', '+7 900 100-10-11'),
    (3, 'Сидоров Павел Андреевич', 2, 'БИ-23', '+7 900 100-10-12'),
    (4, 'Ахметова Лилия Ринатовна', 3, 'ПМ-22', '+7 900 100-10-13')
ON CONFLICT (id) DO NOTHING;

INSERT INTO books (id, title, author, cost) VALUES
    (1, 'Язык программирования C#', 'Шилдт Г.', 1250.00),
    (2, 'Базы данных. Проектирование', 'Карпова И.П.', 980.00),
    (3, 'Алгоритмы: построение и анализ', 'Кормен Т.', 2100.00),
    (4, 'PostgreSQL. Основы', 'Обе Р.', 1450.00),
    (5, 'PostgreSQL. Основы', 'Обе Р.', 1450.00)
ON CONFLICT (id) DO NOTHING;

INSERT INTO book_loans (id, student_id, loan_date, due_date, payment_amount) VALUES
    (1, 1, CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '16 days', 0),
    (2, 2, CURRENT_DATE - INTERVAL '20 days', CURRENT_DATE - INTERVAL '6 days', 0),
    (3, 3, CURRENT_DATE - INTERVAL '25 days', CURRENT_DATE - INTERVAL '10 days', 2100.00),
    (4, 4, CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE + INTERVAL '9 days', 0)
ON CONFLICT (id) DO NOTHING;

INSERT INTO book_loan_items (loan_id, book_id, return_date, lost) VALUES
    (1, 1, NULL, FALSE),
    (1, 4, NULL, FALSE),
    (2, 2, CURRENT_DATE - INTERVAL '2 days', FALSE),
    (3, 3, NULL, TRUE),
    (4, 5, NULL, FALSE)
ON CONFLICT (loan_id, book_id) DO NOTHING;

SELECT setval('universities_id_seq', (SELECT COALESCE(MAX(id), 1) FROM universities));
SELECT setval('students_id_seq', (SELECT COALESCE(MAX(id), 1) FROM students));
SELECT setval('books_id_seq', (SELECT COALESCE(MAX(id), 1) FROM books));
SELECT setval('book_loans_id_seq', (SELECT COALESCE(MAX(id), 1) FROM book_loans));
