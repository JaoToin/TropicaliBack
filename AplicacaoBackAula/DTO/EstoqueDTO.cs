namespace AplicacaoBackAula.DTO
{
    public class EstoqueDTO
    {
        public int IdItem { get; set; }

        public int Quantidade { get; set; }

        public List<EntradaProdutoDTO>? Entradas { get; set; } = new();

    }
}
