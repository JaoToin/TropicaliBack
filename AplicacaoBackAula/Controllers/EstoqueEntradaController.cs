using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //definir a rota do controller e o tipo de controller
    public class EstoqueEntradaController : ControllerBase
    {
        // criar a variavel de contexto do banco de dados
        private readonly BdAulaBackendContext _context;

        // criar o construtor da classe para injetar o contexto do banco de dados
        public EstoqueEntradaController(BdAulaBackendContext context)
        {
            // injetar o contexto do banco de dados
            _context = context;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<EntradaProdutoDTO>>> GetAll()
        {
            var entradas = await _context.Estoques.Select(e => new EstoqueDTO
            {
                IdItem = e.Iditem,
                Quantidade = e.QuantidadeEstoque,

                Entradas = e.EntradaProdutos.Select(en => new EntradaProdutoDTO
                {
                    IdItem = en.Iditem,
                    DataEntrada = en.DataEntradaProduto,
                    QuantidadeEntrada = en.QuantidadeEntrada,
                    Unidade = en.Unidade,
                    PrecoUnitario = (decimal)en.ValorUnitarioEntrada

                }).ToList()

            }).ToListAsync();
            return Ok(entradas);
        } // fim metodo getAll

        [HttpGet("{id}")]

        public async Task<ActionResult<EstoqueDTO>> GetEntrada(int id)
        {
            var entrada = await _context.Estoques.Where(e => e.Iditem == id)
                .Select(e => new EstoqueDTO
                {
                    IdItem = e.Iditem,
                    Quantidade = e.QuantidadeEstoque,
                    Entradas = e.EntradaProdutos.Select(en => new EntradaProdutoDTO
                    {
                        IdItem = en.Iditem,
                        DataEntrada = en.DataEntradaProduto,
                        QuantidadeEntrada = en.QuantidadeEntrada,
                        Unidade = en.Unidade,
                        PrecoUnitario = (decimal)en.ValorUnitarioEntrada
                    }).ToList()
                }).FirstOrDefaultAsync( entrada => entrada.IdItem == id);
            if (entrada == null)
            {
                return NotFound(new { mensagem = "Produto Não Encontrado !!!!!!!!!!!!!!!!!!!!!!!!!!!!!"});
            }
            return Ok(entrada);
        } // fim metodo getEntrada

        [HttpPost]

        public async Task<ActionResult<EntradaProdutoDTO>> Post(EntradaProdutoDTO dto)
        {


            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entrada = new EntradaProduto
                {
                    Iditem = dto.IdItem,
                    DataEntradaProduto = dto.DataEntrada,
                    QuantidadeEntrada = dto.QuantidadeEntrada,
                    Unidade = dto.Unidade,
                    ValorUnitarioEntrada = (decimal)dto.PrecoUnitario
                };
                _context.EntradaProdutos.Add(entrada);
                await _context.SaveChangesAsync();


                var resposta = new EntradaProdutoDTO
                {

                    IdItem = entrada.Iditem,
                    DataEntrada = entrada.DataEntradaProduto,
                    QuantidadeEntrada = entrada.QuantidadeEntrada,
                    Unidade = entrada.Unidade,
                    PrecoUnitario = (decimal)entrada.ValorUnitarioEntrada

                };
                return CreatedAtAction(nameof(GetEntrada), new { id = entrada.Iditem }, resposta);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "Erro ao criar entrada de produto: ", erro = ex.Message });
            } // fim metodo createEntrada
        }

        [HttpPut("{id}")]

        public async Task<ActionResult<EntradaProdutoDTO>> Put(int id, EntradaProdutoDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();


            try
            {
                var entrada = await _context.EntradaProdutos.FindAsync(id);
                //   var entrada = await _context.Estoques.Include(e => e.EntradaProdutos).FirstOrDefaultAsync(i => i.Iditem == id);

                if (entrada == null)
                {
                    return NotFound(new {mensagem = "Produto Não Localizado !!!!!!!!!!!!!!!!!!!!!!!!"});
                }
                entrada.DataEntradaProduto = dto.DataEntrada;
                entrada.QuantidadeEntrada = dto.QuantidadeEntrada;
                entrada.Unidade = dto.Unidade;
                entrada.ValorUnitarioEntrada = (decimal)dto.PrecoUnitario;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var resposta = new EntradaProdutoDTO
                {
                    IdItem = entrada.Iditem,
                    DataEntrada = entrada.DataEntradaProduto,
                    QuantidadeEntrada = entrada.QuantidadeEntrada,
                    Unidade = entrada.Unidade,
                    PrecoUnitario = (decimal)entrada.ValorUnitarioEntrada
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "Erro ao atualizar entrada de produto: ", erro = ex.Message });
            }
            return NoContent();
        } // fim metodo updateEntrada

        [HttpDelete("{id}")]

        public async Task<ActionResult> Delete(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entrada = await _context.EntradaProdutos.FirstOrDefaultAsync(p => p.Iditem == id);
                if (entrada == null)
                {
                    return NotFound(new { mensagem = "Produto não encontrado." });
                }
                _context.EntradaProdutos.Remove(entrada);
                await _context.SaveChangesAsync();

                var produtos = await _context.Produtos.Where(e => e.Iditem == id).ToListAsync();

                // Remove os registros de estoque primeiro
                if (produtos.Any())
                {
                    _context.Produtos.RemoveRange(produtos);
                    await _context.SaveChangesAsync();
                }




                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            { 
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "Erro ao deletar entrada de produto: ", erro = ex.Message });
            }
           
        } // fim metodo deleteEntrada








    }
}
