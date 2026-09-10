CREATE TABLE dbo.Usuarios
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Nick NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(250) NOT NULL
);
GO

CREATE INDEX IX_Usuarios_Nick
    ON dbo.Usuarios (Nick);
GO

CREATE INDEX IX_Usuarios_Nombre
    ON dbo.Usuarios (Nombre);
GO

CREATE TABLE dbo.ItemsTrabajo
(
    IdItem UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ItemsTrabajo PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    Titulo NVARCHAR(200) NOT NULL,
    FechaCreacion DATETIME2(0) NOT NULL,
    FechaEntrega DATETIME2(0) NOT NULL,
    Relevancia NVARCHAR(4) NOT NULL,
    EstadoOrden NVARCHAR(10) NOT NULL,
	Estado BIT NOT NULL DEFAULT (1)
    NombreUsuarioAsignado NVARCHAR(100) NULL,
    FechaAsignacion DATETIME2(0) NULL,
    FechaCompletado DATETIME2(0) NULL
);
GO

CREATE INDEX IX_ItemsTrabajo_UsuarioEstado
    ON dbo.ItemsTrabajo (NombreUsuarioAsignado, Estado)
    INCLUDE (Relevancia, FechaEntrega);
GO

CREATE INDEX IX_ItemsTrabajo_Distribucion
    ON dbo.ItemsTrabajo (Estado, NombreUsuarioAsignado, FechaEntrega)
    INCLUDE (Relevancia);
GO
