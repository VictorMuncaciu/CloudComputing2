using ZodiacFinderGrpc;

namespace ZodiacFinder
{
    public class SignRange
    {
        public int StartDay;
        public int EndDay;
        public int StartMonth;
        public int EndMonth;
        public int SignIndex;

        public override string ToString()
        {
            return ((ZodiacSign) SignIndex).ToString() + " " + StartDay + "/" + StartMonth + " -> " + EndDay +
                   "/" + EndMonth;
        }
    }
}