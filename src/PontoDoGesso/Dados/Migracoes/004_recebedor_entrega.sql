-- Quem recebe no local da entrega e o telefone de lá (opcionais; só saem no orçamento quando preenchidos).
ALTER TABLE orcamentos ADD COLUMN ent_receber_com TEXT;
ALTER TABLE orcamentos ADD COLUMN ent_telefone_local TEXT;
