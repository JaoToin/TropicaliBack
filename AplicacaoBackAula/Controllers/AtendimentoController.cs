using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AplicacaoBackAula.DTO;
using AplicacaoBackAula.Models;
using NuGet.Versioning;

namespace AplicacaoBackAula.Controllers
{
    //ANOTAÇÕES DE IDENTIFICAÇÃO DE CONTROLLER
    [ApiController]
    //ANOTAÇÕES DE IDENTIFICAÇÃO DE CONTROLLER
    [Route("api/[controller]")]

    //DECLARAÇÃO DA CLASSE DE CONTROLLER
    public class AtendimentoController : ControllerBase
    {
        //DECLARAÇÃO DO ATRIBUTO DE CONTEXTO (_context)
        private readonly BdAulaBackendContext _context; //ATRIBUTO DE CONTEXTO PARA ACESSAR O BANCO DE DADOS


        //DECLARAÇÃO DO MÉTODO CONSTRUTOR
        public AtendimentoController(BdAulaBackendContext context) // MÉTODO CONSTRUTOR PARA INICIALIZAR O ATRIBUTO DE CONTEXTO
        {
            //INSTANCIA DO ATRIBUTO CONSTRUTOR
            _context = context;
        }

        // DECLARAÇÃO DO MÉTODO PARA LISTAR TODOS OS ATENDIMENTOS E SEUS ITENS
        [HttpGet]

        public async Task<ActionResult<IEnumerable<AtendimentoListDTO>>> GetAtendimento()
        {

            var atendimentos = await _context.Atendimentos
                .Select(a => new AtendimentoListDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    Status = a.StatusAtendimento,

                    Itens = a.Atendimentoitems.Select(i => new AtendimentoItemListDTO
                    {

                        IdAtendimento = i.Idatendimento,
                        IdItem = i.Iditem,
                        Item = i.IditemNavigation.Item1,
                        IdFuncionario = i.Idfuncionario,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        Quantidade = i.QuantidadeItens,
                        ValorUnitario = i.ValorunitarioEntrada

                    }).ToList()

                }).ToListAsync();

            return Ok(atendimentos);
        }// fim do método GetAtendimento

        // DECLARAÇÃO DO MÉTODO PARA LISTAR UM ATENDIMENTO E SEUS ITENS PELO ID
        [HttpGet("{id}")]

        public async Task<ActionResult<AtendimentoListDTO>> GetAtendimento(int id)
        {
            var atendimento = await _context.Atendimentos.Where(a => a.Idatendimento == id)
                .Select(a => new AtendimentoListDTO
                {
                    IdAtendimento = a.Idatendimento,
                    IdPessoa = a.Idpessoa,
                    NomePessoa = a.IdpessoaNavigation.Nome,
                    DataAtendimento = a.DataAtendimento,
                    Status = a.StatusAtendimento,

                    Itens = a.Atendimentoitems.Select(i => new AtendimentoItemListDTO
                    {

                        IdAtendimento = i.Idatendimento,
                        IdItem = i.Iditem,
                        Item = i.IditemNavigation.Item1,
                        IdFuncionario = i.Idfuncionario,
                        NomeFuncionario = i.IdfuncionarioNavigation.IdpessoaNavigation.Nome,
                        Cargo = i.IdfuncionarioNavigation.Cargo,
                        Quantidade = i.QuantidadeItens,
                        ValorUnitario = i.ValorunitarioEntrada

                    }).ToList()


                }).FirstOrDefaultAsync();

            if (atendimento == null)
            {
                return NotFound(new { mensagem = "Atendimento Não Localizado!!!!!!!!!!!!!!!!!" });
            }

            return Ok(atendimento);
        } // fim do método GetAtendimento

        // DECLARAÇÃO DO MÉTODO PARA CRIAR UM NOVO ATENDIMENTO E SEUS ITENS
        [HttpPost]

        public async Task<ActionResult<AtendimentoListDTO>> PostAtendimento(AtendimentoPostDTO dto)
        {
            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Idpessoa == dto.IdPessoa);

            if ( pessoa == null)
            {
                return BadRequest(NotFound(new { mensagem = "Pessoa Não Localizada!!!!!!!!!!!!!!!!!" }));

            }

            var atendimento = new Atendimento
            {
                Idpessoa = dto.IdPessoa,
                DataAtendimento = dto.DataAtendimento,
                StatusAtendimento = dto.StatusAtendimento,
                               
                Atendimentoitems = dto.Itens.Select(i => new Atendimentoitem
                {
                    Iditem = i.IdItem,
                    Idfuncionario = i.IdFuncionario,
                    QuantidadeItens = i.Quantidade,
                    ValorunitarioEntrada = i.ValorUnitario
                }).ToList()
            };

            _context.Atendimentos.Add(atendimento);

            await _context.SaveChangesAsync();


            // Criação do DTO de resposta
            return CreatedAtAction(nameof(GetAtendimento),
                new {id = atendimento.Idatendimento}, dto);
        }// fim do método PostAtendimento

        [HttpPut("{id}")]

        public async Task<ActionResult> PutAtendimento(int id, AtendimentoPutDTO dto)
        {

            var atendimento = await _context.Atendimentos.Include(a => a.Atendimentoitems).FirstOrDefaultAsync(a => a.Idatendimento == id);

            if (atendimento == null)
            {

                return NotFound(new { mensagem = "Atendimento Não Localizado!!!!!!!!!!!!!!!!!!!" });

            }

            atendimento.StatusAtendimento = dto.StatusAtendimento;

            var IdRecebido = dto.Itens.Select(i => (i.IdItem, i.IdFuncionario)).ToHashSet();

            // comparando os ids recebidos pelo dto com os ids vindo do banco de dados. caso diferente armazena na variavel os ids que não vieram do dto
            var IdRemover = atendimento.Atendimentoitems.Where(i => !IdRecebido.Contains((i.Iditem, i.Idfuncionario)));

            _context.Atendimentoitems.RemoveRange(IdRemover);

            //percorrendo os itens vindos dos dto 
            foreach (var itemDTO in dto.Itens)
            { 
                 // buscar do banco de dados os itens do atendimento passado por parametros 
                var itemExistente = atendimento.Atendimentoitems.FirstOrDefault(i => i.Iditem == itemDTO.IdItem &&
                i.Idfuncionario == itemDTO.IdFuncionario);

                // verificar se existi, caso exista atualizara a quantidade e o valor unitario
                if (itemExistente != null)
                {
                    itemExistente.QuantidadeItens = itemDTO.Quantidade;
                    itemExistente.ValorunitarioEntrada = itemDTO.ValorUnitario;
                }
                else
                {
                    // caso não exista no banco de dados cria um novo item ao pedidoo
                    atendimento.Atendimentoitems.Add(new Atendimentoitem
                    {
                        Idatendimento = atendimento.Idatendimento,
                        Iditem = itemDTO.IdItem,
                        Idfuncionario = itemDTO.IdFuncionario,
                        QuantidadeItens = itemDTO.Quantidade,
                        ValorunitarioEntrada = itemDTO.ValorUnitario
                    });
                }
            }
            // ssalvando os alterações no banco de dados 
            await _context.SaveChangesAsync();

            //não retornara nada !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return NoContent();
        } // fim putatendimento

        [HttpDelete("{id}")]




    }// fim controller 

}// fim namespace
