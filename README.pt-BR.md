# Workspace .NET preparado para IA

[English](README.md)

Um GitHub Template compacto e neutro de provedor para desenvolvimento .NET
reprodutível, com orientações de IA versionadas, ferramentas de privilégio mínimo,
validações determinísticas e uma Minimal API funcional. O repositório foi projetado
para ser renomeado após a criação e não escolhe modelo de IA, banco de dados da
aplicação nem provedor de nuvem de produção.

## Objetivos

- Alinhar colaboradores humanos e de IA por meio de contexto revisável no repositório.
- Tornar equivalentes e reprodutíveis os fluxos local, Dev Container e CI.
- Separar código da aplicação, testes, ferramentas e integrações externas.
- Demonstrar capacidades MCP restritas e fronteiras de segurança explícitas.
- Avaliar trabalho assistido por IA sem tratar checagens estruturais como evidência de modelo.

## Início rápido

Use o Dev Container ou instale os requisitos locais e execute:

```bash
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
dotnet run --project src/SampleApi --urls http://localhost:5080
```

Teste `GET /health` ou `GET /api/greeting?name=Ada`. No Linux, macOS, WSL ou
dentro do Dev Container, valide todo o ambiente local com:

```bash
./scripts/verify-environment.sh
```

## Requisitos do ambiente

Escolha uma forma de desenvolvimento:

- Docker e editor compatível com Dev Containers. A imagem fixada inclui o SDK .NET
  e usa o usuário não root `vscode`.
- SDK .NET 10 `10.0.400` ou patch compatível selecionado por `global.json`, Git e
  Bash. As validações também exigem utilitários POSIX comuns; no Windows, use WSL
  ou o Dev Container.

GitHub CLI é opcional e necessário somente para tarefas do repositório e PRs.
Copilot, Codex e outros clientes de IA são opcionais e podem exigir planos,
autenticação, confiança no workspace ou políticas da organização.

## Funcionalidades disponíveis

- Minimal API .NET 10 e testes HTTP de integração.
- Versões centralizadas, lockfiles, nullable, warnings como erros, build
  determinístico e auditoria NuGet.
- Dev Container fixado por digest, tarefas do VS Code e CI somente leitura.
- `AGENTS.md` compartilhado, instruções Copilot por escopo, prompts, agente de
  revisão especializado e skill de validação sob demanda.
- Servidor MCP local por stdio que expõe apenas metadados sanitizados e regras de
  arquitetura, coberto por testes unitários e de protocolo.
- Baseline de segurança, modelo de ameaças, protocolo de avaliação pareada e
  validação isolada da inicialização do template.

Consulte [Arquitetura](docs/architecture.md), [ADRs](docs/decisions/README.md),
[estudo de caso](docs/case-study/README.md) e [política de segurança](SECURITY.md).

## Usar como template no GitHub

Depois que o proprietário habilitar **Template repository**, selecione **Use this
template** no GitHub, crie e clone um repositório e inicialize os nomes:

```bash
./scripts/initialize-template.sh \
  --solution-name ContosoWorkspace \
  --project-name Contoso.Api
```

O inicializador renomeia solução, aplicação, testes, namespaces, referências de
projeto e referências conhecidas na documentação/configuração. Ele registra os
nomes em `.template-initialized`; faça commit desse arquivo para preservar a
verificação de idempotência. Nomes inseguros, destinos conflitantes e uma segunda
inicialização com nomes diferentes são rejeitados.

Até o merge desta branch, a versão completa não estará na branch padrão. Após o
merge, mantenedores devem seguir [Configurações do repositório](docs/repository-settings.md).

## Configurar Copilot, Codex, MCP, agents e skills

Revise os arquivos antes de confiar no workspace:

| Mecanismo | Responsabilidade |
| --- | --- |
| `AGENTS.md` | Regras compartilhadas para agentes compatíveis. |
| `.github/copilot-instructions.md` | Orientação geral do Copilot e instruções por caminho em `.github/instructions/`, quando suportadas. |
| `.github/agents/` e `.github/prompts/` | Revisor de arquitetura somente leitura e fluxos invocados manualmente. |
| `.agents/skills/validate-change/SKILL.md` | Procedimento de validação reutilizável sob demanda. |
| `.mcp.json` | Declaração portátil do MCP para clientes compatíveis. |
| `.codex/config.toml` | Padrões de sandbox, aprovação e allowlist MCP do Codex para projeto confiável. |

Antes de iniciar o comando MCP versionado, gere a configuração Release:

```bash
dotnet build AiReadyWorkspace.slnx --configuration Release
```

O servidor oferece `list_dotnet_projects` e `get_architecture_rules`, sem ferramenta
de escrita, rede, comando, credencial ou leitura arbitrária. Em um cliente Copilot
compatível, inspecione o servidor `workspace`, escolha **Architecture Reviewer** ou
invoque `/review-architecture`. No Codex, confie no projeto revisado, inspecione a
configuração efetiva e as ferramentas MCP e use a skill `validate-change` quando
relevante. Descoberta e precedência variam; as validações do repositório não provam
compatibilidade com todos os clientes e versões.

## Personalizar o template

1. Execute o inicializador uma vez e revise o diff.
2. Substitua a API de exemplo preservando as fronteiras úteis entre `src/`,
   `tests/` e `tools/`.
3. Atualize `AGENTS.md`, instruções, avaliações e ADRs quando arquitetura ou política mudar.
4. Adicione provedores externos somente com requisito concreto e nunca versione credenciais.
5. Atualize Dev Container, tarefas do VS Code e CI em conjunto.
6. Execute toda a validação antes de publicar o novo repositório.

## Build, testes e avaliações

```bash
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet format AiReadyWorkspace.slnx --verify-no-changes --no-restore
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
./scripts/validate-ai-context.sh
./scripts/validate-security.sh
./scripts/validate-evaluations.sh
./scripts/validate-template.sh
```

O último comando cria uma cópia temporária única, inicializa duas vezes, busca
nomes residuais, restaura, formata, compila, testa e confirma que o repositório de
origem não mudou.

O [protocolo de avaliação](evaluations/protocol.md) define execuções pareadas com
e sem contexto de IA. O validador apenas confere o contrato. Resultados reais de
modelo exigem versões, comandos, logs, auditoria de ferramentas, correções, duração
e limitações; este repositório ainda não afirma ter executado esses experimentos.

## Limitações e próximos passos

- Não há execução real de avaliação Copilot ou Codex incluída.
- Instruções orientam comportamento, mas não implementam sandbox do cliente.
- Proteção de branch, revisões obrigatórias, secret scanning, políticas da
  organização e ativação do template dependem do GitHub.
- O Dev Container não protege contra host ou runtime comprometido; ainda existem
  riscos de corrida no acesso ao sistema de arquivos local.
- A amostra não inclui autenticação, persistência, observabilidade ou deployment.
- Evoluções possíveis incluem avaliações pareadas controladas, CI multiplataforma
  e exemplos de provedores como módulos opcionais.

## Contribuição e licença

Veja [CONTRIBUTING.md](CONTRIBUTING.md) e [SECURITY.md](SECURITY.md). Licenciado
sob a [Licença MIT](LICENSE), copyright Rodrigo de Oliveira.
