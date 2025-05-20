using System.Numerics;

namespace BusinessObjects.ConfigModels
{
    public static class Base36
    {
        const int kByteBitCount = 8; // number of bits in a byte
        public readonly static char[] Digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        // constants that we use in ToBase36CharArray
        static readonly double kBase36CharsLengthDivisor = Math.Log(Digits.Length, 2);
        static readonly BigInteger kBigInt36 = new BigInteger(36);
        public static string ToBase36String(this byte[] bytes, bool bigEndian = false)
        {
            // Estimate the result's length so we don't waste time realloc'ing
            int result_length = (int)
                Math.Ceiling(bytes.Length * kByteBitCount / kBase36CharsLengthDivisor);
            // We use a List so we don't have to CopyTo a StringBuilder's characters
            // to a char[], only to then Array.Reverse it later
            var result = new System.Collections.Generic.List<char>(result_length);
            var dividend = new BigInteger(bytes);
            // IsZero's computation is less complex than evaluating "dividend > 0"
            // which invokes BigInteger.CompareTo(BigInteger)
            while (!dividend.IsZero)
            {
                BigInteger remainder;
                dividend = BigInteger.DivRem(dividend, kBigInt36, out remainder);
                int digit_index = Math.Abs((int)remainder);
                result.Add(Digits[digit_index]);
            }
            // orientate the characters in big-endian ordering
            if (!bigEndian)
                result.Reverse();
            // ToArray will also trim the excess chars used in length prediction
            return new string(result.ToArray());
        }

        public static string RandomCode(int letters = 5)
        {
            var arr = new char[letters];
            var random = new Random();
            for (var i = 0; i < letters; i++)
                arr[i] = Base36.Digits[random.Next(0, 35)];
            return new string(arr);
        }
    }
}
