-- =====================================================
--  CLÍNICA SEPRISE - Base de datos
--  Consultorios Externos y Estudios Diagnósticos
-- =====================================================

DROP DATABASE IF EXISTS ClinicaSePrise;
CREATE DATABASE ClinicaSePrise;
USE ClinicaSePrise;

-- =====================================================
--  TABLAS BASE
-- =====================================================

CREATE TABLE Paciente (
    idPaciente INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    dni VARCHAR(20) UNIQUE NOT NULL,
    telefono VARCHAR(20),
    email VARCHAR(255),
    modalidadCobertura VARCHAR(50),
    obraSocial VARCHAR(100),
    nroAfiliado VARCHAR(50),
    nroHistoriaClinica VARCHAR(50)
);

CREATE TABLE Profesional (
    idProfesional INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    nroMatricula VARCHAR(50) UNIQUE NOT NULL,
    especialidad VARCHAR(100),
    consultorioAsignado VARCHAR(50),
    insumosRequeridos VARCHAR(100)
);

-- =====================================================
--  ROLES Y USUARIOS (login y permisos por actor)
-- =====================================================

CREATE TABLE Rol (
    idRol INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(30) UNIQUE NOT NULL
);

CREATE TABLE RolPantalla (
    idRol INT NOT NULL,
    pantalla VARCHAR(30) NOT NULL,
    PRIMARY KEY (idRol, pantalla),
    FOREIGN KEY (idRol) REFERENCES Rol(idRol)
);

CREATE TABLE Usuario (
    idUsuario INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    usuario VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    idRol INT NOT NULL,
    idPaciente INT NULL,
    idProfesional INT NULL,
    FOREIGN KEY (idRol) REFERENCES Rol(idRol),
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

-- =====================================================
--  CIRCUITO A: CONSULTORIOS EXTERNOS
-- =====================================================

CREATE TABLE Turno (
    idTurno INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idProfesional INT,
    especialidad VARCHAR(100),
    fechaHora DATETIME,
    tipoTurno VARCHAR(50),
    duracion INT,
    modalidadSolicitud VARCHAR(50),
    estado VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

CREATE TABLE Agenda (
    idAgenda INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idProfesional INT,
    especialidad VARCHAR(100),
    disponibilidad VARCHAR(100),
    diaSemana TINYINT,
    horaDesde TIME,
    horaHasta TIME,
    duracionMinima INT,
    sobreturnosPermitidos INT,
    recordatorio BOOLEAN,
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

CREATE TABLE Atencion (
    idAtencion INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idProfesional INT,
    fechaHoraFin DATETIME,
    indicacionNuevoTurno BOOLEAN,
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

CREATE TABLE Honorario (
    idHonorario INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idProfesional INT,
    periodoLiquidacion VARCHAR(50),
    cantidadPacientes INT,
    monto DECIMAL(12,2),
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

-- =====================================================
--  CIRCUITO B: ESTUDIOS DIAGNÓSTICOS
-- =====================================================

CREATE TABLE Estudio (
    idEstudio INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    especialidad VARCHAR(100),
    requisitosPrevios VARCHAR(100),
    horarioEspecifico VARCHAR(50),
    responsable VARCHAR(100)
);

CREATE TABLE TurnoEstudio (
    idTurno INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idEstudio INT,
    fechaHora DATETIME,
    modalidadSolicitud VARCHAR(50),
    requiereTurno BOOLEAN,
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio)
);

CREATE TABLE AgendaEstudio (
    idAgenda INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    sector VARCHAR(100),
    disponibilidad VARCHAR(100),
    duracionEstimada INT,
    recordatorio BOOLEAN
);

CREATE TABLE Insumo (
    idInsumo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100),
    idEstudio INT,
    stock INT,
    stockMinimo INT NOT NULL DEFAULT 10,
    estadoReposicion VARCHAR(50),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio)
);

CREATE TABLE Comprobante (
    idComprobante INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idEstudio INT,
    fechaAtencion DATETIME,
    comprobanteRetiro BOOLEAN,
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio)
);

CREATE TABLE Derivacion (
    idDerivacion INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    origen VARCHAR(50),
    idEstudio INT,
    estado VARCHAR(50),
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio)
);

-- =====================================================
--  FACTURAS (lo que abona el paciente)
--  Se asocia a un turno de consultorio (idTurno)
--  o a un turno de estudio (idTurnoEstudio)
-- =====================================================

CREATE TABLE Factura (
    idFactura INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nroComprobante VARCHAR(20) UNIQUE NOT NULL,
    idPaciente INT NOT NULL,
    idTurno INT NULL,
    idTurnoEstudio INT NULL,
    fechaEmision DATETIME NOT NULL,
    concepto VARCHAR(100),
    monto DECIMAL(12,2) NOT NULL,
    medioPago VARCHAR(30),
    estado VARCHAR(20) NOT NULL DEFAULT 'Pagada',
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idTurno) REFERENCES Turno(idTurno),
    FOREIGN KEY (idTurnoEstudio) REFERENCES TurnoEstudio(idTurno)
);


-- =====================================================
--  PROCEDIMINETOS
-- =====================================================

-- CREAR USUARIO

DELIMITER //
CREATE PROCEDURE CrearUsuario(
  IN p_usuario VARCHAR(50),
  IN p_password VARCHAR(255),
  IN p_idRol INT,
  IN p_idPaciente INT,
  IN p_idProfesional INT
)
crear: BEGIN
  IF EXISTS(SELECT 1 FROM Usuario WHERE usuario = p_usuario) THEN
    SELECT 'Error: usuario ya existe' AS resultado;
    LEAVE crear;
  END IF;

  IF NOT EXISTS(SELECT 1 FROM Rol WHERE idRol = p_idRol) THEN
    SELECT 'Error: rol no existe' AS resultado;
    LEAVE crear;
  END IF;

  IF p_idPaciente IS NOT NULL THEN
    IF NOT EXISTS(SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
      SELECT 'Error: paciente no existe' AS resultado;
      LEAVE crear;
    END IF;
    IF EXISTS(SELECT 1 FROM Usuario WHERE idPaciente = p_idPaciente) THEN
      SELECT 'Error: paciente ya vinculado a otro usuario' AS resultado;
      LEAVE crear;
    END IF;
  END IF;

  IF p_idProfesional IS NOT NULL THEN
    IF NOT EXISTS(SELECT 1 FROM Profesional WHERE idProfesional = p_idProfesional) THEN
      SELECT 'Error: profesional no existe' AS resultado;
      LEAVE crear;
    END IF;
    IF EXISTS(SELECT 1 FROM Usuario WHERE idProfesional = p_idProfesional) THEN
      SELECT 'Error: profesional ya vinculado a otro usuario' AS resultado;
      LEAVE crear;
    END IF;
  END IF;

  INSERT INTO Usuario(usuario, password, idRol, idPaciente, idProfesional)
  VALUES (p_usuario, SHA2(p_password,256), p_idRol, p_idPaciente, p_idProfesional);
END crear //
DELIMITER ;

-- REGISTRAR PACIENTE EN APP

DELIMITER //
CREATE PROCEDURE RegistrarPaciente(
  IN p_nombre VARCHAR(100),
  IN p_apellido VARCHAR(100),
  IN p_dni VARCHAR(20),
  IN p_telefono VARCHAR(20),
  IN p_email VARCHAR(255),
  IN p_usuario VARCHAR(50),
  IN p_password VARCHAR(255)
)
reg: BEGIN
  DECLARE v_id INT;
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    SELECT 'Error: no se pudo registrar' AS mensaje;
  END;

  IF EXISTS (SELECT 1 FROM Paciente WHERE dni = p_dni) THEN
    SELECT 'Error: ya existe un paciente con ese DNI' AS mensaje;
    LEAVE reg;
  END IF;

  IF EXISTS (SELECT 1 FROM Usuario WHERE usuario = p_usuario) THEN
    SELECT 'Error: ese usuario ya existe' AS mensaje;
    LEAVE reg;
  END IF;

  START TRANSACTION;
  INSERT INTO Paciente (nombre, apellido, dni, telefono, email)
  VALUES (p_nombre, p_apellido, p_dni, p_telefono, p_email);
  SET v_id = LAST_INSERT_ID();

  INSERT INTO Usuario (usuario, password, idRol, idPaciente)
  VALUES (p_usuario, SHA2(p_password, 256), 1, v_id);
  COMMIT;
END reg //
DELIMITER ;

-- LOGIN

DELIMITER //
CREATE PROCEDURE IngresoLogin(
    IN p_Usu VARCHAR(50),
    IN p_Pass VARCHAR(255)
)
BEGIN
    SELECT *
    FROM Usuario
    WHERE Usuario = p_Usu
      AND Password = SHA2(p_Pass,256)
    LIMIT 1;
END //
DELIMITER ;

-- AGREGAR PACIENTE

DELIMITER //
CREATE PROCEDURE agregarPaciente(
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_dni VARCHAR(20),
    IN p_telefono VARCHAR(20),
    IN p_email VARCHAR(255)
)
BEGIN
    INSERT INTO Paciente(nombre, apellido, dni, telefono, email)
    VALUES (p_nombre, p_apellido, p_dni, p_telefono, p_email);
END //
DELIMITER ;

-- ACTUALIZAR PACIENTE

DELIMITER //
CREATE PROCEDURE ActualizarPaciente(
    IN p_idPaciente INT,
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_telefono VARCHAR(20),
    IN p_email VARCHAR(255)
)
BEGIN
    UPDATE Paciente
    SET nombre = p_nombre,
        apellido = p_apellido,
        telefono = p_telefono,
        email = p_email
    WHERE idPaciente = p_idPaciente;
END //
DELIMITER ;

-- AGENDAR TURNO

DELIMITER //
CREATE PROCEDURE AgendarTurno(
  IN p_idPaciente INT,
  IN p_idProfesional INT,
  IN p_especialidad VARCHAR(100),
  IN p_fechaHora DATETIME,
  IN p_duracion INT
)
agenda: BEGIN


  IF NOT EXISTS (SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
    SELECT 'Error: paciente no existe' AS mensaje;
    LEAVE agenda;
  END IF;


  IF NOT EXISTS (SELECT 1 FROM Profesional WHERE idProfesional = p_idProfesional) THEN
    SELECT 'Error: profesional no existe' AS mensaje;
    LEAVE agenda;
  END IF;


  IF EXISTS (
    SELECT 1 FROM Turno t
    WHERE t.idProfesional = p_idProfesional
      AND t.estado <> 'Cancelado'
      AND t.fechaHora < DATE_ADD(p_fechaHora, INTERVAL COALESCE(p_duracion,30) MINUTE)
      AND DATE_ADD(t.fechaHora, INTERVAL t.duracion MINUTE) > p_fechaHora
  ) THEN
    SELECT 'Error: profesional con turno solapado' AS mensaje;
    LEAVE agenda;
  END IF;


  INSERT INTO Turno (idPaciente, idProfesional, especialidad, fechaHora, tipoTurno, duracion, modalidadSolicitud, estado)
  VALUES (p_idPaciente, p_idProfesional, p_especialidad, p_fechaHora, 'Presencial', COALESCE(p_duracion,30), 'App', 'Pendiente');


END agenda //
DELIMITER ;

-- VER TURNOS DISPONIBLES 

DELIMITER //
CREATE PROCEDURE TurnosDisponibles(IN p_idProfesional INT, IN p_fecha DATE)
BEGIN
  WITH RECURSIVE slots AS (
    SELECT TIMESTAMP(p_fecha, a.horaDesde) AS inicio, a.duracionMinima AS dur,
           TIMESTAMP(p_fecha, a.horaHasta) AS fin, a.sobreturnosPermitidos AS sob
    FROM Agenda a
    WHERE a.idProfesional = p_idProfesional AND a.diaSemana = WEEKDAY(p_fecha) + 1
    UNION ALL
    SELECT DATE_ADD(inicio, INTERVAL dur MINUTE), dur, fin, sob FROM slots
    WHERE DATE_ADD(inicio, INTERVAL 2 * dur MINUTE) <= fin
  ),
  libres AS (
    SELECT s.inicio, s.dur FROM slots s
    WHERE s.inicio > NOW()
      AND NOT EXISTS (
        SELECT 1 FROM Turno t
        WHERE t.idProfesional = p_idProfesional AND t.estado <> 'Cancelado'
          AND t.fechaHora < DATE_ADD(s.inicio, INTERVAL s.dur MINUTE)
          AND DATE_ADD(t.fechaHora, INTERVAL t.duracion MINUTE) > s.inicio)
  ),
  horas AS (
    SELECT HOUR(inicio) AS h, MIN(inicio) AS primero, MIN(dur) AS dur, MIN(sob) AS sob
    FROM slots GROUP BY HOUR(inicio)
  )
  SELECT l.inicio AS fechaHora, l.dur AS duracion, 'Normal' AS tipo FROM libres l
  UNION ALL
  SELECT h.primero, h.dur, 'Sobreturno' FROM horas h
  WHERE h.primero > NOW()
    AND NOT EXISTS (SELECT 1 FROM libres l WHERE HOUR(l.inicio) = h.h)
    AND (SELECT COUNT(*) FROM Turno t
         WHERE t.idProfesional = p_idProfesional AND t.estado <> 'Cancelado' AND t.tipoTurno = 'Sobreturno'
           AND DATE(t.fechaHora) = p_fecha AND HOUR(t.fechaHora) = h.h) < h.sob
  ORDER BY fechaHora;
END //
DELIMITER ;

-- AGENDAR SOBRETURNO

DELIMITER //
CREATE PROCEDURE AgendarSobreturno(
  IN p_idPaciente INT,
  IN p_idProfesional INT,
  IN p_especialidad VARCHAR(100),
  IN p_fechaHora DATETIME,
  IN p_duracion INT
)
sobre: BEGIN
  DECLARE v_max INT DEFAULT 0;
  DECLARE v_cant INT DEFAULT 0;

  IF NOT EXISTS (SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
    SELECT 'Error: paciente no existe' AS mensaje;
    LEAVE sobre;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Profesional WHERE idProfesional = p_idProfesional) THEN
    SELECT 'Error: profesional no existe' AS mensaje;
    LEAVE sobre;
  END IF;

  SELECT COALESCE(MAX(sobreturnosPermitidos), 0) INTO v_max
  FROM Agenda
  WHERE idProfesional = p_idProfesional AND diaSemana = WEEKDAY(p_fechaHora) + 1;

  SELECT COUNT(*) INTO v_cant
  FROM Turno
  WHERE idProfesional = p_idProfesional AND estado <> 'Cancelado' AND tipoTurno = 'Sobreturno'
    AND DATE(fechaHora) = DATE(p_fechaHora) AND HOUR(fechaHora) = HOUR(p_fechaHora);

  IF v_cant >= v_max THEN
    SELECT 'Error: no hay sobreturnos disponibles en esa hora' AS mensaje;
    LEAVE sobre;
  END IF;

  INSERT INTO Turno (idPaciente, idProfesional, especialidad, fechaHora, tipoTurno, duracion, modalidadSolicitud, estado)
  VALUES (p_idPaciente, p_idProfesional, p_especialidad, p_fechaHora, 'Sobreturno', COALESCE(p_duracion, 30), 'App', 'Pendiente');
END sobre //
DELIMITER ;

-- TURNOS DEL PACIENTE

DELIMITER //
CREATE PROCEDURE MisTurnos(IN p_idPaciente INT)
BEGIN
  SELECT t.idTurno, t.idPaciente, t.idProfesional, t.especialidad, t.fechaHora,
         t.tipoTurno, t.duracion, t.modalidadSolicitud, t.estado,
         CONCAT(p.nombre,' ',p.apellido) AS nombreProfesional
  FROM Turno t
  JOIN Profesional p ON p.idProfesional = t.idProfesional
  WHERE t.idPaciente = p_idPaciente
  ORDER BY t.fechaHora DESC;
END //
DELIMITER ;

-- CANCELAR TURNO

DELIMITER //
CREATE PROCEDURE CancelarTurno(IN p_idTurno INT, IN p_idPaciente INT)
BEGIN
  UPDATE Turno SET estado = 'Cancelado'
  WHERE idTurno = p_idTurno AND idPaciente = p_idPaciente
    AND estado IN ('Pendiente','Confirmado') AND fechaHora > NOW();
  SELECT ROW_COUNT() AS cancelado;
END //
DELIMITER ;

-- LISTADO DE ESPECIALIDADES

DELIMITER //
CREATE PROCEDURE ListarEspecialidades()
BEGIN
  SELECT DISTINCT especialidad FROM Profesional WHERE especialidad IS NOT NULL ORDER BY especialidad;
END //

-- LISTADO DE PROFESIONALES

CREATE PROCEDURE ProfesionalesPorEspecialidad(IN p_especialidad VARCHAR(100))
BEGIN
  SELECT p.idProfesional,
         CONCAT(p.nombre,' ',p.apellido,' (',COALESCE(GROUP_CONCAT(a.disponibilidad SEPARATOR ', '),'sin agenda'),')') AS nombre
  FROM Profesional p
  LEFT JOIN Agenda a ON a.idProfesional = p.idProfesional
  WHERE p.especialidad = p_especialidad
  GROUP BY p.idProfesional, p.nombre, p.apellido
  ORDER BY p.apellido;
END //

-- FACTURAS DEL PACIENTE

CREATE PROCEDURE MisFacturas(IN p_idPaciente INT)
BEGIN
  SELECT nroComprobante, fechaEmision, concepto, monto, medioPago, estado
  FROM Factura WHERE idPaciente = p_idPaciente ORDER BY fechaEmision DESC;
END //

-- COMPROBANTES DEL PACIENTE

CREATE PROCEDURE MisComprobantesEstudio(IN p_idPaciente INT)
BEGIN
  SELECT c.fechaAtencion, e.nombre, c.comprobanteRetiro
  FROM Comprobante c
  JOIN Estudio e ON e.idEstudio = c.idEstudio
  WHERE c.idPaciente = p_idPaciente
  ORDER BY c.fechaAtencion DESC;
END //
DELIMITER ;

-- =====================================================
--  DATOS DE PRUEBA
-- =====================================================

INSERT INTO Paciente (idPaciente, nombre, apellido, dni, telefono, email, modalidadCobertura) VALUES
(1, 'Juan', 'Pérez', '12345678', '1144556677', 'juan.perez@mail.com', 'Particular'),
(2, 'Ana', 'López', '87654321', '1199887766', 'ana.lopez@mail.com', 'Obra Social'),
(3, 'Carlos', 'Martínez', '45678912', '1133224455', 'carlos.m@mail.com', 'Particular'),
(4, 'Lucía', 'Fernández', '78912345', '1177445566', 'lucia.f@mail.com', 'Obra Social'),
(5, 'Pedro', 'García', '32165498', '1166554433', 'pedro.g@mail.com', 'Particular');

INSERT INTO Profesional (idProfesional, nombre, apellido, nroMatricula, especialidad, consultorioAsignado) VALUES
(1, 'María', 'Gómez', 'MAT-001', 'Cardiología', 'Consultorio 3'),
(2, 'Jorge', 'Ramírez', 'MAT-002', 'Pediatría', 'Consultorio 1'),
(3, 'Sofía', 'Torres', 'MAT-003', 'Dermatología', 'Consultorio 5'),
(4, 'Martín', 'Suárez', 'MAT-004', 'Neurología', 'Consultorio 2'),
(5, 'Laura', 'Díaz', 'MAT-005', 'Traumatología', 'Consultorio 4');

-- Roles (uno por actor del diagrama de casos de uso)
INSERT INTO Rol (idRol, nombre) VALUES
(1, 'Paciente'),
(2, 'Recepcion'),
(3, 'Medico'),
(4, 'Administracion'),
(5, 'Guardia_Internacion'),
(6, 'Laboratorio'),
(7, 'Asistencial');

-- Pantallas del menú que ve cada rol
INSERT INTO RolPantalla (idRol, pantalla) VALUES
(1, 'Dashboard'), (1, 'Turnos'), (1, 'Comprobantes'),
(2, 'Dashboard'), (2, 'Turnos'), (2, 'Agenda'), (2, 'Pacientes'), (2, 'Comprobantes'),
(3, 'Dashboard'), (3, 'Turnos'), (3, 'Agenda'), (3, 'Pacientes'),
(4, 'Dashboard'), (4, 'Turnos'), (4, 'Agenda'), (4, 'Pacientes'), (4, 'Estudios'), (4, 'Insumos'), (4, 'Comprobantes'), (4, 'Honorarios'),
(5, 'Dashboard'), (5, 'Estudios'),
(6, 'Dashboard'), (6, 'Estudios'), (6, 'Insumos'), (6, 'Comprobantes'),
(7, 'Dashboard'), (7, 'Estudios'), (7, 'Insumos'), (7, 'Comprobantes');

-- Usuarios (contraseña en texto simple solo para pruebas; en la app real va con hash)
INSERT INTO Usuario (usuario, password, idRol, idPaciente, idProfesional) VALUES
('jperez', SHA2('1234',256), 1, 1, NULL),
('recepcion', SHA2('1234',256), 2, NULL, NULL),
('mgomez', SHA2('1234',256), 3, NULL, 1),
('agarcia', SHA2('1234',256), 4, NULL, NULL),
('guardia_internacion', SHA2('1234',256), 5, NULL, NULL),
('laboratorio', SHA2('1234',256), 6, NULL, NULL),
('asistencial', SHA2('1234',256), 7, NULL, NULL);

-- Turnos de consultorio
INSERT INTO Turno (idTurno, idPaciente, idProfesional, especialidad, fechaHora, tipoTurno, duracion, modalidadSolicitud, estado) VALUES
(1, 1, 1, 'Cardiología', '2026-10-10 09:00:00', 'Normal', 30, 'App', 'Atendido'),
(2, 2, 2, 'Pediatría', '2026-10-11 11:00:00', 'Sobreturno', 20, 'Sistema', 'Atendido'),
(3, 3, 3, 'Dermatología', '2026-10-12 15:00:00', 'Normal', 25, 'App', 'Atendido'),
(4, 4, 4, 'Neurología', '2026-10-13 10:30:00', 'Salud Mental', 40, 'Sistema', 'Cancelado'),
(5, 5, 5, 'Traumatología', '2026-10-14 08:45:00', 'Fisio-Kinesiología', 35, 'App', 'Confirmado');

-- Turnos de HOY (usan CURDATE para que el dashboard siempre tenga datos)
INSERT INTO Turno (idPaciente, idProfesional, especialidad, fechaHora, tipoTurno, duracion, modalidadSolicitud, estado) VALUES
(1, 1, 'Cardiología', TIMESTAMP(CURDATE(), '09:00:00'), 'Normal', 30, 'App', 'Atendido'),
(2, 2, 'Pediatría', TIMESTAMP(CURDATE(), '10:00:00'), 'Normal', 20, 'App', 'En espera'),
(3, 3, 'Dermatología', TIMESTAMP(CURDATE(), '10:30:00'), 'Normal', 30, 'Sistema', 'Confirmado'),
(4, 4, 'Neurología', TIMESTAMP(CURDATE(), '11:00:00'), 'Normal', 40, 'App', 'Confirmado');

INSERT INTO Estudio (idEstudio, nombre, especialidad, requisitosPrevios, horarioEspecifico, responsable) VALUES
(1, 'Ecografía abdominal', 'Diagnóstico por imágenes', 'Ayuno 8 horas', '08:00-12:00', 'Sector Imágenes'),
(2, 'Resonancia magnética', 'Diagnóstico por imágenes', 'Sin objetos metálicos', '09:00-17:00', 'Sector Resonancia'),
(3, 'Electrocardiograma', 'Cardiología', 'Sin requisitos', '08:00-14:00', 'Cardiología'),
(4, 'Análisis de sangre', 'Laboratorio', 'Ayuno 12 horas', '07:00-11:00', 'Laboratorio Central'),
(5, 'Radiografía de tórax', 'Diagnóstico por imágenes', 'Sin requisitos', '10:00-16:00', 'Sector Rayos X');

INSERT INTO TurnoEstudio (idTurno, idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno) VALUES
(1, 1, 1, '2026-10-11 10:00:00', 'App', 1),
(2, 2, 2, '2026-10-12 12:00:00', 'Sistema', 1),
(3, 3, 3, '2026-10-13 09:30:00', 'App', 1),
(4, 4, 4, '2026-10-14 08:00:00', 'Sistema', 0),
(5, 5, 5, '2026-10-15 15:00:00', 'App', 1);

INSERT INTO Agenda (idAgenda, idProfesional, especialidad, disponibilidad, diaSemana, horaDesde, horaHasta, duracionMinima, sobreturnosPermitidos, recordatorio) VALUES
(1, 1, 'Cardiología', 'Lunes 9-12', 1, '09:00', '12:00', 30, 2, 1),
(2, 2, 'Pediatría', 'Martes 10-14', 2, '10:00', '14:00', 20, 1, 1),
(3, 3, 'Dermatología', 'Miércoles 15-18', 3, '15:00', '18:00', 25, 2, 0),
(4, 4, 'Neurología', 'Jueves 9-12', 4, '09:00', '12:00', 40, 1, 1),
(5, 5, 'Traumatología', 'Viernes 8-11', 5, '08:00', '11:00', 35, 2, 0);

-- Profesionales y agendas de Fisioterapia, Kinesiología, Psicología y Psiquiatría
INSERT INTO Profesional (idProfesional, nombre, apellido, nroMatricula, especialidad, consultorioAsignado) VALUES
(6, 'Camila', 'Rojas', 'MAT-006', 'Fisioterapia', 'Consultorio 6'),
(7, 'Diego', 'Herrera', 'MAT-007', 'Kinesiología', 'Consultorio 7'),
(8, 'Valeria', 'Núñez', 'MAT-008', 'Psicología', 'Consultorio 8'),
(9, 'Andrés', 'Molina', 'MAT-009', 'Psiquiatría', 'Consultorio 9');

INSERT INTO Agenda (idAgenda, idProfesional, especialidad, disponibilidad, diaSemana, horaDesde, horaHasta, duracionMinima, sobreturnosPermitidos, recordatorio) VALUES
(6, 6, 'Fisioterapia', 'Lunes 14-18', 1, '14:00', '18:00', 30, 1, 1),
(7, 7, 'Kinesiología', 'Martes 8-12', 2, '08:00', '12:00', 30, 1, 1),
(8, 8, 'Psicología', 'Miércoles 9-13', 3, '09:00', '13:00', 45, 1, 1),
(9, 9, 'Psiquiatría', 'Jueves 14-18', 4, '14:00', '18:00', 30, 1, 1);

-- Regla: duración mínima de 30 minutos
UPDATE Agenda SET duracionMinima = 30 WHERE idAgenda IN (2, 3);
ALTER TABLE Agenda ADD CONSTRAINT chk_duracion_minima CHECK (duracionMinima >= 30);

-- Regla: 1 sobreturno por hora
UPDATE Agenda SET sobreturnosPermitidos = 1 WHERE idAgenda > 0;

INSERT INTO Atencion (idAtencion, idPaciente, idProfesional, fechaHoraFin, indicacionNuevoTurno) VALUES
(1, 1, 1, '2026-10-10 09:45:00', 1),
(2, 2, 2, '2026-10-11 11:30:00', 0),
(3, 3, 3, '2026-10-12 15:30:00', 1),
(4, 4, 4, '2026-10-13 11:15:00', 1),
(5, 5, 5, '2026-10-14 09:20:00', 0);

-- Atención de HOY
INSERT INTO Atencion (idPaciente, idProfesional, fechaHoraFin, indicacionNuevoTurno) VALUES
(1, 1, TIMESTAMP(CURDATE(), '09:40:00'), 0);

INSERT INTO Honorario (idHonorario, idProfesional, periodoLiquidacion, cantidadPacientes, monto) VALUES
(1, 1, 'Octubre 2026', 10, 100000.00),
(2, 2, 'Octubre 2026', 8, 80000.00),
(3, 3, 'Octubre 2026', 12, 120000.00),
(4, 4, 'Octubre 2026', 6, 60000.00),
(5, 5, 'Octubre 2026', 9, 90000.00);

INSERT INTO AgendaEstudio (idAgenda, sector, disponibilidad, duracionEstimada, recordatorio) VALUES
(1, 'Imágenes', 'Lunes 8-12', 30, 1),
(2, 'Resonancia', 'Martes 9-17', 45, 1),
(3, 'Cardiología', 'Miércoles 8-14', 20, 0),
(4, 'Laboratorio', 'Jueves 7-11', 15, 1),
(5, 'Rayos X', 'Viernes 10-16', 25, 0);

INSERT INTO Insumo (idInsumo, nombre, idEstudio, stock, stockMinimo, estadoReposicion) VALUES
(1, 'Gel conductor', 1, 50, 10, 'Disponible'),
(2, 'Contrastante RM', 2, 20, 25, 'Reposición'),
(3, 'Electrodos', 3, 100, 20, 'Disponible'),
(4, 'Tubos de ensayo', 4, 200, 50, 'Disponible'),
(5, 'Placas radiográficas', 5, 30, 40, 'Reposición');

INSERT INTO Comprobante (idComprobante, idPaciente, idEstudio, fechaAtencion, comprobanteRetiro) VALUES
(1, 1, 1, '2026-10-11 10:30:00', 1),
(2, 2, 2, '2026-10-12 12:30:00', 1),
(3, 3, 3, '2026-10-13 10:00:00', 0),
(4, 4, 4, '2026-10-14 08:30:00', 1),
(5, 5, 5, '2026-10-15 15:30:00', 0);

INSERT INTO Derivacion (idDerivacion, idPaciente, origen, idEstudio, estado) VALUES
(1, 1, 'Guardia Médica', 1, 'Pendiente'),
(2, 2, 'Internación', 2, 'Completada'),
(3, 3, 'Guardia Médica', 3, 'Pendiente'),
(4, 4, 'Internación', 4, 'En proceso'),
(5, 5, 'Guardia Médica', 5, 'Completada');

INSERT INTO Factura (nroComprobante, idPaciente, idTurno, idTurnoEstudio, fechaEmision, concepto, monto, medioPago, estado) VALUES
('0001-00000001', 1, 1, NULL, '2026-10-10 08:50:00', 'Consulta Cardiología', 25000.00, 'Tarjeta', 'Pagada'),
('0001-00000002', 2, 2, NULL, '2026-10-11 10:50:00', 'Consulta Pediatría', 18000.00, 'Efectivo', 'Pagada'),
('0001-00000003', 3, 3, NULL, '2026-10-12 14:50:00', 'Consulta Dermatología', 22000.00, 'Transferencia', 'Pagada'),
('0001-00000004', 1, NULL, 1, '2026-10-11 09:50:00', 'Ecografía abdominal', 30000.00, 'Tarjeta', 'Pagada'),
('0001-00000005', 4, 4, NULL, '2026-10-13 10:00:00', 'Consulta Neurología', 28000.00, 'Efectivo', 'Anulada');


-- =====================================================
--  CONSULTAS DE PRUEBA
-- =====================================================

-- Listar todos los pacientes
SELECT * FROM Paciente;

-- Buscar turnos de un paciente específico
SELECT t.idTurno, p.nombre, p.apellido, t.fechaHora, t.tipoTurno, t.estado
FROM Turno t
JOIN Paciente p ON t.idPaciente = p.idPaciente
WHERE p.dni = '12345678';

-- Ver profesionales por especialidad
SELECT nombre, apellido, especialidad
FROM Profesional;

-- Consultar estudios y sus requisitos
SELECT nombre, requisitosPrevios
FROM Estudio;

-- Listar comprobantes con paciente y estudio
SELECT c.idComprobante, p.nombre AS paciente, e.nombre AS estudio, c.fechaAtencion
FROM Comprobante c
JOIN Paciente p ON c.idPaciente = p.idPaciente
JOIN Estudio e ON c.idEstudio = e.idEstudio;

-- Login: validar usuario y contraseña, y traer el rol
SELECT u.idUsuario, u.usuario, r.nombre AS rol, u.idRol
FROM Usuario u
JOIN Rol r ON u.idRol = r.idRol
WHERE u.usuario = 'andrea' AND u.password = SHA2('1234',256);

-- Pantallas del menú según el rol (Administracion = 4)
SELECT pantalla FROM RolPantalla WHERE idRol = 4;

-- Facturas de un paciente
SELECT nroComprobante, fechaEmision, concepto, monto, medioPago, estado
FROM Factura
WHERE idPaciente = 1;

-- Dashboard: tarjetas
SELECT COUNT(*) AS turnosHoy FROM Turno WHERE DATE(fechaHora) = CURDATE();
SELECT COUNT(*) AS atendidos FROM Atencion WHERE DATE(fechaHoraFin) = CURDATE();
SELECT COUNT(*) AS enEspera FROM Turno WHERE DATE(fechaHora) = CURDATE() AND estado = 'En espera';
SELECT COUNT(*) AS insumosCriticos FROM Insumo WHERE stock <= stockMinimo;

-- Dashboard: próximos turnos de hoy
SELECT t.fechaHora, t.duracion, p.nombre AS paciente, p.dni,
       pr.nombre AS profesional, t.estado
FROM Turno t
JOIN Paciente p ON t.idPaciente = p.idPaciente
JOIN Profesional pr ON t.idProfesional = pr.idProfesional
WHERE DATE(t.fechaHora) = CURDATE()
ORDER BY t.fechaHora;

-- Dashboard: stock crítico
SELECT i.nombre, e.responsable AS sector, i.stock
FROM Insumo i
JOIN Estudio e ON i.idEstudio = e.idEstudio
WHERE i.stock <= i.stockMinimo;
