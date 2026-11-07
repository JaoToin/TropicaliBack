using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;

namespace AplicacaoBackAula.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PessoaEnderecoController : ControllerBase
    {
        private readonly BdAulaBackendContext _context;

        public PessoaEnderecoController(BdAulaBackendContext context)
        {
            _context = context;
        }

        // LISTAR TODAS AS PESSOAS E SEUS ENDEREÇOS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaDTO>>> GetAll()
        {
            var pessoas = await _context.Pessoas
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = (long)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdEndereco = e.Idendereco,
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }).ToListAsync();

            return Ok(pessoas);
        }

        // LISTAR UMA PESSOA (devolve UM objeto, e não uma lista)
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaDTO>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoas
                .Where(p => p.Idpessoa == id)
                .Select(p => new PessoaDTO
                {
                    IdPessoa = p.Idpessoa,
                    Nome = p.Nome,
                    CPF = (long)p.Cpf,
                    DataNascimento = p.DataNascimento,
                    Sexo = p.Sexo,
                    Email = p.Email,
                    Telefone = (long)p.Telefone,
                    AtivoPessoa = p.AtivoPessoa,

                    Enderecos = p.Enderecos.Select(e => new EnderecoDTO
                    {
                        IdEndereco = e.Idendereco,
                        IdPessoa = e.Idpessoa,
                        CEP = e.Cep,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (pessoa == null)
            {
                return NotFound(new { mensagem = "Pessoa não localizada!" });
            }

            return Ok(pessoa);
        }

        // CADASTRAR
        [HttpPost]
        public async Task<ActionResult> Post(PessoaDTO dto)
        {
            try
            {
                var pessoa = new Pessoa
                {
                    Nome = dto.Nome,
                    Cpf = (long)dto.CPF,
                    DataNascimento = dto.DataNascimento,
                    Sexo = dto.Sexo,
                    Email = dto.Email,
                    Telefone = (long)dto.Telefone,
                    AtivoPessoa = dto.AtivoPessoa,

                    Enderecos = dto.Enderecos?.Select(e => new Endereco
                    {
                        Cep = e.CEP,
                        Numero = e.Numero,
                        Complemento = e.Complemento
                    }).ToList() ?? new List<Endereco>()
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPessoa),
                    new { id = pessoa.Idpessoa },
                    new { idPessoa = pessoa.Idpessoa });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao cadastrar pessoa.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ALTERAR
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, PessoaDTO dto)
        {
            try
            {
                var pessoa = await _context.Pessoas
                    .Include(p => p.Enderecos)
                    .FirstOrDefaultAsync(p => p.Idpessoa == id);

                if (pessoa == null)
                {
                    return NotFound(new { mensagem = "Pessoa não localizada!" });
                }

                pessoa.Nome = dto.Nome;
                pessoa.Cpf = (long)dto.CPF;
                pessoa.DataNascimento = dto.DataNascimento;
                pessoa.Sexo = dto.Sexo;
                pessoa.Email = dto.Email;
                pessoa.Telefone = (long)dto.Telefone;
                pessoa.AtivoPessoa = dto.AtivoPessoa;

                var enderecosDto = dto.Enderecos ?? new List<EnderecoDTO>();

                // ids dos endereços recebidos
                var idsRecebidos = enderecosDto
                    .Where(e => e.IdEndereco.HasValue)
                    .Select(e => e.IdEndereco!.Value)
                    .ToHashSet();

                // endereços existentes que não vieram no corpo serão removidos
                var enderecosRemover = pessoa.Enderecos
                    .Where(e => !idsRecebidos.Contains(e.Idendereco))
                    .ToList();

                _context.Enderecos.RemoveRange(enderecosRemover);

                foreach (var endPessoa in enderecosDto)
                {
                    if (endPessoa.IdEndereco.HasValue)
                    {
                        var endereco = pessoa.Enderecos
                            .FirstOrDefault(e => e.Idendereco == endPessoa.IdEndereco.Value);

                        if (endereco == null)
                        {
                            return BadRequest(new { mensagem = "O endereço não pertence a essa pessoa." });
                        }

                        endereco.Numero = endPessoa.Numero;
                        endereco.Cep = endPessoa.CEP;
                        endereco.Complemento = endPessoa.Complemento;
                    }
                    else
                    {
                        pessoa.Enderecos.Add(new Endereco
                        {
                            Numero = endPessoa.Numero,
                            Cep = endPessoa.CEP,
                            Complemento = endPessoa.Complemento
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao atualizar pessoa.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // EXCLUIR
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var pessoa = await _context.Pessoas
                    .Include(p => p.Enderecos)
                    .FirstOrDefaultAsync(p => p.Idpessoa == id);

                if (pessoa == null)
                {
                    return NotFound(new { mensagem = "Pessoa não localizada!" });
                }

                _context.Pessoas.Remove(pessoa);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return Conflict(new
                {
                    mensagem = "Não foi possível excluir. A pessoa pode ter atendimentos ou cadastro de funcionário vinculados.",
                    erro = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}
