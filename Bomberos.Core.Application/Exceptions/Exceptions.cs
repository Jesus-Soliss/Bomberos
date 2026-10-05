namespace Bomberos.Core.Application.Exceptions;

/// <summary>El recurso solicitado no existe (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Se incumple una regla de negocio (HTTP 400).</summary>
public class BusinessException(string message) : Exception(message);

/// <summary>Regla de negocio de unicidad o duplicado (HTTP 409).</summary>
public class ConflictException(string message) : BusinessException(message);
