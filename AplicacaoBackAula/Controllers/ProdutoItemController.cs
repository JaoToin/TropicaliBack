using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;


namespace AplicacaoBackAula.Controllers
{

    //definir a rota do controller e o tipo de controller
    [Route("api/[controller]")]
    [ApiController]

    // criar o metodo construtor de classe 
    public class ProdutoItemController : ControllerBase
    {
        // criar a variavel de contexto do banco de dados
        private readonly BdAulaBackendContext _context;

        // criar o construtor da classe para injetar o contexto do banco de dados
        public ProdutoItemController(BdAulaBackendContext context)
        {
            // injetar o contexto do banco de dados
            _context = context;
        }

        //  listar todos os produtos com o relacionamento com a tabela item
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var produtos = await _context.Produtos.Include(p => p.IditemNavigation).ToListAsync();
            return Ok(produtos);
        }

        // listar um 
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.Include(produto => produto.IditemNavigation).FirstOrDefaultAsync(produto => produto.Iditem == id);
            if (produto == null)
            {
                return NotFound(new { mensagem = "PRODUTO NÃO LOCALIZADO!!!!!!!" });
            }
            return Ok(produto);
        }

        // INSERT DADOS 
        [HttpPost]
        public async Task<ActionResult> PostProduto(ProdutoDTO produtoDTO)
        {
            // iniciar a transação do banco de dados
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // criar o objeto item e preencher os dados da tabela item
                var item = new Item
                {
                    Idcategoria = produtoDTO.IdCategoria,
                    Item1 = produtoDTO.Item,
                    DescricaoItem = produtoDTO.DescricaoItem,
                    Preco = (decimal)produtoDTO.Preco,
                    TipoItem = produtoDTO.TipoItem,
                    AtivoProdutoServico = produtoDTO.AtivoProdutoServico
                };

                // adicionar o objeto item no contexto do banco de dados
                _context.Items.Add(item);

                // salvar as alterações no banco de dados
                await _context.SaveChangesAsync();

                // pegar o id do item gerado no banco de dados
                int idItemGerado = item.Iditem;

                // criar o objeto produto e preencher os dados da tabela produto
                var produto = new Produto
                {
                    Iditem = idItemGerado,
                    CodigoPreco = produtoDTO.CodigoPreco,
                    PrecoVenda = (decimal)produtoDTO.PrecoVenda,
                    Unidade = produtoDTO.Unidade
                };

                // adicionar o objeto produto no contexto do banco de dados
                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // retornar o status code 201 (Created) com o objeto produto criado
                return CreatedAtAction(
                    nameof(GetProduto),
                    new { id = produto.Iditem },
                    produto);
            }
            catch (Exception ex)
            {
                // se der erro na transação, fazer o rollback
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "ERRO NO CADASTRO", erro = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduto(int id, ProdutoDTO dados)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var produto = await _context.Produtos.Include(p => p.IditemNavigation).FirstOrDefaultAsync(p => p.Iditem == id);
                if (produto == null)
                {
                    return NotFound(new { mensagem = "PRODUTO NÃO LOCALIZADO!!!!!!!" });
                }

                // atualizar os dados do item
                produto.IditemNavigation.Idcategoria = dados.IdCategoria;
                produto.IditemNavigation.Item1 = dados.Item;
                produto.IditemNavigation.DescricaoItem = dados.DescricaoItem;
                produto.IditemNavigation.Preco = (decimal)dados.Preco;
                produto.IditemNavigation.TipoItem = dados.TipoItem;
                produto.IditemNavigation.AtivoProdutoServico = dados.AtivoProdutoServico;

                // atualizar os dados do produto
                produto.CodigoPreco = dados.CodigoPreco;
                produto.PrecoVenda = (decimal)dados.PrecoVenda;
                produto.Unidade = dados.Unidade;

                // salvar as alterações no banco de dados
                await _context.SaveChangesAsync();

                // COMMIT obrigatório para persistir a transação
                await transaction.CommitAsync();
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
            return NoContent();
        }

        //EXCLUIR DADO NAS TABELAS ESTOQUE, PRODUTO E ITEM
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

                // Busca os registros de estoque do produto
                var estoques = await _context.Estoques.Where(e => e.Iditem == id).ToListAsync();

                // Remove os registros de estoque primeiro
                if (estoques.Any())
                {
                    _context.Estoques.RemoveRange(estoques);
                    await _context.SaveChangesAsync();
                }

                // Remove o produto
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                // Busca e remove o item correspondente
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