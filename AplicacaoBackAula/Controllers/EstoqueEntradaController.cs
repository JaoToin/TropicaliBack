using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueEntradaController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public EstoqueEntradaController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTA OS ESTOQUES (um por produto), cada um com as suas entradas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueDTO>>> GetAll()
        {
            var estoques = await _context.Estoques.Select(e => new EstoqueDTO
            {
                IdItem = e.Iditem,
                Quantidade = e.QuantidadeEstoque,

                Entradas = e.EntradaProdutos.Select(en => new EntradaProdutoDTO
                {
                    IdEntrada = en.IdentradaProduto,
                    IdItem = en.Iditem,
                    DataEntrada = en.DataEntradaProduto,
                    QuantidadeEntrada = en.QuantidadeEntrada,
                    Unidade = en.Unidade,
                    PrecoUnitario = (decimal)en.ValorUnitarioEntrada
                }).ToList()
            }).ToListAsync();

            return Ok(estoques);
        }

        // ESTOQUE DE UM PRODUTO (id = id do item) COM AS ENTRADAS
        [HttpGet("{id}")]
        public async Task<ActionResult<EstoqueDTO>> GetEntrada(int id)
        {
            var estoque = await _context.Estoques
                .Where(e => e.Iditem == id)
                .Select(e => new EstoqueDTO
                {
                    IdItem = e.Iditem,
                    Quantidade = e.QuantidadeEstoque,
                    Entradas = e.EntradaProdutos.Select(en => new EntradaProdutoDTO
                    {
                        IdEntrada = en.IdentradaProduto,
                        IdItem = en.Iditem,
                        DataEntrada = en.DataEntradaProduto,
                        QuantidadeEntrada = en.QuantidadeEntrada,
                        Unidade = en.Unidade,
                        PrecoUnitario = (decimal)en.ValorUnitarioEntrada
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (estoque == null)
            {
                return NotFound(new { mensagem = "Estoque do produto não encontrado!" });
            }

            return Ok(estoque);
        }

        // CADASTRAR ENTRADA (cria o registro de estoque do produto se ainda não existir)
        [HttpPost]
        public async Task<ActionResult> Post(EntradaProdutoDTO dto)
        {
            try
            {
                var produtoExiste = await _context.Produtos.AnyAsync(p => p.Iditem == dto.IdItem);
                if (!produtoExiste)
                {
                    return BadRequest(new { mensagem = "Produto não encontrado. Informe o ID de um produto cadastrado." });
                }

                var estoqueExiste = await _context.Estoques.AnyAsync(e => e.Iditem == dto.IdItem);
                if (!estoqueExiste)
                {
                    _context.Estoques.Add(new Estoque
                    {
                        Iditem = dto.IdItem,
                        QuantidadeEstoque = 0
                    });
                }

                var entrada = new EntradaProduto
                {
                    Iditem = dto.IdItem,
                    DataEntradaProduto = dto.DataEntrada,
                    QuantidadeEntrada = dto.QuantidadeEntrada,
                    Unidade = dto.Unidade,
                    ValorUnitarioEntrada = dto.PrecoUnitario
                };

                _context.EntradaProdutos.Add(entrada);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEntrada),
                    new { id = entrada.Iditem },
                    new { idEntrada = entrada.IdentradaProduto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao criar entrada de produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ALTERAR ENTRADA (id = id da ENTRADA)
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, EntradaProdutoDTO dto)
        {
            try
            {
                var entrada = await _context.EntradaProdutos
                    .FirstOrDefaultAsync(e => e.IdentradaProduto == id);

                if (entrada == null)
                {
                    return NotFound(new { mensagem = "Entrada não localizada!" });
                }

                entrada.DataEntradaProduto = dto.DataEntrada;
                entrada.QuantidadeEntrada = dto.QuantidadeEntrada;
                entrada.Unidade = dto.Unidade;
                entrada.ValorUnitarioEntrada = dto.PrecoUnitario;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao atualizar entrada de produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // EXCLUIR ENTRADA (id = id da ENTRADA). Não apaga o produto.
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var entrada = await _context.EntradaProdutos
                    .FirstOrDefaultAsync(e => e.IdentradaProduto == id);

                if (entrada == null)
                {
                    return NotFound(new { mensagem = "Entrada não encontrada." });
                }

                _context.EntradaProdutos.Remove(entrada);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao excluir entrada de produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
