Este repositório consta o projeto em C#.NET 8.0, utilizando o editor Visual Studio (Community versão 2022):

    • Versão .NET: 8;
    • Extensões: SonarLint (Controle de Código)
    • Dependências:
      o Frameworks:
         Microsoft.AspNetCore.App
         Microsoft.NETCore.App
      o Pacotes:
         Microsoft.AspNetCore.Mvc.NewtonsoftJson
         Microsoft.EntityFrameworkCore.Design
         Microsoft.EntityFrameworkCore.Tools
         Microsoft.VisualStudio.Web.CodeGeneration.Design
         Pomelo.EntityFrameworkCore.MySql
         Swashbuckle.AspNetCore

A finalidade deste projeto é controlar o acesso ao usuário pela aplicação não de si mesmo e sim de outras.

![image](https://github.com/user-attachments/assets/2192056a-8aef-4c7c-a0f2-6ac4964c2a25)

Elaboração de tabelas e seus respectivos relacionamentos escritas em MySQL Workbench.

![image](https://github.com/user-attachments/assets/193f5fa7-5cef-4b4b-b9cf-664c25f4cf2a)

  Detalhes gerais do BD de Controle de Acesso (Tabelas e Stored Procedures).

O propósito geral é testar as funcionalidades nas tabelas com seus relacionamentos baseadas em CRUD via WEB API (com utilização de Hibernate local) com regras básicas de negócio mas sem utilizar os métodos do BD MySQL (Stored Procedures):
    • nulos.
    • conferência de senha (funções próprias do C#, para garantia de segurança).
    • tamanho escasso (menos de 3 caracteres) ou em excesso (acima do permitido).
    • existência de campos chaves nos relacionamentos.

![image](https://github.com/user-attachments/assets/be94e714-7eff-480c-a7bb-0ad9a14f378e)

![image](https://github.com/user-attachments/assets/5a133e08-f82b-468c-9246-d4db8a8570db)

As API's das tabelas foram testadas com exaustão respeitando os princípios da regra de negócio apresentada acima utilizando Swagger (GET, POST, PUT, e DELETE) - 2 imagens acima, obtendo resultados em JSON (dados ou mensagem extraída diretamente no próprio código independentemente da operação que vier a executar).
