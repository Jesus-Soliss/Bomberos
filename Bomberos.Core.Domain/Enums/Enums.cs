namespace Bomberos.Core.Domain.Enums;

public enum EstadoEstacion { Activa, Inactiva }

public enum EstadoBombero { Activo, Inactivo }

public enum RangoBombero { Bombero, Cabo, Sargento, Teniente, Capitan }

public enum TipoVehiculo { Camion, Ambulancia, Rescate, Tanquero }

public enum EstadoVehiculo { Disponible, EnServicio, Mantenimiento, FueraDeServicio }

public enum TipoEmergencia { Incendio, Accidente, Rescate, IncidentePeligroso, Otro }

public enum PrioridadEmergencia { Baja, Media, Alta, Critica }

public enum EstadoEmergencia { Pendiente, EnAtencion, Controlada, Finalizada, Cancelada }

public enum OrigenEmergencia { Operador, Central911 }

public enum EstadoAsignacion { Activa, Finalizada, Cancelada }

public enum ResultadoAuditoria { Exitoso, Fallido }
