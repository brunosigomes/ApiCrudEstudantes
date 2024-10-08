using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {
            var rotasEstudantes = app.MapGroup("estudantes");

            rotasEstudantes.MapGet("", async (AppDbContext context, CancellationToken ct) =>
            {
                var estudantes = await context.Estudantes.Where(e => e.Ativo)
                    .Select(e => new EstudanteDto(e.Id, e.Nome))
                    .ToListAsync(ct);

                if (estudantes == null)
                    return Results.NotFound("Nenhum registro encontrado!");

                return Results.Ok(estudantes);
            });

            rotasEstudantes.MapGet("{id:guid}", async (Guid id, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.Where(e => e.Ativo)
                    .SingleOrDefaultAsync(e => e.Id == id, ct);

                if (estudante == null)
                    return Results.NotFound("Estudante não encontrado!");

                var estudanteDto = new EstudanteDto(estudante.Id, estudante.Nome);

                return Results.Ok(estudanteDto);
            });

            rotasEstudantes.MapPost("", async (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var jaExiste = await context.Estudantes.AnyAsync(e => e.Nome == request.Nome, ct);

                if (jaExiste)
                    return Results.Conflict("Estudante já existe!");

                var novoEstudante = new Estudante(request.Nome);
                await context.Estudantes.AddAsync(novoEstudante);
                await context.SaveChangesAsync();

                var estudanteDto = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(estudanteDto);
            });

            rotasEstudantes.MapPut("{id:guid}", async (Guid id, UpdateEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(e => e.Id == id, ct);

                if (estudante == null)
                    return Results.NotFound("Nenhum registro encontrado!");

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(ct);

                var estudanteDto = new EstudanteDto(estudante.Id, estudante.Nome);

                return Results.Ok(estudanteDto);
            });

            rotasEstudantes.MapDelete("{id:guid}", async (Guid id, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.Where(e => e.Ativo)
                    .SingleOrDefaultAsync(e => e.Id == id, ct);

                if (estudante == null)
                    return Results.NotFound("Estudante não encontrado!");

                estudante.Desativar();

                await context.SaveChangesAsync(ct);

                return Results.Ok("Estudante deletado com sucesso!");
            });
        }
    }
}
