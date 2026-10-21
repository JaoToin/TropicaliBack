namespace AplicacaoBackAula.DTO
{
    public class AtendimentoPostDTO
    {
        public int IdPessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string? StatusAtendimento { get; set; }
        // Lista de itens relacionados ao atendimento
        public List<AtendimentoItemPostDTO> Itens { get; set; } = new();
    }
}
