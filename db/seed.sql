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
    (4, 'PostgreSQL. Основы', 'Обе Р.', 1450.00)
ON CONFLICT (id) DO NOTHING;

INSERT INTO book_issues (id, student_id, book_id, issue_date, due_date, return_date, lost, payment_amount) VALUES
    (1, 1, 1, CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '16 days', NULL, FALSE, 0),
    (2, 2, 2, CURRENT_DATE - INTERVAL '20 days', CURRENT_DATE - INTERVAL '6 days', CURRENT_DATE - INTERVAL '2 days', FALSE, 0),
    (3, 3, 3, CURRENT_DATE - INTERVAL '25 days', CURRENT_DATE - INTERVAL '10 days', CURRENT_DATE, TRUE, 2100.00),
    (4, 4, 4, CURRENT_DATE - INTERVAL '5 days', CURRENT_DATE + INTERVAL '9 days', NULL, FALSE, 0)
ON CONFLICT (id) DO NOTHING;

SELECT setval('universities_id_seq', (SELECT COALESCE(MAX(id), 1) FROM universities));
SELECT setval('students_id_seq', (SELECT COALESCE(MAX(id), 1) FROM students));
SELECT setval('books_id_seq', (SELECT COALESCE(MAX(id), 1) FROM books));
SELECT setval('book_issues_id_seq', (SELECT COALESCE(MAX(id), 1) FROM book_issues));
