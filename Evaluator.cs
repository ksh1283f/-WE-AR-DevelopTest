using System;
using System.Collections.Generic;
using System.Text;

public class Evaluator
{
    private const char OPEN_BRACKET = '(';
    private const char CLOSED_BRACKET = ')';
    private const char PLUS = '+';
    private const char MINUS = '-';
    private const char DIVIDE = '/';
    private const char MULTIPLY = '*';
    private const char SINE = 's';
    private const char COSINE = 'c';

    /// <summary>
    /// char: key, 연산자의 종류
    /// int: 연산자의 우선순위
    /// </summary>
    private Dictionary<char, int> operatorDic = new Dictionary<char, int>()
    {
        { OPEN_BRACKET, 0 },
        { CLOSED_BRACKET, 0 },
        { PLUS, 1 },
        { MINUS, 1 },
        { DIVIDE, 2 },
        { MULTIPLY, 2 },
        { SINE, 3 },
        { COSINE, 3 }
    };

    public Evaluator()
    {
    }

    public double Evaluate(string str)
    {
        // 1. infix -> postfix
        List<string> list = GetPostfix(str);

        if (list.Count == 0)
        {
            Console.WriteLine("Expression is invalid!");
            return double.MaxValue;
        }

        // 2. calculate postfix
        Stack<double> operandStack = new Stack<double>();
        for (int i = 0; i < list.Count; i++)
        {
            string target = list[i];
            char key = ' ';
            // 연산자
            if (char.TryParse(target, out key) && operatorDic.ContainsKey(key))
            {
                Func<double, double, double> calculator = null;
                bool isUnaryOperator = false;
                switch (key)
                {
                    case PLUS:
                        calculator = (n1, n2) => { return n1 + n2; };
                        break;

                    case MINUS:
                        calculator = (n1, n2) => { return n1 - n2; };
                        break;

                    case MULTIPLY:
                        calculator = (n1, n2) => { return n1 * n2; };
                        break;

                    case DIVIDE:
                        calculator = (n1, n2) => { return n1 / n2; };
                        break;

                    case SINE:
                        isUnaryOperator = true;
                        calculator = (n1, n2) => { return Math.Sin(n1); };  // n2 는 사용안함
                        break;

                    case COSINE:
                        isUnaryOperator = true;
                        calculator = (n1, n2) => { return Math.Cos(n1); };  // n2 는 사용안함
                        break;

                    default:
                        Console.WriteLine("operator is invalid: " + key);
                        break;
                }

                int neededOperandCount = isUnaryOperator ? 1 : 2;
                List<double> targetList = new List<double>();
                for (int j = 0; j < neededOperandCount; j++)
                {
                    if (operandStack.Count == 0)
                        return double.MaxValue; // invalid situation

                    targetList.Add(operandStack.Pop());
                }

                if (calculator != null)
                {
                    double middleResult = 0;
                    if (isUnaryOperator)
                        middleResult = calculator(targetList[0], double.MaxValue);
                    else
                        middleResult = calculator(targetList[1], targetList[0]);    // LIFO이므로 반대 순서로 넣어주기

                    operandStack.Push(middleResult);
                }

                continue;
            }

            double num = double.MaxValue;
            if (double.TryParse(target, out num))
            {
                operandStack.Push(num);
                continue;
            }
        }

        if (operandStack.Count == 0 || operandStack.Count > 1)
        {
            Console.WriteLine("expression is invalid!");
            return double.MaxValue;
        }

        double calculatedResult = operandStack.Pop();
        return calculatedResult;
    }

    private List<string> GetPostfix(string str)
    {
        List<string> result = new List<string>();
        Stack<char> operatorStack = new Stack<char>();

        bool isContactMinus = false;

        // check list
        // 1. 연산자 피연산자 체크
        // 2. 괄호
        // 3. 피연산자의 경우
        string target = string.Empty;
        string expression = str;
        expression = expression.Replace(" ", "");   // 수식 공백제거
        expression = expression.Replace("sin", SINE.ToString());    // sin단어 치환
        expression = expression.Replace("cos", COSINE.ToString());    // cos단어 치환
        if (!CheckValidationOfExpression(expression))
            return result;

        int indexResult = 0;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < expression.Length; i++)
        {
            // 1. 피연산자라면
            if (!operatorDic.ContainsKey(expression[i]))
            {
                if (!char.IsDigit(expression[i]))
                    continue;

                indexResult = i;
                sb.Append(expression[indexResult++]);

                while (indexResult < expression.Length)
                {
                    if (expression[indexResult] != '.' && !char.IsDigit(expression[indexResult]))
                        break;

                    i = indexResult;
                    sb.Append(expression[indexResult++]);
                }

                target = sb.ToString();
                if (isContactMinus)
                {
                    target = string.Concat(MINUS, target);
                    isContactMinus = false;
                }

                result.Add(target);
                sb.Clear();
            }
            // 2. 연산자라면
            else
            {
                if (expression[i] == MINUS)
                {
                    if (i + 1 < expression.Length)
                    {
                        // 이전 요소가 연산자이거나
                        // 이전 요소가 열린괄호이거나
                        // 아예 없다면
                        // 부호로 인식
                        if (i - 1 < 0)
                        {
                            isContactMinus = true;
                            continue;
                        }

                        if (expression[i - 1] == OPEN_BRACKET)
                        {
                            isContactMinus = true;
                            continue;
                        }

                        if (expression[i - 1] == PLUS || expression[i - 1] == MINUS || expression[i - 1] == MULTIPLY || expression[i - 1] == DIVIDE)
                        {
                            isContactMinus = true;
                            continue;
                        }
                    }
                }

                // 일반적인 연산자의 경우
                // 연산자 우선순위 비교해야함
                if (IsArithmeticOperator(expression[i]))
                {
                    // 스택에 아무것도 없으면 바로 넣고
                    // 뭔가 있다면 우선순위를 비교한다.
                    if (operatorStack.Count > 0)
                    {
                        int targetPriority = operatorDic[expression[i]];
                        while (operatorStack.Count > 0)
                        {
                            char stackElementOperator = operatorStack.Peek();
                            int stackPriority = operatorDic[stackElementOperator];

                            if (stackPriority >= targetPriority)
                                result.Add(operatorStack.Pop().ToString());
                            else
                                break;
                        }
                    }
                }
                else    // 괄호의 경우
                {
                    if (expression[i] == CLOSED_BRACKET)
                    {
                        while (operatorStack.Peek() != OPEN_BRACKET)
                        {
                            result.Add(operatorStack.Pop().ToString());
                        }
                        if (operatorStack.Peek() == OPEN_BRACKET)
                            operatorStack.Pop();

                        continue;
                    }
                }

                operatorStack.Push(expression[i]);
            }
        }

        while (operatorStack.Count > 0)
            result.Add(operatorStack.Pop().ToString());

        return result;
    }

    private bool IsArithmeticOperator(char paramOperator)
    {
        return paramOperator == PLUS || paramOperator == MINUS || paramOperator == MULTIPLY || paramOperator == DIVIDE;
    }

    private bool CheckValidationOfExpression(string str)
    {
        // 불필요한 문자가 들어간 경우 체크
        for (int i = 0; i < str.Length; i++)
        {
            char element = str[i];
            if (operatorDic.ContainsKey(element))
                continue;
            else
            {
                if (char.IsDigit(element))
                    continue;

                if (element == '.')
                    continue;

                return false;
            }
        }

        return true;
    }
}