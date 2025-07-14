namespace CleanPeak.AseXmlProcessor
{
    public static class NmiHelper
    {
        public static string NmiCheckSum(string nmi)
        {
            nmi = nmi.Length > 10 ? nmi.Substring(0, 10) : nmi;
            int sum = 0;
            int parity = (nmi.Length - 1) % 2;
            for (int i = nmi.Length - 1; i >= 0; i--)
            {
                int digit = char.ToUpper(nmi[i]);
                if (char.IsDigit((char)digit))
                {
                    digit -= '0';
                    if ((i % 2) == parity)
                        digit *= 2;
                    foreach (char c in digit.ToString())
                        sum += c - '0';
                }
            }
            int mod = sum % 10;
            int check = mod == 0 ? 0 : 10 - mod;
            return check.ToString();
        }
    }
}
