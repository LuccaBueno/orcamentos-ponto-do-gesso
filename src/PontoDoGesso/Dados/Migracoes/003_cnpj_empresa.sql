-- CNPJ da empresa (tela Empresa) e cópia no orçamento: reabrir ou exportar de novo sai igual, mesmo se o CNPJ mudar depois.
-- Os outros dados da empresa continuam nas mesmas chaves loja.* da tabela configuracoes: nada é apagado nem copiado.
ALTER TABLE orcamentos ADD COLUMN mostrar_cnpj INTEGER NOT NULL DEFAULT 0;
ALTER TABLE orcamentos ADD COLUMN empresa_cnpj TEXT;

INSERT OR IGNORE INTO configuracoes (chave, valor) VALUES ('loja.cnpj', '');
