using System;

namespace KalkulatorApp
{
    // ==========================================
    // 1. OOP (Object Oriented Programming)
    // Class untuk menangani logika perhitungan matematika
    // ==========================================
    public class CalculatorEngine
    {
        // C# Basic: Variable & Operator
        public double PerformOperation(double operand1, double operand2, string op)
        {
            // C# Basic: Switch / Percabangan logika
            switch (op)
            {
                case "+":
                    return operand1 + operand2;
                case "-":
                    return operand1 - operand2;
                case "×":
                case "*":
                    return operand1 * operand2;
                case "÷":
                case "/":
                    // Error Handling: Pembagian dengan nol
                    if (operand2 == 0)
                        throw new DivideByZeroException("Tidak dapat membagi dengan nol.");
                    return operand1 / operand2;
                case "^":
                    // Operasi Pangkat: operand1 ^ operand2
                    if (operand1 == 0 && operand2 < 0)
                        throw new DivideByZeroException("Tidak dapat membagi dengan nol.");
                    double powResult = Math.Pow(operand1, operand2);
                    if (double.IsNaN(powResult))
                        throw new InvalidOperationException("Hasil tidak terdefinisi (bukan bilangan riil).");
                    if (double.IsInfinity(powResult))
                        throw new OverflowException("Hasil melebihi batas kapasitas angka.");
                    return powResult;
                default:
                    throw new ArgumentException("Operator tidak valid.");
            }
        }

        // Operasi Unary (Akar kuadrat)
        public double PerformUnaryOperation(double operand, string op)
        {
            switch (op)
            {
                case "√":
                case "sqrt":
                    // Error Handling: Akar bilangan negatif
                    if (operand < 0)
                        throw new ArgumentException("Tidak dapat menghitung akar dari bilangan negatif.");
                    return Math.Sqrt(operand);
                default:
                    throw new ArgumentException("Operator tidak valid.");
            }
        }
    }
}
