using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Dados
{
    public class ClienteRepositorio
    {
        private readonly Banco _banco;

        public ClienteRepositorio(Banco banco)
        {
            _banco = banco;
        }

        public void Inserir(Cliente cliente)
        {
            var agora = DateTime.Now;
            cliente.CriadoEm = agora;
            cliente.AtualizadoEm = agora;

            using (var conexao = _banco.Abrir())
            {
                cliente.Id = conexao.ExecuteScalar<long>(
                    @"INSERT INTO clientes (nome, telefone, telefone2, endereco, numero, bairro, cidade, ponto_referencia, observacoes, ativo, criado_em, atualizado_em)
                      VALUES (@Nome, @Telefone, @Telefone2, @Endereco, @Numero, @Bairro, @Cidade, @PontoReferencia, @Observacoes, @Ativo, @CriadoEm, @AtualizadoEm);
                      SELECT last_insert_rowid();",
                    cliente);
            }
        }

        public void Atualizar(Cliente cliente)
        {
            cliente.AtualizadoEm = DateTime.Now;

            using (var conexao = _banco.Abrir())
            {
                conexao.Execute(
                    @"UPDATE clientes
                      SET nome = @Nome, telefone = @Telefone, telefone2 = @Telefone2, endereco = @Endereco, numero = @Numero, bairro = @Bairro,
                          cidade = @Cidade, ponto_referencia = @PontoReferencia, observacoes = @Observacoes, ativo = @Ativo, atualizado_em = @AtualizadoEm
                      WHERE id = @Id",
                    cliente);
            }
        }

        /// <summary>Clientes não são excluídos: orçamentos antigos podem apontar para eles.</summary>
        public void DefinirAtivo(long id, bool ativo)
        {
            using (var conexao = _banco.Abrir())
                conexao.Execute("UPDATE clientes SET ativo = @ativo, atualizado_em = @agora WHERE id = @id", new { id, ativo, agora = DateTime.Now });
        }

        /// <summary>Busca por código, nome (sem acento) ou telefone; o código exato vem primeiro.</summary>
        public List<Cliente> Buscar(string texto, bool incluirInativos = false)
        {
            var termo = new TermoBusca(texto);

            using (var conexao = _banco.Abrir())
            {
                return conexao.Query<Cliente>(
                    @"SELECT * FROM clientes
                      WHERE (@incluirInativos OR ativo = 1)
                        AND (id = @Codigo
                             OR remover_acentos(nome) LIKE @Padrao ESCAPE '\'
                             OR somente_digitos(telefone) LIKE @PadraoTelefone
                             OR somente_digitos(telefone2) LIKE @PadraoTelefone)
                      ORDER BY id = @Codigo DESC, remover_acentos(nome)",
                    new { termo.Codigo, termo.Padrao, termo.PadraoTelefone, incluirInativos }).ToList();
            }
        }

        public Cliente ObterPorId(long id)
        {
            using (var conexao = _banco.Abrir())
                return conexao.QuerySingleOrDefault<Cliente>("SELECT * FROM clientes WHERE id = @id", new { id });
        }
    }
}
