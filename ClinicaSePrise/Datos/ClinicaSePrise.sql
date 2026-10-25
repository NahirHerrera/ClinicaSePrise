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

ALTER TABLE Paciente 
ADD COLUMN sexo VARCHAR(20) AFTER dni,
ADD COLUMN fechaNacimiento DATE AFTER sexo;

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
    displayNombre VARCHAR(255) NOT NULL,
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
    responsable VARCHAR(100),
    requiereAyuno BOOLEAN NOT NULL DEFAULT 0
);

-- estado (imágenes): Pendiente -> Llamado -> Atendido (o Cancelado)
CREATE TABLE TurnoEstudio (
    idTurno INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idEstudio INT,
    fechaHora DATETIME,
    modalidadSolicitud VARCHAR(50),
    requiereTurno BOOLEAN,
    estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    duracion INT NOT NULL DEFAULT 30,
    fechaAtencion DATETIME NULL,
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

-- Qué insumos (y cuántos) consume cada práctica
CREATE TABLE EstudioInsumo (
    idEstudio INT NOT NULL,
    idInsumo INT NOT NULL,
    cantidad INT NOT NULL DEFAULT 1,
    PRIMARY KEY (idEstudio, idInsumo),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio),
    FOREIGN KEY (idInsumo) REFERENCES Insumo(idInsumo)
);

-- Laboratorio (sin turno): la orden nace cuando Recepción acredita y cobra
-- estado: Acreditada -> Llamada -> Atendida
CREATE TABLE OrdenLab (
    idOrden INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT NOT NULL,
    fechaIngreso DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    estado VARCHAR(20) NOT NULL DEFAULT 'Acreditada',
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente)
);

CREATE TABLE OrdenLabEstudio (
    idOrden INT NOT NULL,
    idEstudio INT NOT NULL,
    PRIMARY KEY (idOrden, idEstudio),
    FOREIGN KEY (idOrden) REFERENCES OrdenLab(idOrden),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio)
);

-- Comprobante de atención de estudio. Para laboratorio: uno por orden
-- (idEstudio NULL, idOrden cargado) con fechaRetiro = fecha de atención + 7 días

CREATE TABLE Comprobante (
    idComprobante INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idPaciente INT,
    idEstudio INT,
    fechaAtencion DATETIME,
    comprobanteRetiro BOOLEAN,
    idOrden INT NULL,
    fechaRetiro DATE NULL,
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idEstudio) REFERENCES Estudio(idEstudio),
    FOREIGN KEY (idOrden) REFERENCES OrdenLab(idOrden)
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
--  a un turno de estudio (idTurnoEstudio)
--  o a una orden de laboratorio (idOrdenLab)
-- =====================================================

CREATE TABLE Factura (
    idFactura INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nroComprobante VARCHAR(20) UNIQUE NOT NULL,
    idPaciente INT NOT NULL,
    idTurno INT NULL,
    idTurnoEstudio INT NULL,
    idOrdenLab INT NULL,
    fechaEmision DATETIME NOT NULL,
    concepto VARCHAR(100),
    monto DECIMAL(12,2) NOT NULL,
    medioPago VARCHAR(30),
    estado VARCHAR(20) NOT NULL DEFAULT 'Pagada',
    FOREIGN KEY (idPaciente) REFERENCES Paciente(idPaciente),
    FOREIGN KEY (idTurno) REFERENCES Turno(idTurno),
    FOREIGN KEY (idTurnoEstudio) REFERENCES TurnoEstudio(idTurno),
    FOREIGN KEY (idOrdenLab) REFERENCES OrdenLab(idOrden)
);


-- =====================================================
--  PROCEDIMIENTOS
-- =====================================================

-- CREAR USUARIO

DELIMITER //

CREATE PROCEDURE CrearUsuario(
  IN p_usuario VARCHAR(50),
  IN p_password VARCHAR(255),
  IN p_displayNombre VARCHAR(255),
  IN p_idRol INT,
  IN p_idPaciente INT,
  IN p_idProfesional INT
)
proc: BEGIN
  -- Usuario único
  IF EXISTS(SELECT 1 FROM Usuario WHERE usuario = p_usuario) THEN
    SELECT 'ERROR: usuario ya existe' AS resultado;
    LEAVE proc;
  END IF;

  -- Rol existe
  IF NOT EXISTS(SELECT 1 FROM Rol WHERE idRol = p_idRol) THEN
    SELECT 'ERROR: rol no existe' AS resultado;
    LEAVE proc;
  END IF;

  -- No permitir ambos perfiles
  IF p_idPaciente IS NOT NULL AND p_idProfesional IS NOT NULL THEN
    SELECT 'ERROR: no puede ser paciente y profesional a la vez' AS resultado;
    LEAVE proc;
  END IF;

  -- Validar paciente
  IF p_idPaciente IS NOT NULL THEN
    IF NOT EXISTS(SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
      SELECT 'ERROR: paciente no existe' AS resultado;
      LEAVE proc;
    END IF;
    IF EXISTS(SELECT 1 FROM Usuario WHERE idPaciente = p_idPaciente) THEN
      SELECT 'ERROR: paciente ya vinculado' AS resultado;
      LEAVE proc;
    END IF;
  END IF;

  -- Validar profesional
  IF p_idProfesional IS NOT NULL THEN
    IF NOT EXISTS(SELECT 1 FROM Profesional WHERE idProfesional = p_idProfesional) THEN
      SELECT 'ERROR: profesional no existe' AS resultado;
      LEAVE proc;
    END IF;
    IF EXISTS(SELECT 1 FROM Usuario WHERE idProfesional = p_idProfesional) THEN
      SELECT 'ERROR: profesional ya vinculado' AS resultado;
      LEAVE proc;
    END IF;
  END IF;


  INSERT INTO Usuario(usuario, password, displayNombre, idRol, idPaciente, idProfesional)
  VALUES (p_usuario, SHA2(p_password,256), p_displayNombre, p_idRol, p_idPaciente, p_idProfesional);

  SELECT LAST_INSERT_ID() AS nuevo_id;
END proc;
//

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

-- REGISTRAR PACIENTE EN LABORATORIO

DELIMITER //
CREATE PROCEDURE RegistrarPacienteLaboratorio(
    IN p_nombre VARCHAR(100),
    IN p_apellido VARCHAR(100),
    IN p_dni VARCHAR(20),
    IN p_sexo VARCHAR(20),
    IN p_fechaNacimiento DATE,
    IN p_modalidadCobertura VARCHAR(50),
    IN p_obraSocial VARCHAR(100)
)
reg_lab: BEGIN
    -- Validar si ya existe el paciente por DNI
    IF EXISTS (SELECT 1 FROM Paciente WHERE dni = p_dni) THEN
        SELECT 'Error: ya existe un paciente registrado con ese DNI' AS mensaje;
        LEAVE reg_lab;
    END IF;

    -- Insertar el nuevo paciente con los datos rápidos de laboratorio
    INSERT INTO Paciente (nombre, apellido, dni, sexo, fechaNacimiento, modalidadCobertura, obraSocial)
    VALUES (p_nombre, p_apellido, p_dni, p_sexo, p_fechaNacimiento, p_modalidadCobertura, p_obraSocial);
    
    -- Retornar el ID generado para poder usarlo inmediatamente en la orden
    SELECT 'OK' AS mensaje, LAST_INSERT_ID() AS idPaciente;
END //
DELIMITER ;

-- LOGIN

DELIMITER //
CREATE PROCEDURE IngresoLogin(
    IN p_Usu VARCHAR(50),
    IN p_Pass VARCHAR(255)
)
BEGIN
    SELECT 
        u.idUsuario,
        u.usuario,
        u.idRol,
        r.nombre AS rol,
        
		COALESCE(u.idPaciente, 0) AS idPaciente,
        COALESCE(u.idProfesional, 0) AS idProfesional,
        -- displayNombre: prioriza Paciente, luego Profesional, luego el displayNombre del usuario
        COALESCE(
            NULLIF(CONCAT_WS(' ', p.nombre, p.apellido), ''), 
            NULLIF(CONCAT_WS(' ', pr.nombre, pr.apellido), ''), 
            NULLIF(u.displayNombre, '')
        ) AS displayNombre
    FROM Usuario u
    JOIN Rol r ON u.idRol = r.idRol
    LEFT JOIN Paciente p ON u.idPaciente = p.idPaciente
    LEFT JOIN Profesional pr ON u.idProfesional = pr.idProfesional
    WHERE u.usuario = p_Usu
      AND u.password = SHA2(p_Pass, 256)
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
  SELECT c.fechaAtencion,
         COALESCE(e.nombre, CONCAT('Laboratorio: ',
           (SELECT GROUP_CONCAT(es.nombre SEPARATOR ', ')
            FROM OrdenLabEstudio oe
            JOIN Estudio es ON es.idEstudio = oe.idEstudio
            WHERE oe.idOrden = c.idOrden))) AS nombre,
         c.comprobanteRetiro,
         c.fechaRetiro
  FROM Comprobante c
  LEFT JOIN Estudio e ON e.idEstudio = c.idEstudio
  WHERE c.idPaciente = p_idPaciente
  ORDER BY c.fechaAtencion DESC;
END //

-- =====================================================
--  LABORATORIO (circuito B, sin turno)
-- =====================================================

-- PRÁCTICAS DE LABORATORIO (para que Recepción las elija)

CREATE PROCEDURE ListarPracticasLab()
BEGIN
  SELECT idEstudio, nombre, requiereAyuno
  FROM Estudio
  WHERE especialidad = 'Laboratorio'
  ORDER BY requiereAyuno DESC, nombre;
END //

-- LABORATORIO: BUSCAR PACIENTE POR DNI

CREATE PROCEDURE BuscarPacientePorDni(IN p_dni VARCHAR(20))
BEGIN
  SELECT idPaciente, CONCAT(nombre, ' ', apellido) AS paciente
  FROM Paciente WHERE dni = p_dni;
END //

-- LABORATORIO: INGRESA, COBRA Y REGISTRA LA ORDEN (sin turno, no pasa por Recepción)
-- p_ids = ids de prácticas separados por coma (ej. '6,17,19')

CREATE PROCEDURE RegistrarOrdenLab(
  IN p_idPaciente INT,
  IN p_ids VARCHAR(255),
  IN p_monto DECIMAL(12,2),
  IN p_medioPago VARCHAR(30)
)
orden: BEGIN
  DECLARE v_id INT;
  DECLARE v_cant INT;
  DECLARE v_ayuno INT;
  DECLARE v_nro VARCHAR(20);

  IF NOT EXISTS (SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
    SELECT 'Error: paciente no existe' AS mensaje;
    LEAVE orden;
  END IF;

  SELECT COUNT(*), COALESCE(SUM(requiereAyuno), 0) INTO v_cant, v_ayuno
  FROM Estudio
  WHERE especialidad = 'Laboratorio' AND FIND_IN_SET(idEstudio, p_ids);

  IF v_cant = 0 THEN
    SELECT 'Error: no hay prácticas seleccionadas' AS mensaje;
    LEAVE orden;
  END IF;

  IF CURTIME() < '07:00:00' OR CURTIME() > '17:00:00' THEN
    SELECT 'Error: el laboratorio atiende de 7 a 17' AS mensaje;
    LEAVE orden;
  END IF;

  IF v_ayuno > 0 AND CURTIME() > '11:00:00' THEN
    SELECT 'Error: las prácticas con ayuno se atienden de 7 a 11' AS mensaje;
    LEAVE orden;
  END IF;

  SELECT CONCAT('0001-', LPAD(COALESCE(MAX(CAST(SUBSTRING(nroComprobante, 6) AS UNSIGNED)), 0) + 1, 8, '0'))
  INTO v_nro FROM Factura;

  START TRANSACTION;
  INSERT INTO OrdenLab (idPaciente) VALUES (p_idPaciente);
  SET v_id = LAST_INSERT_ID();

  INSERT INTO OrdenLabEstudio (idOrden, idEstudio)
  SELECT v_id, idEstudio FROM Estudio
  WHERE especialidad = 'Laboratorio' AND FIND_IN_SET(idEstudio, p_ids);

  INSERT INTO Factura (nroComprobante, idPaciente, idOrdenLab, fechaEmision, concepto, monto, medioPago, estado)
  VALUES (v_nro, p_idPaciente, v_id, NOW(), 'Laboratorio', p_monto, p_medioPago, 'Pagada');
  COMMIT;

  SELECT 'OK' AS mensaje, v_id AS idOrden;
END orden //

-- LABORATORIO: LISTA DE ESPERA

CREATE PROCEDURE ListaEsperaLab()
BEGIN
  SELECT o.idOrden,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         o.estado,
         GROUP_CONCAT(e.nombre ORDER BY e.nombre SEPARATOR ', ') AS practicas
  FROM OrdenLab o
  JOIN Paciente p ON p.idPaciente = o.idPaciente
  JOIN OrdenLabEstudio oe ON oe.idOrden = o.idOrden
  JOIN Estudio e ON e.idEstudio = oe.idEstudio
  WHERE o.estado IN ('Acreditada', 'Llamada')
  GROUP BY o.idOrden, p.nombre, p.apellido, p.dni, o.estado, o.fechaIngreso
  ORDER BY o.fechaIngreso;
END //

-- LABORATORIO: LLAMAR AL PACIENTE

CREATE PROCEDURE LlamarPacienteLab(IN p_idOrden INT)
BEGIN
  UPDATE OrdenLab SET estado = 'Llamada'
  WHERE idOrden = p_idOrden AND estado IN ('Acreditada', 'Llamada');
  SELECT ROW_COUNT() AS llamado;
END //

-- COBERTURA PACIENTE (Comprobante de retiro)

DELIMITER //
CREATE PROCEDURE CoberturaPaciente(IN p_dni VARCHAR(20))
BEGIN
  SELECT IF(modalidadCobertura = 'Obra Social',
            CONCAT('Obra Social - ', COALESCE(obraSocial, 's/d')),
            COALESCE(modalidadCobertura, 'Particular')) AS cobertura
  FROM Paciente
  WHERE dni = p_dni;
END //
DELIMITER ;

-- LABORATORIO: ATENDER (descuenta insumos y emite comprobante de retiro a 7 días)

DELIMITER //
CREATE PROCEDURE AtenderOrdenLab(IN p_idOrden INT)
atender: BEGIN
  DECLARE v_pac INT;
  DECLARE v_com INT;

  SELECT idPaciente INTO v_pac FROM OrdenLab
  WHERE idOrden = p_idOrden AND estado = 'Llamada';

  IF v_pac IS NULL THEN
    SELECT 'Error: primero hay que llamar al paciente' AS mensaje;
    LEAVE atender;
  END IF;

  START TRANSACTION;

  UPDATE Insumo i
  JOIN (SELECT ei.idInsumo, SUM(ei.cantidad) AS c
        FROM OrdenLabEstudio oe
        JOIN EstudioInsumo ei ON ei.idEstudio = oe.idEstudio
        WHERE oe.idOrden = p_idOrden
        GROUP BY ei.idInsumo) x ON x.idInsumo = i.idInsumo
  SET i.stock = GREATEST(i.stock - x.c, 0);

  UPDATE Insumo
  SET estadoReposicion = IF(stock <= stockMinimo, 'Reposición', 'Disponible')
  WHERE idInsumo IN (SELECT ei.idInsumo
                     FROM OrdenLabEstudio oe
                     JOIN EstudioInsumo ei ON ei.idEstudio = oe.idEstudio
                     WHERE oe.idOrden = p_idOrden);

  INSERT INTO Comprobante (idPaciente, idEstudio, fechaAtencion, comprobanteRetiro, idOrden, fechaRetiro)
  VALUES (v_pac, NULL, NOW(), 1, p_idOrden, DATE_ADD(CURDATE(), INTERVAL 7 DAY));
  SET v_com = LAST_INSERT_ID();

  UPDATE OrdenLab SET estado = 'Atendida' WHERE idOrden = p_idOrden;
  COMMIT;

  SELECT idComprobante, fechaAtencion, fechaRetiro FROM Comprobante WHERE idComprobante = v_com;
END atender //
DELIMITER ; 

-- =====================================================
--  CONTROL DE STOCK (Laboratorio e Imágenes)
-- =====================================================

DELIMITER //
-- STOCK POR SERVICIO
-- p_especialidad: 'Laboratorio' o 'Diagnóstico por imágenes'

CREATE PROCEDURE StockInsumos(IN p_especialidad VARCHAR(100))
BEGIN
  SELECT i.idInsumo, i.nombre, i.idEstudio, i.stock, i.stockMinimo,
         IF(i.stock <= i.stockMinimo, 'Reposición', 'Disponible') AS estadoReposicion
  FROM Insumo i
  JOIN Estudio e ON e.idEstudio = i.idEstudio
  WHERE e.especialidad = p_especialidad
  ORDER BY (i.stock <= i.stockMinimo) DESC, i.nombre;
END //

-- REPONER STOCK (suma unidades y actualiza el estado)

CREATE PROCEDURE ReponerInsumo(IN p_idInsumo INT, IN p_cantidad INT)
BEGIN
  UPDATE Insumo
  SET stock = COALESCE(stock, 0) + p_cantidad,
      estadoReposicion = IF(stock <= stockMinimo, 'Reposición', 'Disponible')
  WHERE idInsumo = p_idInsumo AND p_cantidad > 0;
END //
DELIMITER ;

-- =====================================================
--  DIAGNÓSTICO POR IMÁGENES (circuito B, con turno)
--  Paciente pide turno -> Asistencial lo ve Pendiente -> lo llama -> lo atiende
--  Indicación previa = Estudio.requisitosPrevios
-- =====================================================

DELIMITER //

-- PRÁCTICAS DE IMÁGENES (para el combo del paciente)
CREATE PROCEDURE ListarPracticasImagen()
BEGIN
  SELECT idEstudio, nombre, requisitosPrevios, horarioEspecifico, responsable
  FROM Estudio
  WHERE especialidad = 'Diagnóstico por imágenes'
  ORDER BY nombre;
END //

-- TURNOS DISPONIBLES: lunes a viernes, cada 30 min, de 7 a 17
CREATE PROCEDURE TurnosDisponiblesImagen(IN p_idEstudio INT, IN p_fecha DATE)
BEGIN
  WITH RECURSIVE h AS (
    SELECT TIMESTAMP(p_fecha, '07:00:00') AS f
    UNION ALL
    SELECT f + INTERVAL 30 MINUTE FROM h WHERE f < TIMESTAMP(p_fecha, '16:30:00')
  )
  SELECT f AS fechaHora, 30 AS duracion, 'Normal' AS tipo
  FROM h
  WHERE DAYOFWEEK(p_fecha) BETWEEN 2 AND 6
    AND f > NOW()
    AND NOT EXISTS (SELECT 1 FROM TurnoEstudio t
                    WHERE t.idEstudio = p_idEstudio
                      AND t.estado <> 'Cancelado'
                      AND t.fechaHora = h.f)
  ORDER BY f;
END //

-- AGENDAR TURNO DE IMAGEN (devuelve una fila solo si hay error)
CREATE PROCEDURE AgendarTurnoImagen(IN p_idPaciente INT, IN p_idEstudio INT, IN p_fechaHora DATETIME)
agenda: BEGIN
  IF NOT EXISTS (SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
    SELECT 'Error: paciente no existe' AS mensaje;
    LEAVE agenda;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Estudio
                 WHERE idEstudio = p_idEstudio AND especialidad = 'Diagnóstico por imágenes') THEN
    SELECT 'Error: estudio no válido' AS mensaje;
    LEAVE agenda;
  END IF;

  IF EXISTS (SELECT 1 FROM TurnoEstudio
             WHERE idEstudio = p_idEstudio AND fechaHora = p_fechaHora AND estado <> 'Cancelado') THEN
    SELECT 'Error: ese horario ya fue tomado' AS mensaje;
    LEAVE agenda;
  END IF;

  INSERT INTO TurnoEstudio (idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno, estado, duracion)
  VALUES (p_idPaciente, p_idEstudio, p_fechaHora, 'App', 1, 'Pendiente', 30);
END agenda //

-- ASISTENCIAL: TURNOS PENDIENTES (con la indicación de cada estudio)
CREATE PROCEDURE ImagenesPendientes()
BEGIN
  SELECT t.idTurno,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         e.nombre AS estudio,
         t.fechaHora,
         COALESCE(e.requisitosPrevios, 'Sin requisitos') AS indicacion,
         t.estado
  FROM TurnoEstudio t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  WHERE e.especialidad = 'Diagnóstico por imágenes'
    AND t.estado IN ('Pendiente', 'Llamado')
  ORDER BY t.fechaHora;
END //

-- ASISTENCIAL: LLAMAR AL PACIENTE
CREATE PROCEDURE LlamarPacienteImagen(IN p_idTurno INT)
BEGIN
  UPDATE TurnoEstudio SET estado = 'Llamado'
  WHERE idTurno = p_idTurno AND estado IN ('Pendiente', 'Llamado');
  SELECT ROW_COUNT() AS llamado;
END //

-- ASISTENCIAL: ATENDER (descuenta insumos, comprobante de retiro a 7 días, pasa a Atendido)
CREATE PROCEDURE AtenderTurnoImagen(IN p_idTurno INT)
atender: BEGIN
  DECLARE v_pac INT;
  DECLARE v_est INT;
  DECLARE v_com INT;

  SELECT idPaciente, idEstudio INTO v_pac, v_est
  FROM TurnoEstudio WHERE idTurno = p_idTurno AND estado = 'Llamado';

  IF v_pac IS NULL THEN
    SELECT 'Error: primero hay que llamar al paciente' AS mensaje;
    LEAVE atender;
  END IF;

  START TRANSACTION;

  UPDATE Insumo i
  JOIN EstudioInsumo ei ON ei.idInsumo = i.idInsumo AND ei.idEstudio = v_est
  SET i.stock = GREATEST(i.stock - ei.cantidad, 0),
      i.estadoReposicion = IF(GREATEST(i.stock - ei.cantidad, 0) <= i.stockMinimo, 'Reposición', 'Disponible');

  INSERT INTO Comprobante (idPaciente, idEstudio, fechaAtencion, comprobanteRetiro, fechaRetiro)
  VALUES (v_pac, v_est, NOW(), 1, DATE_ADD(CURDATE(), INTERVAL 7 DAY));
  SET v_com = LAST_INSERT_ID();

  UPDATE TurnoEstudio SET estado = 'Atendido', fechaAtencion = NOW() WHERE idTurno = p_idTurno;
  COMMIT;

  SELECT c.idComprobante, CONCAT(p.nombre, ' ', p.apellido) AS paciente, e.nombre AS estudio,
         c.fechaAtencion, c.fechaRetiro
  FROM Comprobante c
  JOIN Paciente p ON p.idPaciente = c.idPaciente
  JOIN Estudio e ON e.idEstudio = c.idEstudio
  WHERE c.idComprobante = v_com;
END atender //

-- ASISTENCIAL: ESTUDIOS ATENDIDOS HOY (con su indicación)
CREATE PROCEDURE ImagenesAtendidosHoy()
BEGIN
  SELECT t.idTurno,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         e.nombre AS estudio,
         t.fechaAtencion,
         COALESCE(e.requisitosPrevios, 'Sin requisitos') AS indicacion
  FROM TurnoEstudio t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  WHERE e.especialidad = 'Diagnóstico por imágenes'
    AND t.estado = 'Atendido'
    AND DATE(t.fechaAtencion) = CURDATE()
  ORDER BY t.fechaAtencion DESC;
END //

DELIMITER ;

-- =====================================================
--  PRUEBAS
-- =====================================================

-- CALL StockInsumos('Laboratorio');
-- CALL StockInsumos('Diagnóstico por imágenes');
-- CALL ReponerInsumo(2, 10);
-- CALL ImagenesPendientes();
-- CALL TurnosDisponiblesImagen(1, CURDATE());

-- =====================================================
--  RECEPCIÓN Y ADMINISTRACIÓN
--  Estados del turno: Pendiente -> Confirmado -> En espera (acreditado) -> Atendido
--  Cancelado: lo cancela el paciente o Recepción por teléfono
--  Ausente: el paciente no vino (lo marca Recepción)
-- =====================================================

-- Faltas de los médicos (las carga Administración)
CREATE TABLE AusenciaProfesional (
    idAusencia INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    idProfesional INT NOT NULL,
    fecha DATE NOT NULL,
    motivo VARCHAR(100),
    UNIQUE (idProfesional, fecha),
    FOREIGN KEY (idProfesional) REFERENCES Profesional(idProfesional)
);

-- Detalle de cada liquidación: lo facturado y el % que se le paga al médico
ALTER TABLE Honorario
ADD COLUMN montoFacturado DECIMAL(12,2) NULL AFTER cantidadPacientes,
ADD COLUMN porcentaje DECIMAL(5,2) NULL AFTER montoFacturado;

-- Con qué cobertura se cobró cada consulta: 'Particular' u 'Obra Social - <nombre>'
ALTER TABLE Factura
ADD COLUMN cobertura VARCHAR(100) NULL AFTER medioPago;

DELIMITER //

-- RECEPCIÓN: TURNOS DE HOY

CREATE PROCEDURE RecepcionTurnosHoy()
BEGIN
  SELECT t.idTurno, t.fechaHora, t.estado,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         COALESCE(p.modalidadCobertura, 'Particular') AS cobertura,
         COALESCE(p.obraSocial, '') AS obraSocial,
         CONCAT(pr.nombre, ' ', pr.apellido) AS profesional,
         COALESCE(t.especialidad, pr.especialidad, '') AS especialidad,
         COALESCE(pr.consultorioAsignado, '') AS consultorio
  FROM Turno t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Profesional pr ON pr.idProfesional = t.idProfesional
  WHERE DATE(t.fechaHora) = CURDATE() AND t.estado <> 'Cancelado'
  ORDER BY t.fechaHora;
END //

-- RECEPCIÓN: ACREDITAR (cobra, emite la factura y pasa al paciente a la sala de espera)
-- p_obraSocialAlDia lo tilda Recepción. Al día: el paciente no abona y el valor de la consulta
-- se le factura a la obra social (medio de pago 'Obra Social'). Si no está al día, paga como particular

CREATE PROCEDURE AcreditarTurno(
  IN p_idTurno INT,
  IN p_monto DECIMAL(12,2),
  IN p_medioPago VARCHAR(30),
  IN p_obraSocialAlDia BOOLEAN
)
acreditar: BEGIN
  DECLARE v_pac INT;
  DECLARE v_estado VARCHAR(30);
  DECLARE v_concepto VARCHAR(100);
  DECLARE v_cobertura VARCHAR(100);
  DECLARE v_nro VARCHAR(20);

  SELECT t.idPaciente, t.estado, CONCAT('Consulta ', COALESCE(t.especialidad, pr.especialidad, '')),
         IF(p.modalidadCobertura = 'Obra Social' AND p_obraSocialAlDia,
            CONCAT('Obra Social - ', COALESCE(p.obraSocial, 's/d')), 'Particular')
  INTO v_pac, v_estado, v_concepto, v_cobertura
  FROM Turno t
  JOIN Profesional pr ON pr.idProfesional = t.idProfesional
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  WHERE t.idTurno = p_idTurno AND DATE(t.fechaHora) = CURDATE();

  IF v_pac IS NULL THEN
    SELECT 'Error: el turno no existe o no es de hoy' AS mensaje;
    LEAVE acreditar;
  END IF;

  IF v_estado NOT IN ('Pendiente', 'Confirmado') THEN
    SELECT CONCAT('Error: el turno está ', LOWER(v_estado), ', no se puede acreditar') AS mensaje;
    LEAVE acreditar;
  END IF;

  IF p_monto IS NULL OR p_monto < 0 OR COALESCE(p_medioPago, '') = '' THEN
    SELECT 'Error: falta el monto o el medio de pago' AS mensaje;
    LEAVE acreditar;
  END IF;

  SELECT CONCAT('0001-', LPAD(COALESCE(MAX(CAST(SUBSTRING(nroComprobante, 6) AS UNSIGNED)), 0) + 1, 8, '0'))
  INTO v_nro FROM Factura;

  START TRANSACTION;
  INSERT INTO Factura (nroComprobante, idPaciente, idTurno, fechaEmision, concepto, monto, medioPago, cobertura, estado)
  VALUES (v_nro, v_pac, p_idTurno, NOW(), v_concepto, p_monto, p_medioPago, v_cobertura, 'Pagada');
  UPDATE Turno SET estado = 'En espera' WHERE idTurno = p_idTurno;
  COMMIT;

  SELECT 'OK' AS mensaje, v_nro AS nroComprobante, v_concepto AS concepto, v_cobertura AS cobertura;
END acreditar //

-- RECEPCIÓN: EL PACIENTE NO VINO

CREATE PROCEDURE MarcarAusente(IN p_idTurno INT)
BEGIN
  UPDATE Turno SET estado = 'Ausente'
  WHERE idTurno = p_idTurno AND estado IN ('Pendiente', 'Confirmado') AND fechaHora < NOW();
  SELECT ROW_COUNT() AS marcado;
END //

-- RECEPCIÓN: TURNOS A CONFIRMAR POR TELÉFONO (pendientes de los próximos 3 días)

CREATE PROCEDURE RecepcionLlamadosPendientes()
BEGIN
  SELECT t.idTurno, t.fechaHora,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         COALESCE(p.telefono, '') AS telefono,
         CONCAT(pr.nombre, ' ', pr.apellido) AS profesional
  FROM Turno t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Profesional pr ON pr.idProfesional = t.idProfesional
  WHERE t.estado = 'Pendiente' AND t.fechaHora > NOW()
    AND t.fechaHora < DATE_ADD(CURDATE(), INTERVAL 4 DAY)
  ORDER BY t.fechaHora;
END //

-- RECEPCIÓN: RESULTADO DEL LLAMADO (el paciente confirma o cancela el turno)

CREATE PROCEDURE ResultadoLlamado(IN p_idTurno INT, IN p_confirma BOOLEAN)
BEGIN
  UPDATE Turno SET estado = IF(p_confirma, 'Confirmado', 'Cancelado')
  WHERE idTurno = p_idTurno AND estado = 'Pendiente';
  SELECT ROW_COUNT() AS cambiado;
END //

-- ADMINISTRACIÓN: LO FACTURADO POR LAS CONSULTAS DE UN MÉDICO EN UN MES
-- (facturas pagadas de sus turnos; es la base para liquidar honorarios)

CREATE FUNCTION FacturadoProfesional(p_idProfesional INT, p_anio INT, p_mes INT)
RETURNS DECIMAL(12,2)
READS SQL DATA
BEGIN
  RETURN (SELECT COALESCE(SUM(f.monto), 0)
          FROM Factura f
          JOIN Turno t ON t.idTurno = f.idTurno
          WHERE t.idProfesional = p_idProfesional AND f.estado = 'Pagada'
            AND YEAR(f.fechaEmision) = p_anio AND MONTH(f.fechaEmision) = p_mes);
END //

-- ADMINISTRACIÓN: PACIENTES ATENDIDOS POR UN MÉDICO EN UN MES

CREATE FUNCTION AtencionesProfesional(p_idProfesional INT, p_anio INT, p_mes INT)
RETURNS INT
READS SQL DATA
BEGIN
  RETURN (SELECT COUNT(*) FROM Atencion
          WHERE idProfesional = p_idProfesional
            AND YEAR(fechaHoraFin) = p_anio AND MONTH(fechaHoraFin) = p_mes);
END //

-- ADMINISTRACIÓN: LISTA DE MÉDICOS

CREATE PROCEDURE ProfesionalesListado()
BEGIN
  SELECT idProfesional,
         CONCAT(nombre, ' ', apellido, ' (', COALESCE(especialidad, 'sin especialidad'), ')') AS nombre
  FROM Profesional
  ORDER BY apellido, nombre;
END //

-- ADMINISTRACIÓN: CALCULAR HONORARIO (no guarda nada)

CREATE PROCEDURE CalcularHonorario(
  IN p_idProfesional INT,
  IN p_anio INT,
  IN p_mes INT,
  IN p_porcentaje DECIMAL(5,2),
  IN p_periodo VARCHAR(50)
)
BEGIN
  SELECT x.facturado,
         AtencionesProfesional(p_idProfesional, p_anio, p_mes) AS atenciones,
         ROUND(x.facturado * p_porcentaje / 100, 2) AS honorario,
         EXISTS (SELECT 1 FROM Honorario
                 WHERE idProfesional = p_idProfesional AND periodoLiquidacion = p_periodo) AS yaLiquidado
  FROM (SELECT FacturadoProfesional(p_idProfesional, p_anio, p_mes) AS facturado) x;
END //

-- ADMINISTRACIÓN: DETALLE DE LO QUE SE LIQUIDA A UN MÉDICO
-- Cada consulta cobrada en el mes y el estado del turno: "Atendido" lo marca el médico
-- al finalizar la consulta en su pantalla (y ahí también registra la Atención)

CREATE PROCEDURE DetalleHonorario(IN p_idProfesional INT, IN p_anio INT, IN p_mes INT)
BEGIN
  SELECT f.fechaEmision,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         f.monto,
         t.estado
  FROM Factura f
  JOIN Turno t ON t.idTurno = f.idTurno
  JOIN Paciente p ON p.idPaciente = f.idPaciente
  WHERE t.idProfesional = p_idProfesional AND f.estado = 'Pagada'
    AND YEAR(f.fechaEmision) = p_anio AND MONTH(f.fechaEmision) = p_mes
  ORDER BY f.fechaEmision;
END //

-- ADMINISTRACIÓN: REGISTRAR LA LIQUIDACIÓN DE HONORARIOS

CREATE PROCEDURE RegistrarLiquidacion(
  IN p_idProfesional INT,
  IN p_anio INT,
  IN p_mes INT,
  IN p_porcentaje DECIMAL(5,2),
  IN p_periodo VARCHAR(50)
)
liquidar: BEGIN
  DECLARE v_facturado DECIMAL(12,2);

  IF p_porcentaje IS NULL OR p_porcentaje <= 0 OR p_porcentaje > 100 THEN
    SELECT 'Error: el porcentaje tiene que estar entre 1 y 100' AS mensaje;
    LEAVE liquidar;
  END IF;

  IF EXISTS (SELECT 1 FROM Honorario
             WHERE idProfesional = p_idProfesional AND periodoLiquidacion = p_periodo) THEN
    SELECT 'Error: el médico ya tiene liquidado ese período' AS mensaje;
    LEAVE liquidar;
  END IF;

  SET v_facturado = FacturadoProfesional(p_idProfesional, p_anio, p_mes);

  IF v_facturado = 0 THEN
    SELECT 'Error: el médico no tiene consultas cobradas en ese período' AS mensaje;
    LEAVE liquidar;
  END IF;

  INSERT INTO Honorario (idProfesional, periodoLiquidacion, cantidadPacientes, montoFacturado, porcentaje, monto)
  VALUES (p_idProfesional, p_periodo, AtencionesProfesional(p_idProfesional, p_anio, p_mes),
          v_facturado, p_porcentaje, ROUND(v_facturado * p_porcentaje / 100, 2));

  SELECT 'OK' AS mensaje, LAST_INSERT_ID() AS idHonorario;
END liquidar //

-- ADMINISTRACIÓN: LIQUIDACIONES DE UN PERÍODO

CREATE PROCEDURE HonorariosLiquidados(IN p_periodo VARCHAR(50))
BEGIN
  SELECT CONCAT(pr.nombre, ' ', pr.apellido) AS profesional,
         COALESCE(h.cantidadPacientes, 0) AS pacientes,
         COALESCE(h.montoFacturado, 0) AS facturado,
         COALESCE(h.monto, 0) AS monto
  FROM Honorario h
  JOIN Profesional pr ON pr.idProfesional = h.idProfesional
  WHERE h.periodoLiquidacion = p_periodo
  ORDER BY pr.apellido, pr.nombre;
END //

-- ADMINISTRACIÓN: PACIENTES ATENDIDOS Y FACTURADO DE CADA MÉDICO EN EL MES (gráficos)

CREATE PROCEDURE AdminPorProfesional(IN p_anio INT, IN p_mes INT)
BEGIN
  SELECT CONCAT(LEFT(pr.nombre, 1), '. ', pr.apellido) AS nombre,
         AtencionesProfesional(pr.idProfesional, p_anio, p_mes) AS atendidos,
         FacturadoProfesional(pr.idProfesional, p_anio, p_mes) AS facturado
  FROM Profesional pr
  ORDER BY pr.apellido;
END //

-- ADMINISTRACIÓN: TURNOS DEL MES POR ESTADO (gráfico)

CREATE PROCEDURE AdminTurnosPorEstado(IN p_anio INT, IN p_mes INT)
BEGIN
  SELECT estado, COUNT(*) AS cantidad
  FROM Turno
  WHERE YEAR(fechaHora) = p_anio AND MONTH(fechaHora) = p_mes
  GROUP BY estado
  ORDER BY cantidad DESC;
END //

-- ADMINISTRACIÓN: REGISTRAR LA FALTA DE UN MÉDICO

CREATE PROCEDURE RegistrarAusenciaProfesional(
  IN p_idProfesional INT,
  IN p_fecha DATE,
  IN p_motivo VARCHAR(100)
)
falta: BEGIN
  IF EXISTS (SELECT 1 FROM AusenciaProfesional
             WHERE idProfesional = p_idProfesional AND fecha = p_fecha) THEN
    SELECT 'Error: esa falta ya está registrada' AS mensaje;
    LEAVE falta;
  END IF;

  INSERT INTO AusenciaProfesional (idProfesional, fecha, motivo)
  VALUES (p_idProfesional, p_fecha, NULLIF(TRIM(p_motivo), ''));
  SELECT 'OK' AS mensaje;
END falta //

-- ADMINISTRACIÓN: FALTAS DE MÉDICOS DEL MES

CREATE PROCEDURE AusenciasProfesionales(IN p_anio INT, IN p_mes INT)
BEGIN
  SELECT a.fecha,
         CONCAT(pr.nombre, ' ', pr.apellido) AS profesional,
         COALESCE(a.motivo, '') AS motivo
  FROM AusenciaProfesional a
  JOIN Profesional pr ON pr.idProfesional = a.idProfesional
  WHERE YEAR(a.fecha) = p_anio AND MONTH(a.fecha) = p_mes
  ORDER BY a.fecha DESC;
END //

DELIMITER ;

-- CONEXION TURNOS IMAGENES CON RECEPCION Y ASISTENCIAL 

DROP PROCEDURE IF EXISTS MisTurnos;
DROP PROCEDURE IF EXISTS CancelarTurnoImagen;

DELIMITER //
CREATE PROCEDURE MisTurnos(IN p_idPaciente INT)
BEGIN
  SELECT t.idTurno, t.idPaciente, t.idProfesional, t.especialidad, t.fechaHora,
         t.tipoTurno, t.duracion, t.modalidadSolicitud, t.estado,
         CONCAT(p.nombre,' ',p.apellido) AS nombreProfesional
  FROM Turno t
  JOIN Profesional p ON p.idProfesional = t.idProfesional
  WHERE t.idPaciente = p_idPaciente

  UNION ALL

  SELECT te.idTurno, te.idPaciente, 0, e.nombre, te.fechaHora,
         'Estudio', te.duracion, te.modalidadSolicitud, te.estado,
         'Servicio de Imágenes'
  FROM TurnoEstudio te
  JOIN Estudio e ON e.idEstudio = te.idEstudio
  WHERE te.idPaciente = p_idPaciente
    AND e.especialidad = 'Diagnóstico por imágenes'

  ORDER BY fechaHora DESC;
END //

CREATE PROCEDURE CancelarTurnoImagen(IN p_idTurno INT, IN p_idPaciente INT)
BEGIN
  UPDATE TurnoEstudio SET estado = 'Cancelado'
  WHERE idTurno = p_idTurno AND idPaciente = p_idPaciente
    AND estado = 'Pendiente' AND fechaHora > NOW();
  SELECT ROW_COUNT() AS cancelado;
END //
DELIMITER ;

DELETE t1 FROM TurnoEstudio t1
JOIN TurnoEstudio t2
  ON t1.idPaciente = t2.idPaciente AND t1.idEstudio = t2.idEstudio
 AND t1.fechaHora = t2.fechaHora AND t1.estado = t2.estado
 AND t1.idTurno > t2.idTurno
WHERE t1.idTurno > 0;

DELIMITER //

-- 1) El turno de imagen nace sin pagar
DROP PROCEDURE IF EXISTS AgendarTurnoImagen //
CREATE PROCEDURE AgendarTurnoImagen(IN p_idPaciente INT, IN p_idEstudio INT, IN p_fechaHora DATETIME)
agenda: BEGIN
  IF NOT EXISTS (SELECT 1 FROM Paciente WHERE idPaciente = p_idPaciente) THEN
    SELECT 'Error: paciente no existe' AS mensaje;
    LEAVE agenda;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Estudio
                 WHERE idEstudio = p_idEstudio AND especialidad = 'Diagnóstico por imágenes') THEN
    SELECT 'Error: estudio no válido' AS mensaje;
    LEAVE agenda;
  END IF;

  IF EXISTS (SELECT 1 FROM TurnoEstudio
             WHERE idEstudio = p_idEstudio AND fechaHora = p_fechaHora AND estado <> 'Cancelado') THEN
    SELECT 'Error: ese horario ya fue tomado' AS mensaje;
    LEAVE agenda;
  END IF;

  INSERT INTO TurnoEstudio (idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno, estado, duracion)
  VALUES (p_idPaciente, p_idEstudio, p_fechaHora, 'App', 1, 'Por acreditar', 30);
END agenda //

-- 2) El paciente solo cancela si todavía no pagó
DROP PROCEDURE IF EXISTS CancelarTurnoImagen //
CREATE PROCEDURE CancelarTurnoImagen(IN p_idTurno INT, IN p_idPaciente INT)
BEGIN
  UPDATE TurnoEstudio SET estado = 'Cancelado'
  WHERE idTurno = p_idTurno AND idPaciente = p_idPaciente
    AND estado = 'Por acreditar' AND fechaHora > NOW();
  SELECT ROW_COUNT() AS cancelado;
END //

-- 3) Recepción: imágenes de hoy por acreditar (mismas 10 columnas que RecepcionTurnosHoy + esImagen)
DROP PROCEDURE IF EXISTS RecepcionImagenesHoy //
CREATE PROCEDURE RecepcionImagenesHoy()
BEGIN
  SELECT t.idTurno, t.fechaHora, t.estado,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         COALESCE(p.modalidadCobertura, 'Particular') AS cobertura,
         COALESCE(p.obraSocial, '') AS obraSocial,
         e.nombre AS profesional,
         'Diagnóstico por imágenes' AS especialidad,
         'Servicio de Imágenes' AS consultorio,
         1 AS esImagen
  FROM TurnoEstudio t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  WHERE e.especialidad = 'Diagnóstico por imágenes'
    AND DATE(t.fechaHora) = CURDATE()
    AND t.estado = 'Por acreditar'
  ORDER BY t.fechaHora;
END //

-- 4) Recepción: acreditar imagen (cobra, factura y lo pasa a Pendiente para Asistencial)
DROP PROCEDURE IF EXISTS AcreditarTurnoImagen //
CREATE PROCEDURE AcreditarTurnoImagen(
  IN p_idTurno INT,
  IN p_monto DECIMAL(12,2),
  IN p_medioPago VARCHAR(30),
  IN p_obraSocialAlDia BOOLEAN
)
acreditar: BEGIN
  DECLARE v_pac INT;
  DECLARE v_estado VARCHAR(20);
  DECLARE v_concepto VARCHAR(100);
  DECLARE v_cobertura VARCHAR(100);
  DECLARE v_nro VARCHAR(20);

  SELECT t.idPaciente, t.estado, CONCAT('Estudio ', e.nombre),
         IF(p.modalidadCobertura = 'Obra Social' AND p_obraSocialAlDia,
            CONCAT('Obra Social - ', COALESCE(p.obraSocial, 's/d')), 'Particular')
  INTO v_pac, v_estado, v_concepto, v_cobertura
  FROM TurnoEstudio t
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  WHERE t.idTurno = p_idTurno AND DATE(t.fechaHora) = CURDATE();

  IF v_pac IS NULL THEN
    SELECT 'Error: el turno no existe o no es de hoy' AS mensaje;
    LEAVE acreditar;
  END IF;

  IF v_estado <> 'Por acreditar' THEN
    SELECT CONCAT('Error: el turno está ', LOWER(v_estado), ', no se puede acreditar') AS mensaje;
    LEAVE acreditar;
  END IF;

  IF p_monto IS NULL OR p_monto < 0 OR COALESCE(p_medioPago, '') = '' THEN
    SELECT 'Error: falta el monto o el medio de pago' AS mensaje;
    LEAVE acreditar;
  END IF;

  SELECT CONCAT('0001-', LPAD(COALESCE(MAX(CAST(SUBSTRING(nroComprobante, 6) AS UNSIGNED)), 0) + 1, 8, '0'))
  INTO v_nro FROM Factura;

  START TRANSACTION;
  INSERT INTO Factura (nroComprobante, idPaciente, idTurnoEstudio, fechaEmision, concepto, monto, medioPago, cobertura, estado)
  VALUES (v_nro, v_pac, p_idTurno, NOW(), v_concepto, p_monto, p_medioPago, v_cobertura, 'Pagada');
  UPDATE TurnoEstudio SET estado = 'Pendiente' WHERE idTurno = p_idTurno;
  COMMIT;

  SELECT 'OK' AS mensaje, v_nro AS nroComprobante, v_concepto AS concepto, v_cobertura AS cobertura;
END acreditar //
DELIMITER ;

-- ACREDITAR SIN RESTRICION DE DIA Y HORARIO

DELIMITER //
DROP PROCEDURE IF EXISTS RecepcionImagenesHoy //
CREATE PROCEDURE RecepcionImagenesHoy()
BEGIN
  SELECT t.idTurno, t.fechaHora, t.estado,
         CONCAT(p.nombre, ' ', p.apellido) AS paciente,
         p.dni,
         COALESCE(p.modalidadCobertura, 'Particular') AS cobertura,
         COALESCE(p.obraSocial, '') AS obraSocial,
         e.nombre AS profesional,
         'Diagnóstico por imágenes' AS especialidad,
         'Servicio de Imágenes' AS consultorio,
         1 AS esImagen
  FROM TurnoEstudio t
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  WHERE e.especialidad = 'Diagnóstico por imágenes'
    AND t.fechaHora >= CURDATE()
    AND t.estado = 'Por acreditar'
  ORDER BY t.fechaHora;
END //

DROP PROCEDURE IF EXISTS AcreditarTurnoImagen //
CREATE PROCEDURE AcreditarTurnoImagen(
  IN p_idTurno INT,
  IN p_monto DECIMAL(12,2),
  IN p_medioPago VARCHAR(30),
  IN p_obraSocialAlDia BOOLEAN
)
acreditar: BEGIN
  DECLARE v_pac INT;
  DECLARE v_estado VARCHAR(20);
  DECLARE v_concepto VARCHAR(100);
  DECLARE v_cobertura VARCHAR(100);
  DECLARE v_nro VARCHAR(20);

  SELECT t.idPaciente, t.estado, CONCAT('Estudio ', e.nombre),
         IF(p.modalidadCobertura = 'Obra Social' AND p_obraSocialAlDia,
            CONCAT('Obra Social - ', COALESCE(p.obraSocial, 's/d')), 'Particular')
  INTO v_pac, v_estado, v_concepto, v_cobertura
  FROM TurnoEstudio t
  JOIN Estudio e ON e.idEstudio = t.idEstudio
  JOIN Paciente p ON p.idPaciente = t.idPaciente
  WHERE t.idTurno = p_idTurno;

  IF v_pac IS NULL THEN
    SELECT 'Error: el turno no existe' AS mensaje;
    LEAVE acreditar;
  END IF;

  IF v_estado <> 'Por acreditar' THEN
    SELECT CONCAT('Error: el turno está ', LOWER(v_estado), ', no se puede acreditar') AS mensaje;
    LEAVE acreditar;
  END IF;

  IF p_monto IS NULL OR p_monto < 0 OR COALESCE(p_medioPago, '') = '' THEN
    SELECT 'Error: falta el monto o el medio de pago' AS mensaje;
    LEAVE acreditar;
  END IF;

  SELECT CONCAT('0001-', LPAD(COALESCE(MAX(CAST(SUBSTRING(nroComprobante, 6) AS UNSIGNED)), 0) + 1, 8, '0'))
  INTO v_nro FROM Factura;

  START TRANSACTION;
  INSERT INTO Factura (nroComprobante, idPaciente, idTurnoEstudio, fechaEmision, concepto, monto, medioPago, cobertura, estado)
  VALUES (v_nro, v_pac, p_idTurno, NOW(), v_concepto, p_monto, p_medioPago, v_cobertura, 'Pagada');
  UPDATE TurnoEstudio SET estado = 'Pendiente' WHERE idTurno = p_idTurno;
  COMMIT;

  SELECT 'OK' AS mensaje, v_nro AS nroComprobante, v_concepto AS concepto, v_cobertura AS cobertura;
END acreditar //
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
INSERT INTO Usuario (usuario, password, displayNombre, idRol, idPaciente, idProfesional) VALUES
('jperez', SHA2('1234',256), 'Juan Pérez', 1, 1, NULL),
('recepcion', SHA2('1234',256), 'Recepcion', 2, NULL, NULL),
('mgomez', SHA2('1234',256), 'María Gómez', 3, NULL, 1),
('agarcia', SHA2('1234',256), 'Andrea García', 4, NULL, NULL),
('guardia_internacion', SHA2('1234',256), 'Guardia Internacion', 5, NULL, NULL),
('laboratorio', SHA2('1234',256), 'Laboratorio', 6, NULL, NULL),
('asistencial', SHA2('1234',256), 'Asistencial', 7, NULL, NULL);

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

INSERT INTO Estudio (idEstudio, nombre, especialidad, requisitosPrevios, horarioEspecifico, responsable, requiereAyuno) VALUES
(1, 'Ecografía abdominal', 'Diagnóstico por imágenes', 'Ayuno de 8 horas', '08:00-12:00', 'Sector Imágenes', 1),
(2, 'Resonancia magnética', 'Diagnóstico por imágenes', 'Sin objetos metálicos', '09:00-17:00', 'Sector Resonancia', 0),
(3, 'Electrocardiograma', 'Cardiología', 'Sin requisitos', '08:00-14:00', 'Cardiología', 0),
(4, 'Análisis de sangre', 'Laboratorio', 'Ayuno 12 horas', '07:00-11:00', 'Laboratorio Central', 1),
(5, 'Radiografía de tórax', 'Diagnóstico por imágenes', 'Sin requisitos previos', '10:00-16:00', 'Sector Rayos X', 0),
-- Prácticas de laboratorio CON ayuno (7 a 11)
(6, 'Glucemia', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(7, 'Colesterol total', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(8, 'Colesterol HDL', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(9, 'Colesterol LDL', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(10, 'Triglicéridos', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(11, 'Urea', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(12, 'Creatinina', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(13, 'Ácido úrico', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(14, 'Hepatograma', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(15, 'Hierro sérico', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
(16, 'Ionograma', 'Laboratorio', 'Ayuno 8 horas', '07:00-11:00', 'Laboratorio Central', 1),
-- Prácticas de laboratorio SIN ayuno (7 a 17)
(17, 'Hemograma completo', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(18, 'Eritrosedimentación', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(19, 'Orina completa', 'Laboratorio', 'Primera orina de la mañana', '07:00-17:00', 'Laboratorio Central', 0),
(20, 'TSH', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(21, 'T4 libre', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(22, 'Hemoglobina glicosilada', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(23, 'Proteína C reactiva', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(24, 'Coagulograma', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(25, 'Grupo y factor', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(26, 'VDRL', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(27, 'HIV', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(28, 'Hepatitis B y C', 'Laboratorio', 'Sin requisitos', '07:00-17:00', 'Laboratorio Central', 0),
(29, 'Urocultivo', 'Laboratorio', 'Primera orina de la mañana', '07:00-17:00', 'Laboratorio Central', 0),
(30, 'Coprocultivo', 'Laboratorio', 'Frasco entregado por el laboratorio', '07:00-17:00', 'Laboratorio Central', 0);

INSERT INTO TurnoEstudio (idTurno, idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno) VALUES
(1, 1, 1, '2026-10-11 10:00:00', 'App', 1),
(2, 2, 2, '2026-10-12 12:00:00', 'Sistema', 1),
(3, 3, 3, '2026-10-13 09:30:00', 'App', 1),
(4, 4, 4, '2026-10-14 08:00:00', 'Sistema', 0),
(5, 5, 5, '2026-10-15 15:00:00', 'App', 1);
UPDATE TurnoEstudio SET estado = 'Atendido', fechaAtencion = fechaHora WHERE idTurno > 0 AND fechaHora < CURDATE();

-- Turnos de imágenes precargados (pendientes de atender)
INSERT INTO TurnoEstudio (idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno, estado, duracion) VALUES
(1, 1, TIMESTAMP(CURDATE(), '15:00:00'), 'App', 1, 'Pendiente', 30),
(2, 2, TIMESTAMP(CURDATE(), '16:00:00'), 'App', 1, 'Pendiente', 30),
(3, 5, TIMESTAMP(CURDATE() + INTERVAL 1 DAY, '09:00:00'), 'App', 1, 'Pendiente', 30);

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
(5, 'Placas radiográficas', 5, 30, 40, 'Reposición'),
(6, 'Agujas descartables', 4, 300, 50, 'Disponible'),
(7, 'Tubo tapa amarilla (suero)', 4, 250, 50, 'Disponible'),
(8, 'Tubo tapa violeta (EDTA)', 4, 150, 40, 'Disponible'),
(9, 'Tubo tapa celeste (citrato)', 4, 80, 20, 'Disponible'),
(10, 'Frasco de orina', 4, 120, 30, 'Disponible'),
(11, 'Frasco de materia fecal', 4, 60, 20, 'Disponible'),
(12, 'Torundas de alcohol', 4, 400, 100, 'Disponible');

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

-- Consumo de insumos por práctica de laboratorio
-- Extracción de sangre: 1 aguja + 1 torunda (todo menos orina, urocultivo y coprocultivo)
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad)
SELECT idEstudio, 6, 1 FROM Estudio WHERE idEstudio BETWEEN 6 AND 28;
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad)
SELECT idEstudio, 12, 1 FROM Estudio WHERE idEstudio BETWEEN 6 AND 28;
-- Tubo amarillo (química y serologías)
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad)
SELECT idEstudio, 7, 1 FROM Estudio WHERE idEstudio BETWEEN 6 AND 16 OR idEstudio IN (20, 21, 23, 26, 27, 28);
-- Tubo violeta (hemograma, eritro, HbA1c, grupo y factor)
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad)
SELECT idEstudio, 8, 1 FROM Estudio WHERE idEstudio IN (17, 18, 22, 25);
-- Tubo celeste (coagulograma)
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad) VALUES (24, 9, 1);
-- Frasco de orina / materia fecal
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad) VALUES (19, 10, 1), (29, 10, 1), (30, 11, 1);

-- Consumo de insumos por estudio de imágenes (gel, contraste, placas)
INSERT INTO EstudioInsumo (idEstudio, idInsumo, cantidad) VALUES (1, 1, 1), (2, 2, 1), (5, 5, 1);

-- Órdenes de laboratorio de ejemplo (ya acreditadas y pagas, esperando ser llamadas)
INSERT INTO OrdenLab (idOrden, idPaciente, fechaIngreso, estado) VALUES
(1, 2, NOW(), 'Acreditada'),
(2, 3, NOW(), 'Acreditada');
INSERT INTO OrdenLabEstudio (idOrden, idEstudio) VALUES (1, 6), (1, 17), (2, 19), (2, 20);

INSERT INTO Factura (nroComprobante, idPaciente, idTurno, idTurnoEstudio, fechaEmision, concepto, monto, medioPago, estado) VALUES
('0001-00000001', 1, 1, NULL, '2026-10-10 08:50:00', 'Consulta Cardiología', 25000.00, 'Tarjeta', 'Pagada'),
('0001-00000002', 2, 2, NULL, '2026-10-11 10:50:00', 'Consulta Pediatría', 18000.00, 'Efectivo', 'Pagada'),
('0001-00000003', 3, 3, NULL, '2026-10-12 14:50:00', 'Consulta Dermatología', 22000.00, 'Transferencia', 'Pagada'),
('0001-00000004', 1, NULL, 1, '2026-10-11 09:50:00', 'Ecografía abdominal', 30000.00, 'Tarjeta', 'Pagada'),
('0001-00000005', 4, 4, NULL, '2026-10-13 10:00:00', 'Consulta Neurología', 28000.00, 'Efectivo', 'Anulada');

INSERT INTO Factura (nroComprobante, idPaciente, idOrdenLab, fechaEmision, concepto, monto, medioPago, estado) VALUES
('0001-00000006', 2, 1, NOW(), 'Laboratorio', 12000.00, 'Efectivo', 'Pagada'),
('0001-00000007', 3, 2, NOW(), 'Laboratorio', 9000.00, 'Tarjeta', 'Pagada');

-- Datos de prueba de Recepción y Administración (fechas relativas a hoy)
INSERT INTO Turno (idTurno, idPaciente, idProfesional, especialidad, fechaHora, tipoTurno, duracion, modalidadSolicitud, estado) VALUES
-- Consultas de los últimos días: cobradas y atendidas
(20, 2, 1, 'Cardiología', TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 2 DAY), '09:30:00'), 'Normal', 30, 'App', 'Atendido'),
(21, 4, 3, 'Dermatología', TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 4 DAY), '15:30:00'), 'Normal', 30, 'App', 'Atendido'),
(22, 5, 2, 'Pediatría', TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 3 DAY), '11:00:00'), 'Normal', 30, 'App', 'Atendido'),
-- Pendientes de los próximos días: Recepción los confirma por teléfono
(32, 2, 4, 'Neurología', TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 1 DAY), '09:40:00'), 'Normal', 40, 'App', 'Pendiente'),
(33, 5, 1, 'Cardiología', TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 2 DAY), '10:30:00'), 'Normal', 30, 'App', 'Pendiente'),
-- Turno de hoy con María Gómez (usuario mgomez) para probar Recepción -> Médico -> Administración
(36, 3, 1, 'Cardiología', TIMESTAMP(CURDATE(), '12:00:00'), 'Normal', 30, 'App', 'Confirmado');

INSERT INTO Factura (nroComprobante, idPaciente, idTurno, fechaEmision, concepto, monto, medioPago, cobertura, estado) VALUES
('0001-00000008', 2, 20, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 2 DAY), '09:20:00'), 'Consulta Cardiología', 25000.00, 'Efectivo', 'Particular', 'Pagada'),
('0001-00000009', 4, 21, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 4 DAY), '15:20:00'), 'Consulta Dermatología', 22000.00, 'Tarjeta', 'Particular', 'Pagada'),
('0001-00000010', 5, 22, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 3 DAY), '10:50:00'), 'Consulta Pediatría', 18000.00, 'Transferencia', 'Particular', 'Pagada');

INSERT INTO Atencion (idPaciente, idProfesional, fechaHoraFin, indicacionNuevoTurno) VALUES
(2, 1, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 2 DAY), '10:00:00'), 0),
(4, 3, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 4 DAY), '16:00:00'), 0),
(5, 2, TIMESTAMP(DATE_SUB(CURDATE(), INTERVAL 3 DAY), '11:30:00'), 0);

INSERT INTO AusenciaProfesional (idProfesional, fecha, motivo) VALUES
(4, DATE_SUB(CURDATE(), INTERVAL 7 DAY), 'Licencia por enfermedad');

-- Pacientes de prueba que vienen por obra social
UPDATE Paciente SET obraSocial = 'OSDE', nroAfiliado = '61-123456-01' WHERE idPaciente = 2;
UPDATE Paciente SET obraSocial = 'IOMA', nroAfiliado = 'IO-778899' WHERE idPaciente = 4;

INSERT INTO TurnoEstudio (idPaciente, idEstudio, fechaHora, modalidadSolicitud, requiereTurno, estado, duracion) VALUES
(1, 1, TIMESTAMP(CURDATE(), '15:00:00'), 'App', 1, 'Pendiente', 30),
(2, 2, TIMESTAMP(CURDATE(), '16:00:00'), 'App', 1, 'Pendiente', 30),
(3, 5, TIMESTAMP(CURDATE() + INTERVAL 1 DAY, '09:00:00'), 'App', 1, 'Pendiente', 30);

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
WHERE u.usuario = 'agarcia' AND u.password = SHA2('1234',256);

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

DELETE FROM TurnoEstudio WHERE idTurno IN (9, 10, 11);

-- =====================================================
--  PRUEBAS DE LABORATORIO
-- =====================================================

-- Laboratorio registra una orden (ids de prácticas separados por coma; falla fuera de horario)
-- CALL RegistrarOrdenLab(1, '6,17,19', 15000.00, 'Efectivo');

-- Laboratorio
-- CALL ListaEsperaLab();
-- CALL LlamarPacienteLab(1);
-- CALL AtenderOrdenLab(1);

-- Insumos de laboratorio después de atender
-- SELECT nombre, stock, stockMinimo, estadoReposicion FROM Insumo WHERE idEstudio = 4;

-- Prueba rápida:
-- CALL AgendarTurnoImagen(1, 1, TIMESTAMP(CURDATE(), '16:30:00'));
-- CALL RecepcionImagenesHoy();
-- CALL AcreditarTurnoImagen(<idTurno>, 15000, 'Efectivo', 0);
-- CALL ImagenesPendientes();
-- CALL MisFacturas(1);
