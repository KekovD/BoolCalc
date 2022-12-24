using System;
using System.Collections.Generic;

namespace Calc
{
    public class BoolFunction
    {
        private string _input;
        private List<string> _variables;
        private List<string> _postfixStack;
        private List<string> _functions;
        private List<List<bool>> _truthTable;
        private List<bool> _functionValues;
        private List<string> _fullSDNF;

        public BoolFunction()
        {
            _postfixStack = new List<string>();
            _functions = new List<string>();
            _variables = new List<string>();
            _truthTable = new List<List<bool>>();
            _functionValues = new List<bool>();
            _fullSDNF = new List<string>();
        }

        public void Processing()
        {
            Console.Write("Введите переменные: ");
            _variables.AddRange(Console.ReadLine().Split(' '));
            Console.Write("Введите функцию: ");
            _input = Console.ReadLine();
            AddFunction();
            PostfixParse();
            CrateTruthTable();
            PrintTruthTable();
            PrintSDNF();
            PrintSKNF();
            PrintMinterm();
            PrintMaxterm();
            KarnaughMap();
        }

        private void AddFunction()
        {
            if (_input.Contains(" "))
            {
                _input = _input.Replace(" ", "");
            }

            for (int i = 0; i < _input.Length; i++)
            {
                _functions.Add(_input[i].ToString());
            }
        }

        private void PostfixParse()
        {
            Stack<string> stack = new Stack<string>();
            foreach (var item in _functions)
            {
                if (_variables.Contains(item))
                {
                    _postfixStack.Add(item);
                }
                else if (item == "(")
                {
                    stack.Push(item);
                }
                else if (item == ")")
                {
                    while (stack.Peek() != "(")
                    {
                        _postfixStack.Add(stack.Pop());
                    }

                    stack.Pop();
                }
                else if (item == "!")
                {
                    stack.Push(item);
                }
                else if (item == "*" | item == "+" | item == "^" | item == "|" | item == "/" | item == ">" |
                         item == "<" | item == "=")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        _postfixStack.Add(stack.Pop());
                    }

                    stack.Push(item);
                }
            }

            while (stack.Count > 0)
            {
                _postfixStack.Add(stack.Pop());
            }
        }

        private bool Calculate(List<bool> item)
        {
            Stack<bool> stack = new Stack<bool>();
            ;
            foreach (var item2 in _postfixStack)
            {
                if (_variables.Contains(item2))
                {
                    stack.Push(item[_variables.IndexOf(item2)]);
                }
                else if (item2 == "!")
                {
                    stack.Push(!stack.Pop());
                }
                else if (item2 == "*")
                {
                    stack.Push(stack.Pop() & stack.Pop());
                }
                else if (item2 == "+")
                {
                    stack.Push(stack.Pop() | stack.Pop());
                }
                else if (item2 == "^")
                {
                    stack.Push(stack.Pop() ^ stack.Pop());
                }
                else if (item2 == "|")
                {
                    stack.Push(!stack.Pop() | !stack.Pop());
                }
                else if (item2 == "/")
                {
                    stack.Push(!stack.Pop() & !stack.Pop());
                }
                else if (item2 == ">")
                {
                    stack.Push(!stack.Pop() | stack.Pop());
                }
                else if (item2 == "<")
                {
                    stack.Push(stack.Pop() | !stack.Pop());
                }
                else if (item2 == "=")
                {
                    stack.Push(stack.Pop() == stack.Pop());
                }
            }

            return stack.Pop();
        }

        private void CrateTruthTable()
        {
            _truthTable = new List<List<bool>>();
            for (int i = 0; i < Math.Pow(2, _variables.Count); i++)
            {
                List<bool> list = new List<bool>();
                for (int j = 0; j < _variables.Count; j++)
                {
                    list.Add(Convert.ToBoolean(i & (1 << j)));
                }

                list.Reverse();
                _truthTable.Add(list);
            }
        }

        private void PrintTruthTable()
        {
            foreach (var item in _variables)
            {
                Console.Write(item + " ");
            }

            Console.WriteLine("F");
            foreach (var item in _truthTable)
            {
                foreach (var i in item)
                {
                    Console.Write(Convert.ToInt16(i) + " ");
                }

                var result = Calculate(item);
                _functionValues.Add(result);
                Console.Write(Convert.ToInt16(result));
                Console.WriteLine();
            }
        }

        private void PrintSDNF()
        {
            var list = new List<string>();
            for (int i = 0; i < _truthTable.Count; i++)
            {
                if (_functionValues[i])
                {
                    string s = "";
                    for (int j = 0; j < _truthTable[i].Count; j++)
                    {
                        if (_truthTable[i][j])
                        {
                            s += _variables[j];
                        }
                        else
                        {
                            s += "!" + _variables[j];
                        }
                    }

                    _fullSDNF.Add(s);
                    list.Add(s);
                }
            }

            string[] res = list.ToArray();
            Console.WriteLine("СДНФ: " + string.Join(" + ", res));
        }

        private void PrintSKNF()
        {
            var list = new List<string>();
            for (int i = 0; i < _truthTable.Count; i++)
            {
                if (!_functionValues[i])
                {
                    string s = "";
                    for (int j = 0; j < _truthTable[i].Count; j++)
                    {
                        if (j == 0)
                        {
                            s += "(";
                        }

                        if (_truthTable[i][j])
                        {
                            s += "!" + _variables[j];

                        }
                        else
                        {
                            s += _variables[j];
                        }

                        if (j != _truthTable[i].Count - 1)
                        {
                            s += "+";
                        }

                        if (j == _truthTable[i].Count - 1)
                        {
                            s += ")";
                        }
                    }

                    list.Add(s);
                }
            }

            string[] res = list.ToArray();
            Console.WriteLine("СКНФ: " + string.Join(" * ", res));
        }

        private void PrintMinterm()
        {
            Console.Write("СДНФ(Минтермы): ");
            for (int i = 0; i < _truthTable.Count; i++)
            {
                if (_functionValues[i])
                {
                    Console.Write("m" + i + " ");
                }
            }

            Console.WriteLine();
        }

        private void PrintMaxterm()
        {
            int count = 0;
            Console.Write("СКНФ(Макстермы): ");
            for (int i = _truthTable.Count - 1; i >= 0; i--)
            {
                if (!_functionValues[i])
                {
                    Console.Write("M" + count + " ");
                }

                count++;
            }

            Console.WriteLine();
        }

        private void KarnaughMap()
        {
            if (_variables.Count % 2 == 0)
            {
                int count = 0;
                bool[,] map = new bool[_variables.Count, _variables.Count];
                
                for (int i = 0; i < _variables.Count; i++)
                {
                    for (int j = 0; j < _variables.Count; j++)
                    {
                        map[i, j] = _functionValues[count];
                        count++;
                    }
                }
                
                Console.WriteLine("Карта Карно: ");
                for (int i = map.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        Console.Write(Convert.ToInt16(map[i, j]) + " ");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                int count = 0;
                bool[,] map = new bool[_variables.Count / 2 + 1, _variables.Count + 1];
                
                for (int i = 0; i < _variables.Count / 2 + 1; i++)
                {
                    for (int j = 0; j < _variables.Count + 1; j++)
                    {
                        map[i, j] = _functionValues[count];
                        count++;
                    }
                }
                
                Console.WriteLine("Карта Карно: ");
                for (int i = map.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        Console.Write(Convert.ToInt16(map[i, j]) + " ");
                    }

                    Console.WriteLine();
                }
            }
        }
    }

    internal class Calc
    {
        public static void Main()
        {
            BoolFunction boolFunction = new BoolFunction();
            boolFunction.Processing();
        }
    }
}