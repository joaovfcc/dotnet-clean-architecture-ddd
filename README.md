🎯 BATEU - Sistema de Conciliação Inteligente
"Quando o saldo bate, a gente comemora."

O BATEU é uma API de alta performance para conciliação financeira e contábil. O sistema automatiza o cruzamento de extratos bancários (OFX/CSV) contra razões contábeis (ERP), identificando discrepâncias de valor, data e lançamentos não contabilizados através de algoritmos de Fuzzy Matching.

🏗️ Arquitetura do Sistema
O projeto foi construído seguindo os princípios da Clean Architecture, garantindo que as regras de negócio (Domínio) não dependam de detalhes de implementação (Banco de Dados/Frameworks).

Fluxo de Processamento Assíncrono
Para lidar com arquivos grandes sem bloquear a API, utilizamos um padrão de Producer-Consumer com System.Threading.Channels.

Diagrama de Sequencia
    autonumber
    actor User as Contador
    participant API as API Controller
    participant Queue as Channel (Memory)
    participant Worker as Background Service
    participant Engine as Reconciliation Engine
    participant DB as PostgreSQL

    User->>API: Upload (Extrato Banco + Sistema)
    API->>API: Validação (FluentValidation)
    API->>DB: Cria Reconciliação (Status: Pendente)
    API->>Queue: Enfileira ID
    API-->>User: 202 Accepted (ID do Processo)
    
    rect rgb(240, 248, 255)
    Note right of Worker: Processamento em Background
    Worker->>Queue: Consome ID
    Worker->>DB: Busca Transações
    Worker->>Engine: Inicia Algoritmo de Match
    Engine->>Engine: Fase 1: Match Exato (Hash Map)
    Engine->>Engine: Fase 2: Match Aproximado (Heurística)
    Engine->>DB: Salva Resultados
    end
    
    Worker->>DB: Atualiza Status (Concluído)
    
Modelagem de Dados (ERD)
A estrutura do banco reflete a separação entre o dado bruto importado e o resultado da inteligência do sistema.

Diagrama Entidade-Relacionamento
    RECONCILIACAO ||--|{ TRANSACAO : contem
    RECONCILIACAO ||--|{ RESULTADO : gera
    USER ||--|{ RECONCILIACAO : possui

    RECONCILIACAO {
        uuid Id PK
        string Status
        datetime DataProcessamento
    }

    TRANSACAO {
        uuid Id PK
        decimal Valor
        datetime Data
        string Tipo "Debito/Credito"
        string Origem "Banco/Sistema"
    }

    RESULTADO {
        uuid Id PK
        string TipoMatch "Exato/Fuzzy/Manual"
        decimal DiferencaValor
        string Observacao
    }
    
🧠 Lógica de Negócio (O Diferencial)
Como a contabilidade real raramente é perfeita, o BATEU não faz apenas comparações exatas. Ele utiliza um motor de decisão em duas etapas:

Fast Pass (Exact Match - O(1)):

Utiliza Dicionários em memória para encontrar transações onde Data, Valor e Tipo são idênticos.

Performance instantânea para 90% dos casos.

Smart Pass (Fuzzy Logic):

Analisa as sobras (transações não conciliadas).

Janela Temporal: Aceita casamentos se a data do banco diferir em até X dias da data do sistema (comum em compensação de boletos/cartões).

Tolerância Monetária: Aceita pequenas diferenças de centavos (arredondamento de sistemas diferentes).

🛠️ Stack Tecnológica
Core: .NET 10 (C#)

Banco de Dados: PostgreSQL 15

Containerização: Docker & Docker Compose

ORM: Entity Framework Core (Code First)

Autenticação: Identity + JWT Bearer

Background Jobs: Hosted Services + Channels

Bibliotecas Principais:

Mapster: Mapeamento de objetos de alta performance.

FluentValidation: Regras de validação fora das entidades.

CsvHelper: Parsing robusto de arquivos financeiros.

xUnit: Testes unitários.

🚀 Como Rodar o Projeto
Pré-requisitos
Docker instalado.

.NET 10 SDK (apenas para desenvolvimento).

Passo a Passo
Clone o repositório:

Bash
git clone https://github.com/seu-usuario/bateu.git
cd bateu
Suba a Infraestrutura (Postgres): Não é necessário instalar o Postgres na máquina, o Docker cuida disso.

Bash
docker-compose up -d
Execute a API:

Bash
cd Bateu.API
dotnet run
Acesse a Documentação: Abra o navegador em: http://localhost:5000/swagger

📂 Estrutura do Projeto
Plaintext
Bateu/
├── Bateu.Domain/            # Entidades, Enums e Interfaces (Puro)
├── Bateu.Application/       # Casos de Uso, DTOs, Validators, Services
├── Bateu.Infrastructure/    # EF Core, Identity, File Parsers, Background Jobs
└── Bateu.API/               # Controllers, Configuração de DI, Middlewares
🧪 Testes
A integridade do algoritmo financeiro é garantida por testes unitários cobrindo cenários de borda.

Bash
# Executar todos os testes
dotnet test
Principais cenários cobertos:

✅ Match Exato simples.

✅ Match com diferença de 1 dia (Fuzzy).

✅ Match com diferença de R$ 0,01 (Fuzzy).

✅ Detecção de duplicidade.

👤 Autor
João Vitor Desenvolvedor de Software & Ex-Contador
