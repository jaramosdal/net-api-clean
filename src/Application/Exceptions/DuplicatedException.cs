namespace Application.Exceptions;

public sealed class DuplicatedException(string entidad, string id) 
: Exception($"Ya existe la entidad '{entidad}' con id '{id}'.");