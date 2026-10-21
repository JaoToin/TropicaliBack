using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AplicacaoBackAula.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public FuncionariosController(BdAulaBackendContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionariosDTO>>> GetFuncionarios()
        {
            var funcionarios = await _context.Funcionarios.Select(f => new FuncionariosDTO
            {
                IdPessoa = f.Idpessoa,
                Cargo = f.Cargo,
                DataAdmissao = f.DataAdmissao,

                Pessoas = _context.Pessoas.Where(p => p.Idpessoa == f.Idpessoa).Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long?)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = (long?)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = _context.Enderecos.Where(e => e.Idpessoa == p.Idpessoa).Select(e => new EnderecoDTO
                    {
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }).ToList()
            }).ToListAsync();

            return Ok(funcionarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FuncionariosDTO>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.Where(f => f.Idpessoa == id).Select(f => new FuncionariosDTO
            {
                IdPessoa = f.Idpessoa,
                Cargo = f.Cargo,
                DataAdmissao = f.DataAdmissao,

                Pessoas = _context.Pessoas.Where(p => p.Idpessoa == f.Idpessoa).Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long?)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = (long?)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = _context.Enderecos.Where(e => e.Idpessoa == p.Idpessoa).Select(e => new EnderecoDTO
                    {
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }).ToList()
            }).FirstOrDefaultAsync();

            if (funcionario == null)
            {
                return NotFound(new { mensagem = "FUNCIONÁRIO NÃO LOCALIZADO!!!!!!!" });
            }

            return Ok(funcionario);
        }

        [HttpPost]
        public async Task<ActionResult<FuncionariosDTO>> Post(FuncionariosDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
               
                var funcionario = new Funcionario
                {
                    Idpessoa = dto.IdPessoa,
                    Cargo = dto.Cargo,
                    DataAdmissao = (DateOnly)dto.DataAdmissao
                                    
                };
                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync();

                var pessoa = new Pessoa
                {
                    Idpessoa = dto.IdPessoa,
                    Nome = dto.Pessoas?.FirstOrDefault()?.Nome,
                    Cpf = (long)dto.Pessoas?.FirstOrDefault()?.CPF,
                    DataNascimento = (DateOnly)dto.Pessoas?.FirstOrDefault()?.DataNascimento,
                    Sexo = dto.Pessoas?.FirstOrDefault()?.Sexo,
                    Email = dto.Pessoas?.FirstOrDefault()?.Email,
                    Telefone = (long)dto.Pessoas?.FirstOrDefault()?.Telefone,
                    AtivoPessoa = dto.Pessoas?.FirstOrDefault()?.AtivoPessoa,
                    Enderecos = dto.Pessoas?.FirstOrDefault()?.Enderecos?.Select(e => new Endereco
                    {
                        Idpessoa = e.IdPessoa,
                        Cep = e.CEP,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(funcionario);
                
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "ERRO AO CADASTRAR FUNCIONÁRIO!", erro = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FuncionariosDTO>> Put(int id, FuncionariosDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var funcionario = await _context.Funcionarios.FindAsync(id);
                if (funcionario == null)
                {
                    return NotFound(new { mensagem = "FUNCIONÁRIO NÃO LOCALIZADO!!!!!!!" });
                }

                funcionario.Idpessoa = dto.IdPessoa;
                funcionario.Cargo = dto.Cargo;
                funcionario.DataAdmissao = (DateOnly)dto.DataAdmissao;

                funcionario.IdpessoaNavigation.Nome = dto.Pessoas?.FirstOrDefault()?.Nome;
                funcionario.IdpessoaNavigation.Cpf = (long)dto.Pessoas?.FirstOrDefault()?.CPF;
                funcionario.IdpessoaNavigation.DataNascimento = (DateOnly)(dto.Pessoas?.FirstOrDefault()?.DataNascimento);
                funcionario.IdpessoaNavigation.Sexo = dto.Pessoas?.FirstOrDefault()?.Sexo;
                funcionario.IdpessoaNavigation.Email = dto.Pessoas?.FirstOrDefault()?.Email;
                funcionario.IdpessoaNavigation.Telefone = (long)dto.Pessoas?.FirstOrDefault()?.Telefone;
                funcionario.IdpessoaNavigation.AtivoPessoa = dto.Pessoas?.FirstOrDefault()?.AtivoPessoa;

                funcionario.IdpessoaNavigation.Enderecos = dto.Pessoas?.FirstOrDefault()?.Enderecos?.Select(e => new Endereco
                {
                    Idpessoa = e.IdPessoa,
                    Cep = e.CEP,
                    Numero = e.Numero,
                    Complemento = e.Complemento
                }).ToList();

                _context.Entry(funcionario).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(funcionario);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "ERRO AO ATUALIZAR FUNCIONÁRIO!", erro = ex.Message });
            }
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult<Funcionario>> Delete(int id)
        {
            var funcionario = await _context.Funcionarios.Include(f => f.IdpessoaNavigation).FirstOrDefaultAsync(f => f.Idpessoa == id);
            if (funcionario == null)
            {
                return NotFound(new { mendagem = "Funcionario não encontrado!!!!!!!!!!!!!!!!"});
            }
            
            _context.Funcionarios.Remove(funcionario);

            if (funcionario.IdpessoaNavigation != null)
            {
                _context.Pessoas.Remove(funcionario.IdpessoaNavigation);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}