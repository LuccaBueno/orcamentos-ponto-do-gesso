namespace PontoDoGesso.Dominio
{
    public class ItemOrcamento
    {
        public long Id { get; set; }
        public int Ordem { get; set; }
        public decimal Quantidade { get; set; }
        public string Unidade { get; set; }
        public string Descricao { get; set; }
        public long UnitarioCentavos { get; set; }
        public long TotalCentavos { get; set; }

        /// <summary>Só referência; descrição e preço ficam copiados no item. Nulo = item digitado à mão.</summary>
        public long? ProdutoId { get; set; }

        public static ItemOrcamento DoProduto(Produto produto, decimal quantidade) =>
            new ItemOrcamento
            {
                ProdutoId = produto.Id,
                Quantidade = quantidade,
                Unidade = produto.Unidade,
                Descricao = produto.Nome,
                UnitarioCentavos = produto.PrecoCentavos
            };
    }
}
