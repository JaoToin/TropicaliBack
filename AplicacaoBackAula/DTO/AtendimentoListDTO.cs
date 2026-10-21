namespace AplicacaoBackAula.DTO
{
    // DTO para listar atendimentos com seus itens
    public class AtendimentoListDTO
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public string? NomePessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string? Status { get; set; }

        // Lista de itens relacionados ao atendimento
        public List<AtendimentoItemListDTO> Itens { get; set; } = new();
    }
}
