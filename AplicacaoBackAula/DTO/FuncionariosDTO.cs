namespace AplicacaoBackAula.DTO
{
    public class FuncionariosDTO
    {
        public int IdPessoa { get; set; }
        public string? Cargo { get; set; }
        
        public DateOnly? DataAdmissao { get; set; }

        public List<PessoaDTO>? Pessoas { get; set; }
        
    }
}
