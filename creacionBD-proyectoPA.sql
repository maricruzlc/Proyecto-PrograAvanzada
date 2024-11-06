create database ProyectoPA;
GO

USE ProyectoPA;
GO

create table roles(
	IdRol int primary key, 
	Nombre nvarchar(100),
	Descripcion nvarchar(200));

create table estadoUsuario(
	IdEstado int primary key,
	nombreEstado nvarchar(100));

create table usuarios(
	IdUsuario int primary key, 
	IdRol int foreign key references roles(IdRol),
	IdEstado int foreign key references estadoUsuario(IdEstado),
	Correo nvarchar(100),
	FechaCreacion date);

create table estadoSala(
	IdEstado int primary key,
	nombreEstado nvarchar(100));

create table salas(
	IdSala int primary key,
	IdEstado int foreign key references estadoSala(IdEstado),
	nombre nvarchar(100),
	capacidad int,
	ubicacion nvarchar(200));

create table historialUso(
	IdHistorial int primary key,
	IdSala int foreign key references salas(IdSala),
	Fecha date,
	reservasRealizadas int,
	totalHoras int);

create table estadoReserva(
	IdEstado int primary key,
	nombreEstado nvarchar(100));

create table reservas(
	IdReserva int primary key,
	IdUsuario int foreign key references usuarios(IdUsuario),
	IdSala int foreign key references salas(IdSala),
	IdEstado int foreign key references estadoReserva(IdEstado),
	FechaInicio time, 
	FechaFin time, 
	Detalle nvarchar(100));

GO

