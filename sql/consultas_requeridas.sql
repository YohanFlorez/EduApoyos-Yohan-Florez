
-- EduApoyos — Scripts SQL requeridos (sección 4.2 de la prueba técnica)
-- Motor de base de datos: SQL Server. Nombres de tabla/columnas según las migraciones EF Core.

-- 1) Solicitudes pendientes con más de 5 días sin actualización, ordenadas por antigüedad.
--    Antigüedad = tiempo desde FechaActualizacion .
SELECT
    s.Id,
    s.EstudianteId,
    s.TipoApoyo,
    s.MontoSolicitado,
    s.Estado,
    s.FechaSolicitud,
    s.FechaActualizacion,
    DATEDIFF(DAY, s.FechaActualizacion, GETUTCDATE()) AS DiasSinActualizar
FROM dbo.Solicitudes AS s
WHERE s.Estado = 1 -- 1 = Pendiente (ver enum EstadoSolicitud)
  AND s.FechaActualizacion <= DATEADD(DAY, -5, GETUTCDATE())
ORDER BY s.FechaActualizacion ASC; -- más antiguas primero


-- 2) Total de solicitudes agrupadas por estado y tipo de apoyo, en el último mes.
SELECT
    s.Estado,
    s.TipoApoyo,
    COUNT(*) AS TotalSolicitudes
FROM dbo.Solicitudes AS s
WHERE s.FechaSolicitud >= DATEADD(MONTH, -1, GETUTCDATE())
GROUP BY s.Estado, s.TipoApoyo
ORDER BY s.Estado, s.TipoApoyo;


-- 3) Índice para la tabla de solicitudes
--    ¿Por qué este índice? Porque las dos pantallas que más se van a usar son:
--      a) el panel del asesor, donde se filtran las solicitudes por su estado
--         y se ordenan por la fecha en que se actualizaron (la consulta #1 hace justo esto);
--      b) los reportes, donde se cuentan las solicitudes agrupadas por estado y tipo de apoyo (consulta #2).
--    Sin este índice, la base de datos tendría que revisar solicitud por solicitud
--    cada vez que alguien abre estas pantallas, lo cual se vuelve lento a medida que crecen los datos.
--    Con el índice, la base de datos puede ir directo a los registros que necesita.
--    Nota: este índice ya está definido también en el código (EF Core), así que se crea solo
--    al aplicar las migraciones. Este script se deja aparte por si se necesita aplicar
--    directamente sobre una base de datos que ya existe.
CREATE NONCLUSTERED INDEX IX_Solicitudes_Estado_FechaActualizacion
    ON dbo.Solicitudes (Estado, FechaActualizacion)
    INCLUDE (EstudianteId, TipoApoyo, MontoSolicitado);
GO

-- Índice adicional (también en el modelo EF) para acelerar "solicitudes de un estudiante" .
CREATE NONCLUSTERED INDEX IX_Solicitudes_EstudianteId
    ON dbo.Solicitudes (EstudianteId);
GO
