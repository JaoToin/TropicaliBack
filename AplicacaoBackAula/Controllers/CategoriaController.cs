using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //definir a rota do controller e o tipo de controller
    public class CategoriaController : ControllerBase
    {
        // criar a variavel de contexto do banco de dados
        private readonly BdAulaBackendContext _context;

        // criar o construtor da classe para injetar o contexto do banco de dados
        public CategoriaController(BdAulaBackendContext context)
        {
            // injetar o contexto do banco de dados
            _context = context;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAll()
        {
            var categorias = await _context.Categoria.Select(c => new CategoriaDTO
            {
                
                Categoria = c.Categoria,
                Produtos = c.Items.Select(p => new ProdutoDTO
                {
                  IdCategoria = p.Idcategoria,
                   Item = p.Item1,
                    DescricaoItem = p.DescricaoItem,
                    Preco = p.Preco,
                    TipoItem = p.TipoItem,
                    AtivoProdutoServico = p.AtivoProdutoServico
                }).ToList()
            }).ToListAsync();
            return Ok(categorias);
        } // fim metodo getAll

        [HttpGet("{id}")]

        public async Task<ActionResult<CategoriaDTO>> GetCategoria(int id)
        {
            var categoria = await _context.Categoria.Where(c => c.Idcategoria == id)            
                .Select(c => new CategoriaDTO
                {
                    Categoria = c.Categoria,
                    Produtos = c.Items.Select(p => new ProdutoDTO
                    {
                        IdCategoria = p.Idcategoria,
                        Item = p.Item1,
                        DescricaoItem = p.DescricaoItem,
                        Preco = p.Preco,
                        TipoItem = p.TipoItem,
                        AtivoProdutoServico = p.AtivoProdutoServico
                    }).ToList()

                }).ToListAsync();       
            return Ok(categoria);
        } // fim metodo getCategoria

        [HttpPost]

        public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO dto)
        {
            var categoria = new Categorium
            {
                Categoria = dto.Categoria
            };
            _context.Categoria.Add(categoria);
            await _context.SaveChangesAsync();

            var resposta = new CategoriaDTO
            {
                Categoria = categoria.Categoria,
                Produtos = new List<ProdutoDTO>()
            };

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Idcategoria }, resposta);
        } // fim metodo createCategoria

        [HttpPut("{id}")]

        public async Task<ActionResult> Put(int id, CategoriaDTO dto)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            categoria.Categoria = dto.Categoria;
            await _context.SaveChangesAsync();
            return NoContent();
        } // fim metodo updateCategoria

        [HttpDelete("{id}")]

        public async Task<ActionResult<Categorium>> Delete(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        } // fim metodo deleteCategoria
    }
}
