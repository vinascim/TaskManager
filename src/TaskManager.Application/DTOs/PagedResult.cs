namespace TaskManager.Application.DTOs;

/// <summary>
/// Página de resultados com os metadados de paginação.
/// </summary>
/// <param name="Items">Itens da página atual.</param>
/// <param name="Page" example="1">Número da página atual (começa em 1).</param>
/// <param name="PageSize" example="20">Quantidade máxima de itens por página.</param>
/// <param name="TotalCount" example="57">Total de itens que correspondem aos filtros, considerando todas as páginas.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Total de páginas disponíveis.
    /// </summary>
    /// <example>3</example>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> map) =>
        new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
