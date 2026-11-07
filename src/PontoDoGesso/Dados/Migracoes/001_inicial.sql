-- Esquema inicial. Valores em centavos; quantidades em TEXT invariante; datas em TEXT ISO.

CREATE TABLE clientes (
  id               INTEGER PRIMARY KEY AUTOINCREMENT,
  nome             TEXT NOT NULL,
  telefone         TEXT,
  telefone2        TEXT,
  endereco         TEXT,           -- rua
  numero           TEXT,
  bairro           TEXT,
  cidade           TEXT DEFAULT 'Pindamonhangaba',
  ponto_referencia TEXT,
  observacoes      TEXT,           -- nota interna; NÃO sai no orçamento
  ativo            INTEGER NOT NULL DEFAULT 1,
  criado_em        TEXT NOT NULL,
  atualizado_em    TEXT NOT NULL
);

CREATE TABLE produtos (
  id               INTEGER PRIMARY KEY AUTOINCREMENT,
  nome             TEXT NOT NULL,
  unidade          TEXT,           -- un, m², m, kg, saco, cx...
  preco_centavos   INTEGER NOT NULL DEFAULT 0,
  ativo            INTEGER NOT NULL DEFAULT 1,
  criado_em        TEXT NOT NULL,
  atualizado_em    TEXT NOT NULL
);

CREATE TABLE orcamentos (
  id                INTEGER PRIMARY KEY AUTOINCREMENT,
  numero            INTEGER NOT NULL UNIQUE,   -- sequencial 1, 2, 3... exibido como 0001
  data              TEXT NOT NULL,
  validade_dias     INTEGER,                   -- NULL = sem validade (não imprime)
  cliente_id        INTEGER REFERENCES clientes(id),   -- NULL = cliente avulso
  -- cópia (snapshot) do cliente/entrega no momento do orçamento
  cli_nome          TEXT NOT NULL,
  cli_telefone      TEXT,
  ent_endereco      TEXT,
  ent_numero        TEXT,
  ent_bairro        TEXT,
  ent_cidade        TEXT,
  ent_referencia    TEXT,
  tipo_entrega      TEXT NOT NULL DEFAULT 'ENTREGA',   -- ENTREGA | RETIRADA
  observacoes       TEXT,                      -- sai impresso no orçamento
  subtotal_centavos INTEGER NOT NULL,
  desconto_tipo     TEXT,                      -- NULL (sem desconto) | VALOR | PERCENTUAL
  desconto_valor    TEXT,                      -- centavos se VALOR; percentual invariante (ex.: "5.5") se PERCENTUAL
  desconto_centavos INTEGER NOT NULL DEFAULT 0,  -- valor final do desconto em centavos
  frete_centavos    INTEGER NOT NULL DEFAULT 0,
  total_centavos    INTEGER NOT NULL,
  criado_em         TEXT NOT NULL,
  atualizado_em     TEXT NOT NULL
);

CREATE TABLE itens_orcamento (
  id                INTEGER PRIMARY KEY AUTOINCREMENT,
  orcamento_id      INTEGER NOT NULL REFERENCES orcamentos(id) ON DELETE CASCADE,
  ordem             INTEGER NOT NULL,
  quantidade        TEXT NOT NULL,
  unidade           TEXT,
  descricao         TEXT NOT NULL,
  unitario_centavos INTEGER NOT NULL,
  total_centavos    INTEGER NOT NULL,
  produto_id        INTEGER REFERENCES produtos(id)   -- só referência; descrição e preço ficam copiados
);

CREATE TABLE configuracoes ( chave TEXT PRIMARY KEY, valor TEXT );
CREATE TABLE versao_banco  ( versao INTEGER NOT NULL );

CREATE INDEX ix_clientes_nome     ON clientes(nome);
CREATE INDEX ix_clientes_telefone ON clientes(telefone);
CREATE INDEX ix_orcamentos_data   ON orcamentos(data);
CREATE INDEX ix_orcamentos_cli    ON orcamentos(cli_nome);
CREATE INDEX ix_orcamentos_clid   ON orcamentos(cliente_id);
