using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qcx
{
    public interface IQC
    {
        int Integer { get; }
    }

    public abstract class QcBase
    {
        /// <summary>
        /// an int
        /// </summary>
        int integer = 1;

        /// <summary>
        /// An Integer
        /// </summary>
        protected int Integer
        {
            get { return integer; }
            set { integer = value; }
        }

        /// <summary>
        /// Of course, you know which double we're going to return...
        /// </summary>
        /// <returns>PI</returns>
        protected virtual double GetDouble()
        {
            return 3.1415926;
        }

        /// <summary>
        /// Multiplicatoin is fun!!
        /// </summary>
        /// <param name="multiplicand">Could have been factor</param>
        /// <returns>The product</returns>
        public double Multiply(double multiplicand)
        {
            return multiplicand * GetDouble();
        }

        /// <summary>
        /// A 2-factor mulication fn
        /// </summary>
        /// <param name="multiplier">Could have been factor1</param>
        /// <param name="multiplicand">Could have been factor2</param>
        /// <returns>The product</returns>
        public double Multiply(double multiplier, double multiplicand)
        {
            return multiplier * multiplicand * GetDouble();
        }
    }

    /// <summary>
    /// A delegate
    /// </summary>
    public delegate void QcImplement();
    
    /// <summary>
    /// A static class with a type param
    /// </summary>
    /// <typeparam name="T">The obligatory type param</typeparam>
    public static class StaticClass<T>
    {
        /// <summary>
        /// A static ToString method.  Not actually very helpful.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToString(T value)
        {
            return value.ToString();
        }
    }
    
    /// <summary>
    /// The test class
    /// </summary>
    public class FooBar : QcBase
    {
        /// <summary>
        /// string value
        /// </summary>
        public string StrValue { get; set; }

        /// <summary>
        /// The overriding method
        /// </summary>
        /// <returns></returns>
        protected override double GetDouble()
        {
            return 3.14159265359;
        }

        /// <summary>
        /// Integral addition
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int Add(int i)
        {
            return i + Integer;
        }
    }
}
