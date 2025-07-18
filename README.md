# ExpressionParser

Este projeto C# permite compilar e executar **expressões matemáticas** em tempo de execução utilizando a **API Roslyn** da Microsoft. Ele identifica variáveis, transforma a expressão original em um código C# compilável e executa dinamicamente a função gerada.

## ✨ Funcionalidades

- Detecta variáveis usadas na expressão (exclui chamadas de função como `Math.Sin`, `Double.Abs`, etc.).
- Gera código C# com assinatura: `double Run(double[] parms)`
- Compila dinamicamente com `Microsoft.CodeAnalysis` (Roslyn).
- Executa a expressão compilada com parâmetros numéricos.

## 🧠 Exemplo

Expressão:
```csharp
"X1 + X2 + Double.Sin(X1) / 1234 + Double.Tanh(X2) + Double.Abs(X2)"
```

Variáveis detectadas: `X1`, `X2`  
Elas serão mapeadas para `parms[0]` e `parms[1]`, respectivamente.

Código gerado:
```csharp
public class DynamicFunction {
    public static double Run(double[] parms) {
        // parms[0] -> X1
        // parms[1] -> X2
        return parms[0] + parms[1] + Double.Sin(parms[0]) / 1234 + Double.Tanh(parms[1]) + Double.Abs(parms[1]);
    }
}
```

## 🚀 Como usar

1. Clone o projeto e adicione as dependências necessárias ao seu projeto (.NET SDK e NuGet `Microsoft.CodeAnalysis.CSharp`).
2. Compile o projeto.
3. Execute o `Main()` para ver a geração e execução dinâmica da expressão.

## 🧩 Dependências

- [.NET 6 ou superior](https://dotnet.microsoft.com/)
- `Microsoft.CodeAnalysis.CSharp` (via NuGet)

## 🔧 Estrutura do código

- `DynamicExpression`:
    - Construtor: recebe a string da expressão.
    - Gera o código compilável com base nas variáveis detectadas.
    - Compila em memória.
    - Executa a função via reflexão.
- `Main()`:
    - Exemplo de uso com uma expressão de teste.

## ⚠️ Avisos

- Apenas variáveis simples (ex: `X1`, `X2`) são detectadas.
- Métodos embutidos do tipo `Math.Sin`, `Double.Tanh`, etc., são preservados.
- Não há validação semântica completa das expressões fornecidas — forneça expressões válidas em C#.

## 📄 Licença

MIT (ou adapte conforme seu projeto).
