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
        public async Task<ActionResult> Post(FuncionariosDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pDto = dto.Pessoas?.FirstOrDefault();
                if (pDto == null)
                    return BadRequest(new { mensagem = "Dados da pessoa não informados." });

                var pessoa = new Pessoa
                {
                    Nome = pDto.Nome,
                    Cpf = (long)pDto.CPF,
                    DataNascimento = pDto.DataNascimento,
                    Sexo = pDto.Sexo,
                    Email = pDto.Email,
                    Telefone = (long)pDto.Telefone,
                    AtivoPessoa = pDto.AtivoPessoa,
                    Enderecos = pDto.Enderecos?.Select(e => new Endereco
                    {
                        Cep = e.CEP,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList() ?? new List<Endereco>()
                };
                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();          // gera o Idpessoa

                var funcionario = new Funcionario
                {
                    Idpessoa = pessoa.Idpessoa,             // usa o id gerado
                    Cargo = dto.Cargo,
                    DataAdmissao = (DateOnly)dto.DataAdmissao
                };
                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetFuncionario),
                    new { id = funcionario.Idpessoa },
                    new { idPessoa = funcionario.Idpessoa, cargo = funcionario.Cargo });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensagem = "ERRO AO CADASTRAR FUNCIONÁRIO!", erro = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, FuncionariosDTO dto)
        {
            try
            {
                var funcionario = await _context.Funcionarios
                    .Include(f => f.IdpessoaNavigation)
                        .ThenInclude(p => p.Enderecos)
                    .FirstOrDefaultAsync(f => f.Idpessoa == id);

                if (funcionario == null)
                    return NotFound(new { mensagem = "FUNCIONÁRIO NÃO LOCALIZADO!" });

                var pDto = dto.Pessoas?.FirstOrDefault();
                if (pDto == null)
                    return BadRequest(new { mensagem = "Dados da pessoa não informados." });

                funcionario.Cargo = dto.Cargo;
                funcionario.DataAdmissao = (DateOnly)dto.DataAdmissao;

                var pessoa = funcionario.IdpessoaNavigation;
                pessoa.Nome = pDto.Nome;
                pessoa.Cpf = (long)pDto.CPF;
                pessoa.DataNascimento = pDto.DataNascimento;
                pessoa.Sexo = pDto.Sexo;
                pessoa.Email = pDto.Email;
                pessoa.Telefone = (long)pDto.Telefone;
                pessoa.AtivoPessoa = pDto.AtivoPessoa;

                var endDto = pDto.Enderecos?.FirstOrDefault();
                if (endDto != null)
                {
                    var end = pessoa.Enderecos.FirstOrDefault();
                    if (end == null)
                    {
                        pessoa.Enderecos.Add(new Endereco
                        {
                            Cep = endDto.CEP,
                            Numero = endDto.Numero,
                            Complemento = endDto.Complemento
                        });
                    }
                    else
                    {
                        end.Cep = endDto.CEP;
                        end.Numero = endDto.Numero;
                        end.Complemento = endDto.Complemento;
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();                 // não devolve a entidade (evita o ciclo)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "ERRO AO ATUALIZAR FUNCIONÁRIO!", erro = ex.InnerException?.Message ?? ex.Message });
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
