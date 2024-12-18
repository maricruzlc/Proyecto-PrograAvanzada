create database ProyectoPA;
GO

USE ProyectoPA;
GO

create table roles(
	IdRol int IDENTITY(1,1) primary key, 
	Nombre varchar(100));

create table usuarios(
	IdUsuario int IDENTITY(1,1) primary key, 
	IdRol int foreign key references roles(IdRol),
	Usuario varchar(100),
	contrasena varchar(100),
	Nombre varchar(100),
	Apellidos varchar(100),
	Correo varchar(100),
	FechaCreacion date);


create table salas(
	IdSala int IDENTITY(1,1) primary key,
	nombre varchar(100),
	capacidad int,
	ubicacion varchar(200),
	hora_inicio_dispo time,
	hora_fin_dispo time);

create table estadoSala(
	IdEstado int IDENTITY(1,1) primary key,
	IdSala int foreign key references salas(IdSala),
	nombreEstado varchar(100),
	fecha date,
	hora_inicio time,
	hora_fin time);

create table equipo(
	Id_Equipo int IDENTITY(1,1) primary key, 
	nombre_equipo varchar(100));

create table equipo_salas(
	Id_equipo_salas int IDENTITY(1,1) primary key,
	Id_Equipo int foreign key references equipo(Id_Equipo),
	IdSala int foreign key references salas(IdSala));

create table reservas(
	IdReserva int IDENTITY(1,1) primary key,
	IdUsuario int foreign key references usuarios(IdUsuario),
	IdSala int foreign key references salas(IdSala),
	fecha date,
	hora_inicio time, 
	hora_fin time, 
	detalle varchar(500));


GO

INSERT INTO roles VALUES('Administrador');
INSERT INTO roles VALUES('Usuario');
Go
INSERT INTO equipo VALUES('Pizarra');
INSERT INTO equipo VALUES('Proyector');
Go

Select * from roles;
Select * from usuarios;
Select * from equipo_salas;
