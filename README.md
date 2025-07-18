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
