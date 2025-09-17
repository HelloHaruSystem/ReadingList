CREATE DATABASE reading_list;
-- uncomment below for SSMS
-- GO

USE reading_list;
-- uncomment below for SSMS
-- GO

CREATE TABLE authors (
    id int IDENTITY(1,1) PRIMARY KEY,
    first_name nvarchar(100),
    last_name nvarchar(100) NOT NULL,
    full_name AS (
        CASE
            WHEN first_name IS NOT NULL THEN first_name + ' ' + last_name
            ELSE last_name
        END
    ) PERSISTED,
    created_at datetime DEFAULT GETDATE()
);
-- uncomment below for SSMS
-- GO

CREATE TABLE subjects (
    id int IDENTITY(1,1) PRIMARY KEY,
    subject_name nvarchar(100) NOT NULL UNIQUE,
    created_at datetime DEFAULT GETDATE()
);
-- uncomment below for SSMS
-- GO

CREATE TABLE books (
    isbn nvarchar(20) PRIMARY KEY,
    title nvarchar(300) NOT NULL,
    publication_year int,
    pages int,
    description nvarchar(500),
    created_at datetime DEFAULT GETDATE(),
    updated_at datetime DEFAULT GETDATE()
);
-- uncomment below for SSMS
-- GO

CREATE TABLE book_authors (
    book_isbn nvarchar(20) NOT NULL,
    author_id int NOT NULL,
    author_order int DEFAULT 1, -- if multiple authors

    CONSTRAINT pk_book_authors PRIMARY KEY(book_isbn, author_id),
    CONSTRAINT fk_book_authors_book FOREIGN KEY (book_isbn) REFERENCES books(isbn) ON DELETE CASCADE,
    CONSTRAINT fk_book_authors_author FOREIGN KEY (author_id) REFERENCES authors(id) ON DELETE CASCADE
);
-- uncomment below for SSMS
-- GO

CREATE TABLE book_subjects (
    book_isbn nvarchar(20) NOT NULL,
    subject_id int NOT NULL,

    CONSTRAINT pk_book_subjects PRIMARY KEY (book_isbn, subject_id),
    CONSTRAINT fk_book_subjects_book FOREIGN KEY (book_isbn) REFERENCES books(isbn) ON DELETE CASCADE,
    CONSTRAINT fk_book_subjects_subject FOREIGN KEY (subject_id) REFERENCES subjects(id) ON DELETE CASCADE
);
-- uncomment below for SSMS
-- GO

-- reading status = enum values: to_read, currently_reading. completed, paused and abandoned
CREATE TABLE user_books (
    id int IDENTITY(1,1) PRIMARY KEY,
    book_isbn nvarchar(20) NOT NULL,
    reading_status nvarchar(20) NOT NULL DEFAULT 'to_read',
    personal_rating int CHECK(personal_rating >= 1 AND personal_rating <= 5),
    personal_notes nvarchar(500),
    date_started datetime DEFAULT GETDATE(),
    updated_at datetime DEFAULT GETDATE(),

    CONSTRAINT fk_user_books_book FOREIGN KEY(book_isbn) REFERENCES books(isbn) ON DELETE CASCADE,
    CHECK (reading_status IN ('to_read', 'currently_reading', 'completed', 'paused', 'abandoned'))
);
-- uncomment below for SSMS
-- GO

CREATE TABLE reading_goals (
    id int IDENTITY(1,1) PRIMARY KEY,
    goal_name nvarchar(200) NOT NULL,
    description nvarchar(500),
    start_date DATETIME NOT NULL,
    deadline DATETIME NOT NULL,
    target_books int,
    target_pages int,
    is_completed bit DEFAULT 0,

    CHECK (deadline >= start_date),
    CHECK (target_books > 0 OR target_pages > 0)
);
-- uncomment below for SSMS
-- GO

CREATE TABLE goal_books (
    goal_id int NOT NULL,
    book_isbn nvarchar(20) NOT NULL,
    added_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT pk_goal_books PRIMARY KEY (goal_id, book_isbn),
    CONSTRAINT fk_goal_books_goal FOREIGN KEY (goal_id) REFERENCES reading_goals(id) ON DELETE CASCADE,
    CONSTRAINT fk_goal_books_book FOREIGN KEY (book_isbn) REFERENCES books(isbn) ON DELETE CASCADE
);
-- uncomment below for SSMS
-- GO

-- Indexes for maybe? better performance
CREATE INDEX idx_books_title ON books(title);
-- uncomment below for SSMS
-- GO
CREATE INDEX idx_authors_last_name ON authors(last_name);
-- uncomment below for SSMS
-- GO
CREATE INDEX idx_user_books_status ON user_books(reading_status);
-- uncomment below for SSMS
-- GO
CREATE INDEX idx_user_books_book_isbn ON user_books(book_isbn);
-- uncomment below for SSMS
-- GO
CREATE INDEX idx_reading_goals_deadline ON reading_goals(deadline);
-- uncomment below for SSMS
-- GO

-- alter statement
ALTER TABLE user_books
ADD CONSTRAINT uk_user_books_isbn UNIQUE (book_isbn);
-- uncomment below for SSMS
-- GO

-- Initial Inserts
INSERT INTO subjects (subject_name)
VALUES
    ('Computer Science'),
    ('Programming'),
    ('Database Systems'),
    ('Algorithms'),
    ('Software Engineering'),
    ('Mathematics'),
    ('Machine Learning'),
    ('Operating Systems'),
    ('Computer Graphics'),
    ('Game Development'),
    ('Networking'),
    ('System Administration');
-- uncomment below for SSMS
-- GO

INSERT INTO authors (first_name, last_name)
VALUES
    ('Martin', 'Kleppmann'),
    ('David', 'Thomas'),
    ('Andrew', 'Hunt'),
    ('Dennis', 'Ritchie');
-- uncomment below for SSMS
-- GO

INSERT INTO books (isbn, title, publication_year, pages, description)
VALUES
    ('9781449373320', 'Designing Data-Intensive Applications', 2017, 616, 'The big ideas behind reliable, scalable, and maintainable systems'),
    ('9780135957059', 'The Pragmatic Programmer: Your Journey To Mastery, 20th Anniversary Edition', 2019, 352, 'Your journey to mastery, 20th Anniversary Edition'),
    ('9780201616224', 'The Pragmatic Programmer: From Journeyman to Master', 1999, 352, 'From Journeyman to Master (First Edition)'),
    ('9780131103627', 'The C Programming Language', 1988, 274, 'The definitive guide to C programming by its creators');
-- uncomment below for SSMS
-- GO

INSERT INTO book_authors (book_isbn, author_id, author_order)
VALUES
    ('9781449373320', 1, 1),  -- Martin Kleppmann for Designing Data-Intensive Applications
    ('9780135957059', 2, 1),  -- David Thomas for Pragmatic Programmer 20th Anniversary
    ('9780135957059', 3, 2),  -- Andrew Hunt for Pragmatic Programmer 20th Anniversary (co-author)
    ('9780201616224', 2, 1),  -- David Thomas for Pragmatic Programmer First Edition
    ('9780131103627', 4, 1);
-- uncomment below for SSMS
-- GO

INSERT INTO book_subjects (book_isbn, subject_id)
VALUES
    ('9781449373320', 3),  -- Designing Data-Intensive Applications for Database Systems
    ('9780135957059', 5),  -- Pragmatic Programmer 20th Anniversary for Software Engineering
    ('9780201616224', 5),  -- Pragmatic Programmer First Edition for Software Engineering
    ('9780131103627', 2);  -- C Programming Language for Programming
-- uncomment below for SSMS
-- GO

INSERT INTO user_books (book_isbn, reading_status, personal_rating, personal_notes)
VALUES
    ('9781449373320', 'completed', 5, 'Excellent overview of distributed systems concepts'),
    ('9780135957059', 'currently_reading', NULL, 'Great practical advice for developers'),
    ('9780201616224', 'to_read', NULL, 'Classic programming wisdom'),
    ('9780131103627', 'completed', 4, 'Essential C reference, still relevant today');
-- uncomment below for SSMS
-- GO

INSERT INTO reading_goals (goal_name, description, start_date, deadline, target_books, target_pages, is_completed)
VALUES
    ('2025 Programming Books', 'Read 12 programming books this year', '2025-01-01', '2025-12-31', 12, NULL, 0),
    ('Database Deep Dive', 'Focus on database and system design books', '2025-01-01', '2025-06-30', 5, NULL, 0),
    ('Quick Technical Reads', 'Read shorter technical books', '2025-01-01', '2025-03-31', 3, 1000, 0),
    ('Classic CS Texts', 'Work through fundamental computer science books', '2025-01-01', '2025-12-31', NULL, 2000, 0);
-- uncomment below for SSMS
-- GO

INSERT INTO goal_books (goal_id, book_isbn)
VALUES
    (1, '9781449373320'),
    (1, '9780135957059'),
    (2, '9781449373320'),
    (4, '9780131103627');
-- uncomment below for SSMS
-- GO

-- Update a book's page count
UPDATE books
SET pages = 620
WHERE isbn = '9781449373320';

-- Update a user's reading status
UPDATE user_books
SET reading_status = 'completed', personal_rating = 5
WHERE book_isbn = '9780135957059';

-- Update an author's first name
UPDATE authors
SET first_name = 'Dennis M.'
WHERE last_name = 'Ritchie';

-- Mark a reading goal as completed
UPDATE reading_goals
SET is_completed = 1
WHERE goal_name = 'Quick Technical Reads';

INSERT INTO authors (first_name, last_name)
VALUES
    -- Networking book authors
    ('James F.', 'Kurose'),
    ('Keith W.', 'Ross'),

    -- Zig Programming (assuming author - this book may not exist or have clear author info)
    -- Skipping as no clear author found

    -- Linear Algebra
    ('Sheldon', 'Axler'),

    -- Probability
    ('Dimitri P.', 'Bertsekas'),
    ('John N.', 'Tsitsiklis'),

    -- Introduction to Algorithms
    ('Thomas H.', 'Cormen'),
    ('Charles E.', 'Leiserson'),
    ('Ronald L.', 'Rivest'),
    ('Clifford', 'Stein'),

    -- Compilers (Dragon Book)
    ('Alfred V.', 'Aho'),
    ('Monica S.', 'Lam'),
    ('Ravi', 'Sethi'),
    ('Jeffrey D.', 'Ullman'),

    -- Database Systems
    ('Abraham', 'Silberschatz'),
    ('Henry F.', 'Korth'),
    ('S.', 'Sudarshan'),

    -- Computer Graphics
    ('Donald', 'Hearn'),
    ('M. Pauline', 'Baker'),
    ('Warren', 'Carithers'),

    -- AI Modern Approach
    ('Stuart', 'Russell'),
    ('Peter', 'Norvig'),

    -- Operating Systems
    -- Abraham Silberschatz already inserted above
    ('Peter Baer', 'Galvin'),
    ('Greg', 'Gagne'),

    -- Game Engine Architecture
    ('Jason', 'Gregory'),

    -- Linux Programming Interface
    ('Michael', 'Kerrisk'),

    -- SICP
    ('Harold', 'Abelson'),
    ('Gerald Jay', 'Sussman'),
    ('Julie', 'Sussman'),

    -- Art of Computer Programming
    ('Donald E.', 'Knuth'),

    -- Code: The Hidden Language
    ('Charles', 'Petzold'),

    -- Design Patterns (Gang of Four)
    ('Erich', 'Gamma'),
    ('Richard', 'Helm'),
    ('Ralph', 'Johnson'),
    ('John', 'Vlissides'),

    -- Head First Design Patterns
    ('Eric', 'Freeman'),
    ('Bert', 'Bates'),
    ('Kathy', 'Sierra'),
    ('Elisabeth', 'Robson'),

    -- Refactoring
    ('Martin', 'Fowler'),

    -- Clean Code
    ('Robert C.', 'Martin'),

    -- Systems Performance
    ('Brendan', 'Gregg'),

    -- Deep Learning
    ('Ian', 'Goodfellow'),
    ('Yoshua', 'Bengio'),
    ('Aaron', 'Courville'),

    -- Cracking the Coding Interview
    ('Gayle Laakmann', 'McDowell'),

    -- Learn You A Haskell
    ('Miran', 'Lipovača'),

    -- Crafting Interpreters
    ('Robert', 'Nystrom'),

    -- Game Programming Patterns
    -- Robert Nystrom already inserted above

    -- Regular Expressions Cookbook
    ('Jan', 'Goyvaerts'),
    ('Steven', 'Levithan');
-- uncomment below for SSMS
-- GO

-- INSERT statements for books

INSERT INTO books (isbn, title, publication_year, pages, description)
VALUES
    ('9780133594140', 'Computer Networking: A Top-Down Approach, Global Edition', 2017, 864, 'Top-down approach to computer networking'),
    ('9783031410253', 'Linear Algebra Done Right', 2024, 458, 'Determinant-free approach to linear algebra'),
    ('9781886529236', 'Introduction to Probability, Second Edition', 2008, 544, 'Comprehensive introduction to probability theory'),
    ('9780262046305', 'Introduction to Algorithms', 2022, 1312, 'Comprehensive textbook on algorithms and data structures'),
    ('9780321486813', 'Compilers: Principles, Techniques, and Tools', 2006, 1000, 'The Dragon Book - comprehensive guide to compiler design'),
    ('9781260084504', 'Database System Concepts', 2019, 1376, 'Comprehensive database systems textbook'),
    ('9780135021446', 'Computer Graphics with OpenGL', 2011, 976, 'Computer graphics programming with OpenGL'),
    ('9780134610993', 'Artificial Intelligence: A Modern Approach, Global Edition', 2021, 1136, 'Comprehensive AI textbook'),
    ('9781119320913', 'Operating System Concepts, Global Edition', 2018, 944, 'Fundamental operating systems concepts'),
    ('9781138035454', 'Game Engine Architecture', 2018, 1200, 'Comprehensive guide to game engine development'),
    ('9781593272203', 'The Linux Programming Interface', 2010, 1552, 'Comprehensive Linux and UNIX programming guide'),
    ('9780262510875', 'Structure and Interpretation of Computer Programs', 1996, 657, 'Classic computer science textbook'),
    ('9780321751041', 'The Art of Computer Programming, Volumes 1-4B, Boxed Set', 2011, 3168, 'Donald Knuth''s masterwork on programming'),
    ('9780735611313', 'Code: The Hidden Language of Computer Hardware and Software', 2000, 393, 'How computers work at the fundamental level'),
    ('9780201633610', 'Design Patterns: Elements of Reusable Object-Oriented Software', 1994, 395, 'Gang of Four design patterns book'),
    ('9781492078006', 'Head First Design Patterns', 2020, 672, 'Design patterns with a brain-friendly approach'),
    ('9780134757599', 'Refactoring: Improving the Design of Existing Code', 2018, 448, 'Second edition of the refactoring classic'),
    ('9780132350884', 'Clean Code: A Handbook of Agile Software Craftsmanship', 2008, 464, 'Writing clean, maintainable code'),
    ('9780136820154', 'Systems Performance', 2020, 880, 'Performance analysis and tuning'),
    ('9780262035613', 'Deep Learning', 2016, 800, 'Comprehensive deep learning textbook'),
    ('9780984782857', 'Cracking the Coding Interview', 2015, 687, 'Programming interview preparation'),
    ('9781593272838', 'Learn You a Haskell for Great Good!', 2011, 360, 'A beginner''s guide to Haskell'),
    ('9780990582939', 'Crafting Interpreters', 2021, 640, 'A hands-on guide to building interpreters'),
    ('9780990582922', 'Game Programming Patterns', 2014, 354, 'Design patterns specifically for game development'),
    ('9781449392680', 'Regular Expressions Cookbook', 2012, 612, 'Detailed solutions for regular expressions');
-- uncomment below for SSMS
-- GO

-- INSERT statements for book_authors (connecting books to their authors)

INSERT INTO book_authors (book_isbn, author_id, author_order)
VALUES
    -- Computer Networking (Kurose & Ross)
    ('9780133594140', (SELECT id FROM authors WHERE last_name = 'Kurose'), 1),
    ('9780133594140', (SELECT id FROM authors WHERE last_name = 'Ross'), 2),

    -- Linear Algebra Done Right (Axler)
    ('9783031410253', (SELECT id FROM authors WHERE last_name = 'Axler'), 1),

    -- Introduction to Probability (Bertsekas & Tsitsiklis)
    ('9781886529236', (SELECT id FROM authors WHERE last_name = 'Bertsekas'), 1),
    ('9781886529236', (SELECT id FROM authors WHERE last_name = 'Tsitsiklis'), 2),

    -- Introduction to Algorithms (CLRS)
    ('9780262046305', (SELECT id FROM authors WHERE last_name = 'Cormen'), 1),
    ('9780262046305', (SELECT id FROM authors WHERE last_name = 'Leiserson'), 2),
    ('9780262046305', (SELECT id FROM authors WHERE last_name = 'Rivest'), 3),
    ('9780262046305', (SELECT id FROM authors WHERE last_name = 'Stein'), 4),

    -- Compilers Dragon Book (Aho, Lam, Sethi, Ullman)
    ('9780321486813', (SELECT id FROM authors WHERE first_name = 'Alfred V.' AND last_name = 'Aho'), 1),
    ('9780321486813', (SELECT id FROM authors WHERE last_name = 'Lam'), 2),
    ('9780321486813', (SELECT id FROM authors WHERE last_name = 'Sethi'), 3),
    ('9780321486813', (SELECT id FROM authors WHERE first_name = 'Jeffrey D.' AND last_name = 'Ullman'), 4),

    -- Database System Concepts (Silberschatz, Korth, Sudarshan)
    ('9781260084504', (SELECT id FROM authors WHERE first_name = 'Abraham' AND last_name = 'Silberschatz'), 1),
    ('9781260084504', (SELECT id FROM authors WHERE last_name = 'Korth'), 2),
    ('9781260084504', (SELECT id FROM authors WHERE last_name = 'Sudarshan'), 3),

    -- Computer Graphics (Hearn, Baker, Carithers)
    ('9780135021446', (SELECT id FROM authors WHERE first_name = 'Donald' AND last_name = 'Hearn'), 1),
    ('9780135021446', (SELECT id FROM authors WHERE last_name = 'Baker'), 2),
    ('9780135021446', (SELECT id FROM authors WHERE last_name = 'Carithers'), 3),

    -- AI Modern Approach (Russell & Norvig)
    ('9780134610993', (SELECT id FROM authors WHERE last_name = 'Russell'), 1),
    ('9780134610993', (SELECT id FROM authors WHERE last_name = 'Norvig'), 2),

    -- Operating System Concepts (Silberschatz, Galvin, Gagne)
    ('9781119320913', (SELECT id FROM authors WHERE first_name = 'Abraham' AND last_name = 'Silberschatz'), 1),
    ('9781119320913', (SELECT id FROM authors WHERE last_name = 'Galvin'), 2),
    ('9781119320913', (SELECT id FROM authors WHERE last_name = 'Gagne'), 3),

    -- Game Engine Architecture (Gregory)
    ('9781138035454', (SELECT id FROM authors WHERE last_name = 'Gregory'), 1),

    -- Linux Programming Interface (Kerrisk)
    ('9781593272203', (SELECT id FROM authors WHERE last_name = 'Kerrisk'), 1),

    -- SICP (Abelson, Sussman, Sussman)
    ('9780262510875', (SELECT id FROM authors WHERE first_name = 'Harold' AND last_name = 'Abelson'), 1),
    ('9780262510875', (SELECT id FROM authors WHERE first_name = 'Gerald Jay' AND last_name = 'Sussman'), 2),
    ('9780262510875', (SELECT id FROM authors WHERE first_name = 'Julie' AND last_name = 'Sussman'), 3),

    -- Art of Computer Programming (Knuth)
    ('9780321751041', (SELECT id FROM authors WHERE last_name = 'Knuth'), 1),

    -- Code: The Hidden Language (Petzold)
    ('9780735611313', (SELECT id FROM authors WHERE last_name = 'Petzold'), 1),

    -- Design Patterns Gang of Four
    ('9780201633610', (SELECT id FROM authors WHERE last_name = 'Gamma'), 1),
    ('9780201633610', (SELECT id FROM authors WHERE last_name = 'Helm'), 2),
    ('9780201633610', (SELECT id FROM authors WHERE last_name = 'Johnson'), 3),
    ('9780201633610', (SELECT id FROM authors WHERE last_name = 'Vlissides'), 4),

    -- Head First Design Patterns
    ('9781492078006', (SELECT id FROM authors WHERE last_name = 'Freeman'), 1),
    ('9781492078006', (SELECT id FROM authors WHERE last_name = 'Bates'), 2),
    ('9781492078006', (SELECT id FROM authors WHERE last_name = 'Sierra'), 3),
    ('9781492078006', (SELECT id FROM authors WHERE last_name = 'Robson'), 4),

    -- Refactoring (Fowler)
    ('9780134757599', (SELECT id FROM authors WHERE last_name = 'Fowler'), 1),

    -- Clean Code (Martin)
    ('9780132350884', (SELECT id FROM authors WHERE first_name = 'Robert C.' AND last_name = 'Martin'), 1),

    -- Systems Performance (Gregg)
    ('9780136820154', (SELECT id FROM authors WHERE last_name = 'Gregg'), 1),

    -- Deep Learning (Goodfellow, Bengio, Courville)
    ('9780262035613', (SELECT id FROM authors WHERE last_name = 'Goodfellow'), 1),
    ('9780262035613', (SELECT id FROM authors WHERE last_name = 'Bengio'), 2),
    ('9780262035613', (SELECT id FROM authors WHERE last_name = 'Courville'), 3),

    -- Cracking the Coding Interview (McDowell)
    ('9780984782857', (SELECT id FROM authors WHERE last_name = 'McDowell'), 1),

    -- Learn You A Haskell (Lipovača)
    ('9781593272838', (SELECT id FROM authors WHERE last_name = 'Lipovača'), 1),

    -- Crafting Interpreters (Nystrom)
    ('9780990582939', (SELECT id FROM authors WHERE first_name = 'Robert' AND last_name = 'Nystrom'), 1),

    -- Game Programming Patterns (Nystrom)
    ('9780990582922', (SELECT id FROM authors WHERE first_name = 'Robert' AND last_name = 'Nystrom'), 1),

    -- Regular Expressions Cookbook (Goyvaerts & Levithan)
    ('9781449392680', (SELECT id FROM authors WHERE last_name = 'Goyvaerts'), 1),
    ('9781449392680', (SELECT id FROM authors WHERE last_name = 'Levithan'), 2);
-- uncomment below for SSMS
-- GO

-- INSERT statements for book_subjects (connecting books to subjects)

INSERT INTO book_subjects (book_isbn, subject_id)
VALUES
    -- Computer Networking
    ('9780133594140', (SELECT id FROM subjects WHERE subject_name = 'Networking')),

    -- Linear Algebra
    ('9783031410253', (SELECT id FROM subjects WHERE subject_name = 'Mathematics')),

    -- Introduction to Probability
    ('9781886529236', (SELECT id FROM subjects WHERE subject_name = 'Mathematics')),

    -- Introduction to Algorithms
    ('9780262046305', (SELECT id FROM subjects WHERE subject_name = 'Algorithms')),
    ('9780262046305', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Compilers
    ('9780321486813', (SELECT id FROM subjects WHERE subject_name = 'Programming')),
    ('9780321486813', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Database System Concepts
    ('9781260084504', (SELECT id FROM subjects WHERE subject_name = 'Database Systems')),

    -- Computer Graphics
    ('9780135021446', (SELECT id FROM subjects WHERE subject_name = 'Computer Graphics')),

    -- AI Modern Approach
    ('9780134610993', (SELECT id FROM subjects WHERE subject_name = 'Machine Learning')),
    ('9780134610993', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Operating System Concepts
    ('9781119320913', (SELECT id FROM subjects WHERE subject_name = 'Operating Systems')),

    -- Game Engine Architecture
    ('9781138035454', (SELECT id FROM subjects WHERE subject_name = 'Game Development')),
    ('9781138035454', (SELECT id FROM subjects WHERE subject_name = 'Software Engineering')),

    -- Linux Programming Interface
    ('9781593272203', (SELECT id FROM subjects WHERE subject_name = 'System Administration')),
    ('9781593272203', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- SICP
    ('9780262510875', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),
    ('9780262510875', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Art of Computer Programming
    ('9780321751041', (SELECT id FROM subjects WHERE subject_name = 'Algorithms')),
    ('9780321751041', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Code: The Hidden Language
    ('9780735611313', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Design Patterns
    ('9780201633610', (SELECT id FROM subjects WHERE subject_name = 'Software Engineering')),
    ('9780201633610', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Head First Design Patterns
    ('9781492078006', (SELECT id FROM subjects WHERE subject_name = 'Software Engineering')),
    ('9781492078006', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Refactoring
    ('9780134757599', (SELECT id FROM subjects WHERE subject_name = 'Software Engineering')),
    ('9780134757599', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Clean Code
    ('9780132350884', (SELECT id FROM subjects WHERE subject_name = 'Software Engineering')),
    ('9780132350884', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Systems Performance
    ('9780136820154', (SELECT id FROM subjects WHERE subject_name = 'System Administration')),
    ('9780136820154', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Deep Learning
    ('9780262035613', (SELECT id FROM subjects WHERE subject_name = 'Machine Learning')),
    ('9780262035613', (SELECT id FROM subjects WHERE subject_name = 'Mathematics')),

    -- Cracking the Coding Interview
    ('9780984782857', (SELECT id FROM subjects WHERE subject_name = 'Algorithms')),
    ('9780984782857', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Learn You A Haskell
    ('9781593272838', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Crafting Interpreters
    ('9780990582939', (SELECT id FROM subjects WHERE subject_name = 'Programming')),
    ('9780990582939', (SELECT id FROM subjects WHERE subject_name = 'Computer Science')),

    -- Game Programming Patterns
    ('9780990582922', (SELECT id FROM subjects WHERE subject_name = 'Game Development')),
    ('9780990582922', (SELECT id FROM subjects WHERE subject_name = 'Programming')),

    -- Regular Expressions Cookbook
    ('9781449392680', (SELECT id FROM subjects WHERE subject_name = 'Programming'));
-- uncomment below for SSMS
-- GO

SELECT * FROM books;

-- Remove the unique constraint to be able to alter status
ALTER TABLE user_books DROP CONSTRAINT uk_user_books_isbn;
