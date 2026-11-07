using System;
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

        public Cliente ObterPorId(long id)
        {
            using (var conexao = _banco.Abrir())
                return conexao.QuerySingleOrDefault<Cliente>("SELECT * FROM clientes WHERE id = @id", new { id });
        }
    }
}
