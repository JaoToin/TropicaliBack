namespace AplicacaoBackAula.DTO
{
    public class AtendimentoPutDTO
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public DateTime DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }
        public List<AtendimentoItemPutDTO> Itens { get; set; } = new();
    }
}
