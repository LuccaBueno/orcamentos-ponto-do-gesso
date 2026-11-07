using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Dados
{
    public class ProdutoRepositorio
    {
        private readonly Banco _banco;

        public ProdutoRepositorio(Banco banco)
        {
            _banco = banco;
        }

        public void Inserir(Produto produto)
        {
            var agora = DateTime.Now;
            produto.CriadoEm = agora;
            produto.AtualizadoEm = agora;

            using (var conexao = _banco.Abrir())
            {
                produto.Id = conexao.ExecuteScalar<long>(
                    @"INSERT INTO produtos (nome, unidade, preco_centavos, ativo, criado_em, atualizado_em)
                      VALUES (@Nome, @Unidade, @PrecoCentavos, @Ativo, @CriadoEm, @AtualizadoEm);
                      SELECT last_insert_rowid();",
                    produto);
            }
        }

        public void Atualizar(Produto produto)
        {
            produto.AtualizadoEm = DateTime.Now;

            using (var conexao = _banco.Abrir())
            {
                conexao.Execute(
                    @"UPDATE produtos
                      SET nome = @Nome, unidade = @Unidade, preco_centavos = @PrecoCentavos, ativo = @Ativo, atualizado_em = @AtualizadoEm
                      WHERE id = @Id",
                    produto);
            }
        }

        /// <summary>Busca por código ou nome (sem acento); o código exato vem primeiro.</summary>
        public List<Produto> Buscar(string texto, bool incluirInativos = false)
        {
            var termo = new TermoBusca(texto);

            using (var conexao = _banco.Abrir())
            {
                return conexao.Query<Produto>(
                    @"SELECT * FROM produtos
                      WHERE (@incluirInativos OR ativo = 1)
                        AND (id = @Codigo OR remover_acentos(nome) LIKE @Padrao ESCAPE '\')
                      ORDER BY id = @Codigo DESC, remover_acentos(nome)",
                    new { termo.Codigo, termo.Padrao, incluirInativos }).ToList();
            }
        }

        public Produto ObterPorId(long id)
        {
            using (var conexao = _banco.Abrir())
                return conexao.QuerySingleOrDefault<Produto>("SELECT * FROM produtos WHERE id = @id", new { id });
        }
    }
}
