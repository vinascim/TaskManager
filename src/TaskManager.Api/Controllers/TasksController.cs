using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Gerenciamento de tarefas.
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <remarks>
    /// Apenas o título é obrigatório. Se o status não for informado, a tarefa é criada como Pending.
    /// </remarks>
    /// <response code="201">Tarefa criada. O header Location aponta para o novo recurso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Lista as tarefas de forma paginada, com filtros opcionais.
    /// </summary>
    /// <remarks>
    /// Os filtros podem ser combinados. Tarefas com vencimento aparecem primeiro, ordenadas pela data.
    /// Por padrão retorna a página 1 com 20 itens; o tamanho máximo da página é 100.
    /// </remarks>
    /// <response code="200">Página de tarefas com os metadados de paginação (itens vazios se nada corresponder aos filtros).</response>
    /// <response code="400">Filtros ou parâmetros de paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<TaskResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TaskResponse>>> List([FromQuery] TaskFilter filter, CancellationToken cancellationToken)
    {
        var tasks = await _taskService.ListAsync(filter, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Busca uma tarefa pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da tarefa.</param>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Atualiza uma tarefa existente.
    /// </summary>
    /// <remarks>
    /// Substitui todos os campos da tarefa. Campos opcionais enviados como null são removidos.
    /// </remarks>
    /// <param name="id">Identificador da tarefa.</param>
    /// <param name="request">Novos dados da tarefa.</param>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Exclui uma tarefa.
    /// </summary>
    /// <param name="id">Identificador da tarefa.</param>
    /// <response code="204">Tarefa excluída.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}