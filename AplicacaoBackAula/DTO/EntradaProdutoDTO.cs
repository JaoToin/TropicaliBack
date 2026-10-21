namespace AplicacaoBackAula.DTO
{
    public class EntradaProdutoDTO
    {
        public int IdItem { get; set; }
        public int QuantidadeEntrada { get; set; }
        public decimal PrecoUnitario { get; set; }
        public DateOnly DataEntrada { get; set; }
        public string? Unidade {get; set;}

    }
}
