using System;
using System.Globalization;
using Grpc.Core;
using ZodiacFinderGrpc;

namespace ZodiacClient
{
    class Program
    {
        private static bool IsDateValid(string date)
        {
            var dateFormats = new[] {"dd/MM/yyyy","dd/MM/yyy","dd/MM/yy","dd/MM/y"};
            Console.WriteLine(date);
            return DateTime.TryParseExact(
                date,
                dateFormats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None, 
                out _);
        }
        
        private static BirthDateRequest PromptBirthday()
        {
            Console.Write("Date of birth: ");
            var dateOfBirthStr = Console.ReadLine();
            if (dateOfBirthStr==null || !IsDateValid(dateOfBirthStr))
            {
                Console.WriteLine("Invalid date!");
                System.Environment.Exit(1);
            }

            var fields = dateOfBirthStr.Split('/');

            var bdr=new BirthDateRequest();

            bdr.Day = Int32.Parse(fields[0]);
            bdr.Mo = Int32.Parse(fields[1]);
            bdr.Year = Int32.Parse(fields[2]);
            
            return bdr;
        }
        
        public static void Main(string[] args)
        {
            Channel channel = new Channel("localhost:88888", ChannelCredentials.Insecure);

            var client = new ZodiacService.ZodiacServiceClient(channel);

            var birthRequest = PromptBirthday();

            var reply = client.findZodie(birthRequest);
            Console.WriteLine("Season: " + reply.Season + " || Sign: "+ reply.Sign);

            Console.ReadKey();
            channel.ShutdownAsync().Wait();
        }
    }
}