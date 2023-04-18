namespace Moonlight.Protocol
{
    public static class Extensions
    {
        public static int GetVarLongLength(this long val)
        {
            int amount = 0;
            do
            {
                val >>= 7;
                amount++;
            } while (val != 0);

            return amount;
        }
    }
}
