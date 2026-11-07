-- Data da última mudança de preço do produto (atualizado_em muda também quando só o nome é corrigido)
ALTER TABLE produtos ADD COLUMN preco_atualizado_em TEXT;

UPDATE produtos SET preco_atualizado_em = atualizado_em;
