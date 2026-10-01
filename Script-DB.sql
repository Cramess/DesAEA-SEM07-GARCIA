CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;
GO

CREATE TABLE Autores (
    AutorId      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(100) NOT NULL,
    Nacionalidad NVARCHAR(60)  NULL,
    Activo       BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE Libros (
    LibroId    INT IDENTITY(1,1) PRIMARY KEY,
    Titulo     NVARCHAR(200) NOT NULL,
    ISBN       NVARCHAR(20)  NOT NULL UNIQUE,
    AutorId    INT           NOT NULL REFERENCES Autores(AutorId),
    Ejemplares INT           NOT NULL DEFAULT 1,
    Activo     BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE Socios (
    SocioId INT IDENTITY(1,1) PRIMARY KEY,
    DNI     NVARCHAR(15)  NOT NULL UNIQUE,
    Nombre  NVARCHAR(100) NOT NULL,
    Email   NVARCHAR(100) NULL,
    Activo  BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE Prestamos (
    PrestamoId    INT IDENTITY(1,1) PRIMARY KEY,
    SocioId       INT          NOT NULL REFERENCES Socios(SocioId),
    FechaPrestamo DATE         NOT NULL DEFAULT GETDATE(),
    FechaLimite   DATE         NOT NULL,
    Estado        NVARCHAR(20) NOT NULL DEFAULT 'Pendiente'
);
GO

CREATE TABLE DetallePrestamo (
    PrestamoId      INT           NOT NULL REFERENCES Prestamos(PrestamoId),
    LibroId         INT           NOT NULL REFERENCES Libros(LibroId),
    FechaDevolucion DATE          NULL,
    Multa           DECIMAL(8,2)  NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId)
);
GO

INSERT INTO Autores (Nombre, Nacionalidad) VALUES
    (N'Gabriel Garcia Marquez', N'Colombiana'),
    (N'Isabel Allende',         N'Chilena'),
    (N'Mario Vargas Llosa',     N'Peruana'),
    (N'Jorge Luis Borges',      N'Argentina'),
    (N'Octavio Paz',            N'Mexicana'),
    (N'Pablo Neruda',           N'Chilena'),
    (N'Julio Cortazar',         N'Argentina'),
    (N'Laura Restrepo',         N'Colombiana');
GO

INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES
    (N'Cien anos de soledad',            N'978-84-376-0494-7', 1, 4),
    (N'El amor en los tiempos del colera', N'978-84-376-0536-4', 1, 3),
    (N'La casa de los espiritus',         N'978-84-01-49906-9', 2, 3),
    (N'Eva Luna',                         N'978-84-01-49026-4', 2, 2),
    (N'La ciudad y los perros',           N'978-84-322-0286-7', 3, 2),
    (N'La fiesta del chivo',              N'978-84-204-4340-2', 3, 2),
    (N'Ficciones',                        N'978-84-665-8802-7', 4, 5),
    (N'El Aleph',                         N'978-84-665-8803-4', 4, 4),
    (N'Laberintos',                       N'978-84-665-8804-1', 4, 3),
    (N'El laberinto de la soledad',       N'978-84-376-0232-5', 5, 2),
    (N'Veinte poemas de amor',            N'978-84-206-3612-8', 6, 6),
    (N'Canto general',                    N'978-84-206-3613-5', 6, 3),
    (N'Rayuela',                          N'978-84-204-5502-3', 7, 4),
    (N'Bestiario',                        N'978-84-204-5503-0', 7, 2),
    (N'Los premios',                      N'978-84-204-5504-7', 7, 2),
    (N'La novia oscura',                  N'978-84-322-0800-5', 8, 2),
    (N'Leopardo al sol',                  N'978-84-322-0801-2', 8, 2),
    (N'Delirio',                          N'978-84-322-0802-9', 8, 3),
    (N'Historia de Mayta',                N'978-84-204-4341-9', 3, 2),
    (N'Conversacion en La Catedral',      N'978-84-204-4342-6', 3, 1);
GO

INSERT INTO Socios (DNI, Nombre, Email) VALUES
    (N'12345678', N'Ana Torres',    N'ana.torres@mail.com'),
    (N'23456789', N'Luis Ramos',    N'luis.ramos@mail.com'),
    (N'34567890', N'Carla Vega',    NULL),
    (N'45678901', N'Pedro Flores',  N'pedro.flores@mail.com'),
    (N'56789012', N'Maria Quispe',  N'maria.quispe@mail.com'),
    (N'67890123', N'Jose Mamani',   NULL),
    (N'78901234', N'Rosa Huanca',   N'rosa.huanca@mail.com'),
    (N'89012345', N'Carlos Diaz',   N'carlos.diaz@mail.com'),
    (N'90123456', N'Elena Paredes', NULL),
    (N'01234567', N'Socio Limite',  N'limite@mail.com');
GO

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
    (1, '2026-09-01', '2026-09-16', 'Devuelto'),
    (2, '2026-09-15', '2026-09-30', 'Pendiente'),
    (3, '2026-09-20', '2026-10-05', 'Pendiente'),
    (10,'2026-09-10', '2026-09-25', 'Pendiente'),
    (10,'2026-09-12', '2026-09-27', 'Pendiente');
GO

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion, Multa) VALUES
    (1, 1, '2026-09-10', 0),
    (1, 7, '2026-09-20', 6.00);
GO
UPDATE Libros SET Ejemplares = Ejemplares + 2 WHERE LibroId IN (1, 7);
GO

INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (2, 5);
INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (3, 13), (3, 11);
INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (4, 2);
INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (5, 8);
GO

UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId IN (5, 13, 11, 2, 8);
GO



USE BibliotecaDB;
SELECT (SELECT COUNT(*) FROM Autores)         AS Autores,
       (SELECT COUNT(*) FROM Libros)          AS Libros,
       (SELECT COUNT(*) FROM Socios)          AS Socios,
       (SELECT COUNT(*) FROM Prestamos)       AS Prestamos,
       (SELECT COUNT(*) FROM DetallePrestamo) AS Detalles;