namespace Application.Exceptions;

public sealed class NotFoundException(string entidad, string id) 
: Exception($"No se ha encontrado la entidad '{entidad}' con id '{id}'.");
