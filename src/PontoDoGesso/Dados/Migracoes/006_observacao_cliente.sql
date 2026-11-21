-- Observação do cadastro do cliente incluída no orçamento (caixinha desmarcada por padrão).
-- Cópia (snapshot) do texto usado: editar o cliente depois não muda o orçamento.
ALTER TABLE orcamentos ADD COLUMN incluir_obs_cliente INTEGER NOT NULL DEFAULT 0;
ALTER TABLE orcamentos ADD COLUMN obs_cliente TEXT;             -- NULL quando não incluída
