-- Versões do orçamento e controle de saída de material.
-- Os orçamentos que já existem viram a versão 1 (sem sufixo), não enviada e sem controle de saídas.
ALTER TABLE orcamentos ADD COLUMN versao INTEGER NOT NULL DEFAULT 1;
ALTER TABLE orcamentos ADD COLUMN enviado_em TEXT;              -- NULL = versão atual ainda não enviada
ALTER TABLE orcamentos ADD COLUMN controlar_saidas INTEGER NOT NULL DEFAULT 0;
ALTER TABLE itens_orcamento ADD COLUMN saiu TEXT NOT NULL DEFAULT '0';  -- quantidade invariante, como a quantidade

-- Versões antigas (já enviadas), guardadas inteiras para abrir só para leitura e exportar de novo
CREATE TABLE versoes_orcamento (
  id           INTEGER PRIMARY KEY AUTOINCREMENT,
  orcamento_id INTEGER NOT NULL REFERENCES orcamentos(id) ON DELETE CASCADE,
  versao       INTEGER NOT NULL,
  conteudo     TEXT NOT NULL,            -- o orçamento com os itens, em XML
  criado_em    TEXT NOT NULL,
  UNIQUE (orcamento_id, versao)
);
