namespace AplicacaoBackAula.DTO
{
    public class CategoriaDTO
    {
        
        public string? Categoria { get; set; }

        public List<ProdutoDTO>? Produtos { get; set; } = new();
    }
}
