using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoItemController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public ProdutoItemController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // GET: api/ProdutoItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var produtos = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .ToListAsync();

            return Ok(produtos);
        }

        // GET: api/ProdutoItem/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.IditemNavigation)
                .FirstOrDefaultAsync(p => p.Iditem == id);

            if (produto == null)
            {
                return NotFound(new { mensagem = "PRODUTO NÃO LOCALIZADO!!!!!!!" });
            }

            return Ok(produto);
        }

        // POST: api/ProdutoItem
        [HttpPost]
        public async Task<ActionResult> PostProduto(ProdutoDTO produtoDTO)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var item = new Item
                {
                    Idcategoria = produtoDTO.IdCategoria,
                    Item1 = produtoDTO.Item,
                    DescricaoItem = produtoDTO.DescricaoItem,
                    Preco = produtoDTO.Preco,
                    TipoItem = produtoDTO.TipoItem,
                    AtivoProdutoServico = produtoDTO.AtivoProdutoServico
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                var produto = new Produto
                {
                    Iditem = item.Iditem,
                    CodigoPreco = produtoDTO.CodigoPreco,
                    PrecoVenda = produtoDTO.PrecoVenda ?? 0m, // Evita InvalidCastException caso o valor venha nulo
                    Unidade = produtoDTO.Unidade
                };

                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Retorna o DTO em vez da entidade para evitar loops de serialização JSON
                return CreatedAtAction(
                    nameof(GetProduto),
                    new { id = produto.Iditem },
                    produtoDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    mensagem = "ERRO NO CADASTRO",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // PUT: api/ProdutoItem/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduto(int id, ProdutoDTO dados)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.IditemNavigation)
                    .FirstOrDefaultAsync(p => p.Iditem == id);

                if (produto == null)
                {
                    return NotFound(new { mensagem = "PRODUTO NÃO LOCALIZADO!!!!!!!" });
                }

                // Atualizar os dados do item associado
                if (produto.IditemNavigation != null)
                {
                    produto.IditemNavigation.Idcategoria = dados.IdCategoria;
                    produto.IditemNavigation.Item1 = dados.Item;
                    produto.IditemNavigation.DescricaoItem = dados.DescricaoItem;
                    produto.IditemNavigation.Preco = dados.Preco;
                    produto.IditemNavigation.TipoItem = dados.TipoItem;
                    produto.IditemNavigation.AtivoProdutoServico = dados.AtivoProdutoServico;
                }

                // Atualizar os dados do produto
                produto.CodigoPreco = dados.CodigoPreco;
                produto.PrecoVenda = dados.PrecoVenda ?? 0m;
                produto.Unidade = dados.Unidade;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    mensagem = "Erro ao atualizar produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // DELETE: api/ProdutoItem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Iditem == id);

                if (produto == null)
                {
                    return NotFound(new { mensagem = "Produto não encontrado." });
                }

                // Remove os registros associados na tabela Estoque primeiro
                var estoques = await _context.Estoques.Where(e => e.Iditem == id).ToListAsync();
                if (estoques.Any())
                {
                    _context.Estoques.RemoveRange(estoques);
                    await _context.SaveChangesAsync();
                }

                // Remove o registro da tabela Produto
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                // Remove o registro da tabela Item correspondente
                var item = await _context.Items.FirstOrDefaultAsync(i => i.Iditem == id);
                if (item != null)
                {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    mensagem = "Erro ao excluir produto.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
